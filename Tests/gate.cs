using System;
using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    [TestFixture]
    public class gate
    {
        int warned;
        int kicked;
        int banned;
        int peak;

        float clock;
        int sq;

        [SetUp]
        public void up()
        {
            tst.boot();

            warned = 0;
            kicked = 0;
            banned = 0;
            peak = 0;

            clock = 100f;
            sq = 0;

            gacs.onwarning = e => warned++;
            gacs.onkick = e => kicked++;
            gacs.onban = e => banned++;

            gacs.onflag = e =>
            {
                tst.flags++;
                tst.names.Add(e.chk + ":" + e.rsn);

                int n = gacr.evind(e.id);

                if (n > peak) peak = n;
            };
        }

        [TearDown]
        public void dn()
        {
            world.off();
            gacp.srv = false;
        }

        void put(string id, float x, Vector3 v, float dt)
        {
            clock += dt;

            gactime.now = clock;
            gactime.dt = dt;

            sq++;

            gac.upd2(id, new Vector3(x, 0f, 0f), v, true, sq, clock);
        }

        void spoofed(string id, float spd, float seconds, float dt, bool tell)
        {
            Vector3 v = tell ? new Vector3(spd, 0f, 0f) : Vector3.zero;

            put(id, 0f, v, dt);

            int n = (int)(seconds / dt);

            for (int i = 1; i <= n; i++) put(id, i * spd * dt, v, dt);
        }

        void hop(string id, float seconds, float dt)
        {
            put(id, 0f, new Vector3(3f, 0f, 0f), dt);

            float x = 0f;
            int n = (int)(seconds / dt);

            for (int i = 1; i <= n; i++)
            {
                x += 3f * dt;

                if (i % 10 == 0) x += 30f;

                put(id, x, new Vector3(3f, 0f, 0f), dt);
            }
        }

        void walls(string id, float seconds, float dt)
        {
            gacp.srv = true;

            world.wx = 4.2f;
            world.wall();

            float w = 2f * (float)Math.PI / 2f;

            put(id, 3.6f, Vector3.zero, dt);

            int n = (int)(seconds / dt);

            for (int i = 1; i <= n; i++)
            {
                float t = i * dt;

                float x = 3.6f + 1.2f * (float)Math.Sin(w * t);
                float v = 1.2f * w * (float)Math.Cos(w * t);

                put(id, x, new Vector3(v, 0f, 0f), dt);
            }
        }

        [Test]
        public void the_gate_is_off_by_default()
        {
            Assert.That(gacp.evon, Is.False, "the gate must be opt in for backwards compatibility");
            Assert.That(gacp.evmin, Is.EqualTo(2f));
        }

        [Test]
        public void with_the_gate_off_a_speedhack_is_still_kicked()
        {
            spoofed("c", 60f, 40f, 0.05f, true);

            Assert.That(kicked, Is.GreaterThan(0), "the legacy path must keep punishing");
        }

        [Test]
        public void with_the_gate_on_a_speedhack_is_still_kicked()
        {
            gacp.evon = true;

            spoofed("c", 60f, 40f, 0.05f, true);

            Assert.That(tst.flags, Is.GreaterThan(0), "detection must not be suppressed");
            Assert.That(peak, Is.GreaterThanOrEqualTo(2),
                "a consistent speedhack trips speed and velocity, which is two independent checks");
            Assert.That(kicked, Is.GreaterThan(0), "the gate must not neuter the common exploit");
        }

        [Test]
        public void with_the_gate_on_a_single_check_exploit_is_flagged_but_not_punished()
        {
            gacp.evon = true;

            walls("c", 120f, 0.05f);

            Assert.That(tst.flags, Is.GreaterThan(0), "detection must not be suppressed");
            Assert.That(peak, Is.EqualTo(1), "wall crossing is one independent check");
            Assert.That(warned, Is.GreaterThan(0), "a warning is informational and must still fire");
            Assert.That(kicked, Is.EqualTo(0), "one independent check must not be enough to kick");
            Assert.That(banned, Is.EqualTo(0), "one independent check must not be enough to ban");
        }

        [Test]
        public void repeated_flags_of_one_check_are_not_independent()
        {
            gacp.evon = true;

            walls("c", 120f, 0.05f);

            Assert.That(tst.flags, Is.GreaterThan(10), "one check fired many times");
            Assert.That(peak, Is.EqualTo(1), "the same check is not independent evidence");
        }

        [Test]
        public void a_second_check_lets_the_punishment_through()
        {
            gacp.evon = true;
            gacp.evmin = 2f;

            walls("c", 120f, 0.05f);
            spoofed("c", 60f, 40f, 0.05f, true);

            Assert.That(peak, Is.GreaterThanOrEqualTo(2), "two distinct checks must count");
            Assert.That(kicked, Is.GreaterThan(0), "two independent checks must punish");
        }

        [Test]
        public void a_repeated_teleport_passes_the_gate()
        {
            gacp.evon = true;
            gacp.evmin = 2f;

            hop("c", 40f, 0.05f);

            Assert.That(peak, Is.GreaterThanOrEqualTo(2), "a teleport trips several checks");
            Assert.That(kicked, Is.GreaterThan(0), "a teleport must still be punished");
        }

        [Test]
        public void the_window_closes_older_evidence()
        {
            gacp.evon = true;
            gacp.evwin = 5f;
            gacp.pun = false;

            walls("c", 30f, 0.05f);

            Assert.That(gacr.evind("c"), Is.GreaterThan(0), "the wall check is recent");
            Assert.That(gacr.evok("c"), Is.False, "one check is below evmin");

            gactime.now += 60f;

            Assert.That(gacr.evind("c"), Is.EqualTo(0), "old evidence must leave the window");
            Assert.That(gacr.evok("c"), Is.False, "the gate must close with it");
        }

        [Test]
        public void the_gate_applies_to_bans_too()
        {
            gacp.evon = true;
            gacp.evmin = 5f;
            gacp.bth = 5f;

            walls("c", 120f, 0.05f);

            Assert.That(banned, Is.EqualTo(0), "a single check must not ban");
        }

        [Test]
        public void a_satisfied_gate_lets_a_ban_through()
        {
            gacp.evon = true;
            gacp.evmin = 2f;
            gacp.bth = 5f;

            walls("c", 120f, 0.05f);
            spoofed("c", 60f, 40f, 0.05f, true);

            Assert.That(banned, Is.GreaterThan(0), "satisfied evidence must ban");
        }

        [Test]
        public void a_blocked_punishment_is_recorded()
        {
            gacp.evon = true;
            gacp.evmin = 5f;

            gaclog.clr();

            walls("c", 120f, 0.05f);

            int n = 0;

            for (int i = 0; i < gaclog.kept; i++) if (gaclog.at(i).lvl == gaclog.lgate) n++;

            Assert.That(n, Is.GreaterThan(0), "an operator must be able to see that a punishment was held back");
        }

        [Test]
        public void a_blocked_punishment_is_retried_when_evidence_arrives()
        {
            gacp.evon = true;

            walls("c", 120f, 0.05f);

            Assert.That(kicked, Is.EqualTo(0), "held back with one check");

            spoofed("c", 60f, 40f, 0.05f, true);

            Assert.That(kicked, Is.GreaterThan(0), "the gate must not block forever once evidence arrives");
        }

        [Test]
        public void the_gate_is_per_player()
        {
            gacp.evon = true;

            walls("c", 120f, 0.05f);

            Assert.That(gacr.evok("c"), Is.False);
            Assert.That(gacr.evind("other"), Is.EqualTo(0), "an unknown player has no evidence");
            Assert.That(gacr.evok("other"), Is.False);
        }

        [Test]
        public void punishments_can_be_disabled_entirely()
        {
            gacp.evon = true;
            gacp.pun = false;

            spoofed("c", 60f, 40f, 0.05f, true);

            Assert.That(kicked, Is.EqualTo(0), "pun=false must win over the gate");
            Assert.That(banned, Is.EqualTo(0));
        }

        [Test]
        public void a_clean_player_passes_every_legit_scenario_with_the_gate_on()
        {
            gacp.evon = true;

            tst.legit();

            Assert.That(tst.flags, Is.EqualTo(0), "no legitimate scenario may flag: " + tst.What());
            Assert.That(kicked, Is.EqualTo(0));
            Assert.That(banned, Is.EqualTo(0));
        }

        [Test]
        public void bad_network_conditions_do_not_satisfy_the_gate()
        {
            gacp.evon = true;
            gacp.evmin = 2f;

            var rnd = new Random(5);

            gactime.now = clock;
            gactime.dt = 0.05f;

            float x = 0f;
            float t = clock;

            for (int i = 0; i < 3000; i++)
            {
                t += 0.05f + ((float)rnd.NextDouble() - 0.5f) * 0.3f;
                x += 6f * 0.05f;
                sq++;
                gactime.now = t;
                gactime.dt = t;
                gac.upd2("c", new Vector3(x + ((float)rnd.NextDouble() - 0.5f) * 0.4f, 0f, 0f),
                    new Vector3(6f, 0f, 0f), true, sq, t);
            }

            clock = t;

            Assert.That(tst.flags, Is.EqualTo(0), "a bad wifi player must not be punished: " + tst.What());
            Assert.That(kicked, Is.EqualTo(0));
            Assert.That(banned, Is.EqualTo(0));
        }

        [Test]
        public void the_report_hides_the_gate_when_off()
        {
            spoofed("c", 60f, 40f, 0.05f, true);

            Assert.That(gacdiag.txt(), Does.Not.Contain("gate "), "the gate block is hidden when it is off");
        }

        [Test]
        public void the_report_shows_the_gate_when_on()
        {
            gacp.evon = true;

            spoofed("c", 60f, 40f, 0.05f, true);

            string t = gacdiag.txt();

            Assert.That(t, Does.Contain("gate "));
            Assert.That(t, Does.Contain("ind "));
        }
    }
}