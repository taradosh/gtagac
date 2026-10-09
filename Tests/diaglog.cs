using System;
using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    [TestFixture]
    public class diaglog
    {
        [SetUp]
        public void up()
        {
            tst.boot();
            gaclog.clr();
        }

        static void net(string id, float seconds, float dt, float spd, int seed, bool cheat)
        {
            var rnd = new Random(seed);

            float now = 100f;
            float x = 0f;
            int seq = 0;
            bool cheatin = false;

            gactime.now = now;
            gactime.dt = dt;
            seq++;
            gac.upd2(id, new Vector3(x, 0f, 0f), Vector3.zero, true, seq, now);

            int n = (int)(seconds / dt);

            for (int i = 0; i < n; i++)
            {
                now += dt;

                x += spd * dt;

                if (cheat && i == 40) { x += 40f; cheatin = true; }

                seq++;
                gactime.now = now;
                gactime.dt = dt;
                gac.upd2(id, new Vector3(x, 0f, 0f), cheatin ? new Vector3(60f, 0f, 0f) : Vector3.zero, true, seq, now);
            }
        }

        static bool has(string lvl)
        {
            for (int i = 0; i < gaclog.kept; i++) if (gaclog.at(i).lvl == lvl) return true;

            return false;
        }

        static bool hasid(string id)
        {
            for (int i = 0; i < gaclog.kept; i++) if (gaclog.at(i).id == id) return true;

            return false;
        }

        static int count(string lvl, string id)
        {
            int n = 0;

            for (int i = 0; i < gaclog.kept; i++)
            {
                gacl e = gaclog.at(i);

                if (e.lvl == lvl && (id == null || e.id == id)) n++;
            }

            return n;
        }

        [Test]
        public void flags_are_recorded()
        {
            net("c", 1f, 0.05f, 120f, 0, false);

            Assert.That(has(gaclog.lflag), Is.True, "an exploited player must leave a record");
        }

        [Test]
        public void the_record_carries_the_measurement()
        {
            net("c", 1f, 0.05f, 120f, 0, false);

            bool ok = false;

            for (int i = 0; i < gaclog.kept; i++)
            {
                gacl e = gaclog.at(i);

                if (e.lvl != gaclog.lflag) continue;

                ok |= e.chk == "speed" && e.rsn != null && e.rsn.Length > 0
                    && e.cf > 0f && e.cf <= 1f && e.tot > 0 && e.id == "c";
            }

            Assert.That(ok, Is.True, "the record must name the check, the measured value, confidence and total");
        }

        [Test]
        public void warnings_and_kicks_are_never_rate_limited_away()
        {
            gacp.loglim = 1f;
            gacp.logwin = 600f;

            net("c", 10f, 0.05f, 120f, 0, false);

            Assert.That(has(gaclog.lwarn), Is.True, "a warning must always be recorded");
            Assert.That(has(gaclog.lkick), Is.True, "a kick must always be recorded");
        }

        [Test]
        public void spam_from_one_player_is_rate_limited()
        {
            gacp.loglim = 2f;
            gacp.logwin = 600f;

            net("c", 10f, 0.05f, 120f, 0, false);

            Assert.That(count(gaclog.lflag, null), Is.LessThanOrEqualTo(2),
                "flag records must be capped at " + gacp.loglim + ", got " + count(gaclog.lflag, null));
        }

        [Test]
        public void the_window_reopens()
        {
            gacp.loglim = 2f;
            gacp.logwin = 0.5f;

            net("c", 5f, 0.05f, 120f, 0, false);

            Assert.That(count(gaclog.lflag, null), Is.GreaterThan(2),
                "the window must reopen, got " + count(gaclog.lflag, null));
        }

        [Test]
        public void the_limit_is_per_player()
        {
            gacp.loglim = 1f;
            gacp.logwin = 600f;

            net("c", 5f, 0.05f, 120f, 0, false);
            net("d", 5f, 0.05f, 120f, 0, false);

            Assert.That(count(gaclog.lflag, "c"), Is.EqualTo(1), "player c records");
            Assert.That(count(gaclog.lflag, "d"), Is.EqualTo(1), "player d must not be starved by player c");
        }

        [Test]
        public void network_rejections_are_recorded()
        {
            net("c", 1f, 0.05f, 2f, 0, false);

            Assert.That(has(gaclog.lnet), Is.False, "a clean stream must be silent");

            gaclog.clr();

            gacp.strict = true;

            gactime.now = 200f;
            gacp.logn = true;

            gac.upd2("r", Vector3.zero, Vector3.zero, true, 1, 200.0);
            gac.upd2("r", Vector3.zero, Vector3.zero, true, 1, 200.0);

            Assert.That(has(gaclog.lnet), Is.True, "a replay must leave a record");
            Assert.That(hasid("r"), Is.True, "the record must name the player");
        }

        [Test]
        public void the_ring_never_overflows()
        {
            gacp.logkeep = 16;
            gacp.loglim = 10000f;
            gacp.logwin = 0.0001f;

            net("c", 10f, 0.05f, 120f, 0, false);

            Assert.That(gaclog.kept, Is.LessThanOrEqualTo(16), "the ring must stay bounded");
            Assert.That(gaclog.lost, Is.GreaterThan(0), "dropped records must be counted");
        }

        [Test]
        public void logging_can_be_switched_off()
        {
            gacp.logon = false;

            net("c", 5f, 0.05f, 120f, 0, false);

            Assert.That(gaclog.kept, Is.EqualTo(0), "logon=false must record nothing");
        }

        [Test]
        public void text_export_is_newest_first()
        {
            net("c", 1f, 0.05f, 120f, 0, false);

            string t = gacdiag.txt();

            Assert.That(t, Does.Contain("gtagac diag v"));
            Assert.That(t, Does.Contain("ply c"));
            Assert.That(t, Does.Contain("cfg "));

            float a = gaclog.at(0).t;
            float b = gaclog.at(gaclog.kept - 1).t;

            Assert.That(a, Is.GreaterThanOrEqualTo(b), "newest first");
        }

        [Test]
        public void text_export_carries_network_state()
        {
            net("c", 1f, 0.05f, 3f, 0, false);

            string t = gacdiag.txt();

            Assert.That(t, Does.Contain("net seq "));
            Assert.That(t, Does.Contain("jit "));
            Assert.That(t, Does.Contain("slack "));
        }

        [Test]
        public void text_export_carries_the_per_check_breakdown()
        {
            net("c", 1f, 0.05f, 120f, 0, false);

            Assert.That(gacdiag.txt(), Does.Contain("speed="));
            Assert.That(gacdiag.txt(), Does.Contain("chk "));
        }

        [Test]
        public void csv_export_is_well_formed()
        {
            net("c", 1f, 0.05f, 120f, 0, false);

            string t = gacdiag.csv();
            string[] ln = t.TrimEnd('\n').Split('\n');

            Assert.That(ln[0], Is.EqualTo("t,level,id,check,confidence,flags,reason"));
            Assert.That(ln.Length, Is.EqualTo(gaclog.kept + 1));

            foreach (string l in ln)
            {
                Assert.That(l, Does.Contain(","));
                Assert.That(l.Split(',').Length, Is.GreaterThanOrEqualTo(7));
            }
        }

        [Test]
        public void the_report_survives_an_empty_state()
        {
            gaclog.clr();

            Assert.That(gacdiag.txt(), Does.Contain("gtagac diag v"));
            Assert.That(gacdiag.csv(), Does.StartWith("t,level"));
        }

        [Test]
        public void numbers_are_culture_independent()
        {
            var ci = System.Globalization.CultureInfo.CurrentCulture;

            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("ru-RU");

                net("c", 1f, 0.05f, 120f, 0, false);

                string t = gacdiag.txt();
                string c = gacdiag.csv();

                Assert.That(t, Does.Not.Contain(","), "the text report must use a dot: " + t);

                foreach (string l in c.TrimEnd('\n').Split('\n'))
                {
                    if (l.StartsWith("t,")) continue;

                    string[] f = l.Split(',');

                    if (!float.TryParse(f[0], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float _))
                    {
                        Assert.Fail("the timestamp must parse as a dot decimal: " + l);
                    }

                    if (!float.TryParse(f[4], System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture, out float _))
                    {
                        Assert.Fail("the confidence must parse as a dot decimal: " + l);
                    }
                }

                Assert.That(t, Does.Contain("speed="));
            }
            finally
            {
                System.Globalization.CultureInfo.CurrentCulture = ci;
            }
        }

        [Test]
        public void a_record_survives_a_save_failure()
        {
            Assert.That(gacdiag.save("Z:\\nope\\nope\\x.txt"), Is.False);
        }
    }
}