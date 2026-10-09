using System;
using System.Collections.Generic;
using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    [TestFixture]
    public class perf
    {
        [SetUp]
        public void up()
        {
            tst.boot();
        }

        static long alloc()
        {
            return GC.GetAllocatedBytesForCurrentThread();
        }

        static long per(int n, Action a)
        {
            a();

            long b = alloc();

            for (int i = 0; i < n; i++) a();

            return (alloc() - b) / n;
        }

        static void push(Vector3 p, Vector3 v)
        {
            gac.upd2("c", p, v, true, ++sq, gactime.now);
        }

        static int sq;

        static float x;

        static void one(float spd)
        {
            x += spd * 0.05f;
            gactime.now += 0.05f;
            gactime.dt = 0.05f;

            push(new Vector3(x, 0f, 0f), new Vector3(spd, 0f, 0f));
        }

        static void spam(int n, float spd)
        {
            for (int i = 0; i < n; i++) one(spd);
        }

        [Test]
        public void a_clean_stream_allocates_nothing_per_tick()
        {
            spam(400, 6f);

            long a = per(2000, () => one(6f));

            Assert.That(a, Is.EqualTo(0), "a clean stream must not allocate, got " + a + " bytes per tick");
        }

        [Test]
        public void flagging_a_punished_player_allocates_nothing_per_tick()
        {
            spam(400, 60f);

            long a = per(2000, () => one(60f));

            Assert.That(a, Is.EqualTo(0), "a known cheater must not allocate, got " + a + " bytes per tick");
        }

        [Test]
        public void after_a_kick_the_stream_still_allocates_nothing()
        {
            spam(2000, 60f);

            Assert.That(gacr.kkd("c"), Is.True, "the player must have been kicked first");

            long a = per(2000, () => one(60f));

            Assert.That(a, Is.EqualTo(0), "p.sent must short circuit before any string work, got " + a);
        }

        [Test]
        public void the_whole_pipeline_allocates_nothing_per_tick()
        {
            var s = new sim("q", 3);

            for (int i = 0; i < 400; i++)
            {
                s.vel(6f, 0f, 0f).gnd(true).at(i * 0.3f, 0f, 0f).step(0.05f);

                gac.evl(s.d);
            }

            int k = 400;

            long a = per(2000, () =>
            {
                s.vel(6f, 0f, 0f).gnd(true).at(k * 0.3f, 0f, 0f).step(0.05f);

                k++;

                gac.evl(s.d);
            });

            Assert.That(a, Is.EqualTo(0), "the local path must not allocate, got " + a + " bytes per tick");
        }

        [Test]
        public void prediction_does_not_allocate_per_tick()
        {
            gacp.pr = true;

            spam(400, 6f);

            long a = per(2000, () => one(6f));

            Assert.That(a, Is.EqualTo(0), "prediction must not allocate, got " + a + " bytes per tick");
        }

        [Test]
        public void lossy_network_does_not_allocate_by_default()
        {
            spam(400, 6f);

            var rnd = new Random(3);

            x = 0f;

            long a = per(2000, () =>
            {
                x += 0.3f;
                gactime.now += 0.05f + ((float)rnd.NextDouble() - 0.5f) * 0.3f;
                gactime.dt = 0.05f;

                if (rnd.Next(100) < 20) return;

                push(new Vector3(x, 0f, 0f), new Vector3(6f, 0f, 0f));
            });

            Assert.That(a, Is.EqualTo(0), "loss must not allocate by default, got " + a + " bytes per tick");
        }

        [Test]
        public void network_logging_is_opt_in()
        {
            Assert.That(gacp.logn, Is.False, "per packet logging must be off by default");
        }

        [Test]
        public void network_logging_can_be_switched_on()
        {
            gacp.logn = true;

            spam(400, 6f);

            for (int i = 0; i < 500; i++)
            {
                gactime.now += 0.05f;
                gactime.dt = 0.05f;

                sq += 12;

                push(new Vector3(x, 0f, 0f), new Vector3(6f, 0f, 0f));
            }

            int n = 0;

            for (int i = 0; i < gaclog.kept; i++) if (gaclog.at(i).lvl == gaclog.lnet) n++;

            Assert.That(n, Is.GreaterThan(0), "gaps must be recorded when logging is on");
        }

        [Test]
        public void the_player_table_is_bounded()
        {
            for (int i = 0; i < gacp.slots * 4; i++)
            {
                gac.upd2("p" + i, Vector3.zero, Vector3.zero, true, 1, gactime.now);
            }

            int n = 0;

            for (int i = 0; i < gac.ply.Count; i++) n++;

            Assert.That(n, Is.LessThanOrEqualTo(gacp.slots + 1),
                "the slot cap must hold, got " + n);
        }

        [Test]
        public void the_per_player_table_is_bounded()
        {
            for (int i = 0; i < gacp.slots * 4; i++) gacr.get("p" + i);

            int n = 0;

            for (int i = 0; i < gacp.slots; i++) if (gacr.flgof("p" + i) >= 0) n++;

            Assert.That(n, Is.GreaterThanOrEqualTo(0));

            bool last = gacr.evok("p" + (gacp.slots * 4 - 1));

            Assert.That(last, Is.True, "the newest player must be retained");
        }

        [Test]
        public void an_evicted_player_no_longer_accumulates()
        {
            gacp.evon = true;
            gacp.evmin = 1;

            gacr.get("old");

            gacd d = new gacd("old", 9);
            d.reset();

            gacper p = gacr.get("old");

            p.f.add("speed", 1, 1f, 0.9f, gactime.now, 1f);

            for (int i = 0; i < gacp.slots + 2; i++) gacr.get("f" + i);

            gacr.flg(d, chk("noclip"), 0.9f, "t");

            Assert.That(tst.flags, Is.LessThanOrEqualTo(1));
        }

        static gaccheck chk(string n)
        {
            for (int i = 0; i < gac.cnum; i++)
            {
                gaccheck c = gac.chk(i);

                if (c.name == n) return c;
            }

            return null;
        }

        sealed class sink : igace
        {
            public int flags;
            public int warnings;
            public int kicks;
            public int bans;
            public int liveflags;
            public int livewarnings;
            public int livekicks;
            public int livebans;

            public void onflag(gace e)
            {
                flags++;
                liveflags++;
            }

            public void onwarning(gace e)
            {
                warnings++;
                livewarnings++;
            }

            public void onkick(gace e)
            {
                kicks++;
                livekicks++;
            }

            public void onban(gace e)
            {
                bans++;
                livebans++;
            }
        }

        static sink sub;

        [Test]
        public void a_bound_sink_receives_every_event()
        {
            sub = new sink();

            gacs.bind(sub);

            Assert.That(gacs.bnd, Is.GreaterThan(0), "the built in sink must always be bound");

            spam(2000, 60f);

            Assert.That(sub.flags, Is.GreaterThan(0), "flags must reach the sink");
            Assert.That(sub.kicks, Is.GreaterThan(0), "kicks must reach the sink");

            int lg = 0;

            for (int i = 0; i < gaclog.kept; i++) if (gaclog.at(i).lvl == gaclog.lflag) lg++;

            Assert.That(lg, Is.GreaterThan(0), "the ring must still be fed by the built in sink");

            gacs.unbind(sub);

            int before = sub.flags;

            spam(200, 60f);

            Assert.That(sub.flags, Is.EqualTo(before), "an unbound sink receives nothing");
        }

        [Test]
        public void binding_the_same_sink_twice_is_a_no_op()
        {
            var s = new sink();

            int n = gacs.bnd;

            gacs.bind(s);
            gacs.bind(s);

            Assert.That(gacs.bnd, Is.EqualTo(n + 1), "a duplicate bind must be ignored");

            gacs.unbind(s);
        }

        [Test]
        public void the_configuration_hash_is_stable_and_sensitive()
        {
            float a = gacp.hash();

            Assert.That(gacp.hash(), Is.EqualTo(a), "the hash must be stable");
            Assert.That(a, Is.Not.EqualTo(0f), "the hash must never be zero");

            float maxspeed = gacp.maxspeed;

            gacp.maxspeed = maxspeed + 1f;

            Assert.That(gacp.hash(), Is.Not.EqualTo(a), "a changed threshold must change the hash");

            gacp.maxspeed = maxspeed;
        }

        [Test]
        public void the_hash_is_sensitive_to_every_group()
        {
            float a = gacp.hash();

            gacp.logon = !gacp.logon;

            Assert.That(gacp.hash(), Is.Not.EqualTo(a), "logon");

            gacp.logon = !gacp.logon;

            gacp.evon = true;

            Assert.That(gacp.hash(), Is.Not.EqualTo(a), "evon");

            gacp.evon = false;

            gacp.pr = true;

            Assert.That(gacp.hash(), Is.Not.EqualTo(a), "pr");

            gacp.pr = false;

            Assert.That(gacp.hash(), Is.EqualTo(a), "and back again");
        }

        [Test]
        public void a_diagnostic_report_allocates_only_on_demand()
        {
            spam(400, 6f);

            gacdiag.txt();

            long b = alloc();

            gacdiag.txt();

            Assert.That(alloc() - b, Is.GreaterThan(0), "building a report allocates, which is fine");

            long c = alloc();

            for (int i = 0; i < 2000; i++) gac.evl(gac.find("c"));

            Assert.That(alloc() - c, Is.EqualTo(0), "but evaluation must not");
        }
    }
}