using System;
using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    [TestFixture]
    public class thresh
    {
        [Test]
        public void the_first_flag_of_a_check_counts()
        {
            var f = new gacf();

            f.add("speed", 1, 3f, 0.5f, 10f, 1f);

            Assert.That(f.cnt("speed"), Is.EqualTo(1), "the first flag must be recorded");
            Assert.That(f.ncf(), Is.EqualTo(1), "the first flag is live evidence");
            Assert.That(f.total(), Is.EqualTo(3), "the first flag must carry its weight");
            Assert.That(f.conf(), Is.GreaterThan(0f), "the first flag must register confidence");
        }

        [Test]
        public void every_flag_carries_its_weight()
        {
            var f = new gacf();

            for (int i = 0; i < 5; i++) f.add("speed", 1, 2f, 0.5f, 10f + i, 1f);

            Assert.That(f.cnt("speed"), Is.EqualTo(5));
            Assert.That(f.total(), Is.EqualTo(10));
        }

        [Test]
        public void the_weight_is_taken_from_the_check()
        {
            var f = new gacf();

            f.add("noclip", 1, 3f, 0.5f, 10f, 1f);
            f.add("noclip", 1, 3f, 0.5f, 11f, 1f);

            Assert.That(f.total(), Is.EqualTo(6), "two flags of a weight 3 check");
        }

        [Test]
        public void one_flag_moves_the_player_total_by_the_check_weight()
        {
            tst.boot();

            gacp.pun = false;

            gacd d = new gacd("z", 1);
            d.reset();
            d.grnd = true;
            d.push(Vector3.zero, Vector3.zero, gactime.now);

            gaccheck c = chk("noclip");

            Assert.That(c, Is.Not.Null, "the noclip check must be registered");

            int w = (int)gacr.wgt.of("noclip");

            Assert.That(w, Is.GreaterThan(0));

            gacr.flg(d, c, 0.5f, "t");

            Assert.That(gacr.flgof("z"), Is.EqualTo(w),
                "one flag must move the total by exactly the check weight");
            Assert.That(gacr.warnof("z"), Is.EqualTo(0), "one flag is below the warning threshold");
        }

        [Test]
        public void the_cooldown_stops_a_second_flag_in_the_same_instant()
        {
            tst.boot();

            gacp.pun = false;

            gacd d = new gacd("z", 1);
            d.reset();
            d.grnd = true;
            d.push(Vector3.zero, Vector3.zero, gactime.now);

            gaccheck c = chk("noclip");

            int w = (int)gacr.wgt.of("noclip");

            gacr.flg(d, c, 0.5f, "t");
            gacr.flg(d, c, 0.5f, "t");

            Assert.That(gacr.flgof("z"), Is.EqualTo(w), "the second call must be inside the cooldown");
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
    }

    [TestFixture]
    public class sensitivity
    {
        [SetUp]
        public void up()
        {
            tst.boot();
        }

        [Test]
        public void movement_just_under_the_limit_is_silent()
        {
            float lim = gacp.maxspeed * gacp.burst * 0.97f;

            tst.walk(30f, 0.05f, lim);

            Assert.That(tst.flags, Is.EqualTo(0), "movement under the burst limit must be silent: " + tst.What());
        }

        [Test]
        public void movement_just_over_the_limit_is_caught()
        {
            float lim = gacp.maxspeed * gacp.burst * 1.05f;

            tst.walk(30f, 0.05f, lim);

            Assert.That(tst.flags, Is.GreaterThan(0), "the speed check must still be sensitive");
        }

        [Test]
        public void the_threshold_shift_did_not_lower_the_bar()
        {
            gacp.maxspeed = 18f;

            tst.walk(30f, 0.05f, 17f);

            Assert.That(tst.flags, Is.EqualTo(0), "a normal run must stay silent: " + tst.What());
        }

        [Test]
        public void a_sprint_within_the_burst_allowance_is_silent()
        {
            tst.walk(2f, 0.05f, 22f);
            tst.walk(20f, 0.05f, 5f);

            Assert.That(tst.flags, Is.EqualTo(0), "the burst allowance must tolerate a sprint: " + tst.What());
        }

        [Test]
        public void every_legitimate_scenario_is_still_silent()
        {
            tst.legit();

            Assert.That(tst.flags, Is.EqualTo(0), "the threshold change introduced a false positive: " + tst.What());
        }

        [Test]
        public void bad_wifi_is_still_silent()
        {
            var rnd = new Random(3);

            float t = 100f;
            float x = 0f;
            int sq = 0;

            gactime.now = t;
            gactime.dt = 0.05f;

            for (int i = 0; i < 4000; i++)
            {
                t += 0.05f + ((float)rnd.NextDouble() - 0.5f) * 0.3f;
                x += 6f * 0.05f;
                sq++;

                gactime.now = t;
                gactime.dt = t;

                gac.upd2("c", new Vector3(x + ((float)rnd.NextDouble() - 0.5f) * 0.4f, 0f, 0f),
                    new Vector3(6f, 0f, 0f), true, sq, t);
            }

            Assert.That(tst.flags, Is.EqualTo(0),
                "the threshold change introduced a false positive on a bad link: " + tst.What());
        }

        [Test]
        public void a_frozen_link_is_still_silent()
        {
            gacp.strict = true;

            float t = 100f;
            int sq = 0;

            gactime.now = t;
            gactime.dt = 0.05f;

            for (int i = 0; i < 2000; i++)
            {
                t += 0.05f;
                sq++;

                gactime.now = t;
                gactime.dt = 0.05f;

                gac.upd2("c", new Vector3(0f, 0f, 0f), Vector3.zero, true, sq, t);
            }

            Assert.That(tst.flags, Is.EqualTo(0), "a stationary player must stay silent: " + tst.What());
        }

        [Test]
        public void a_real_exploit_still_escalates()
        {
            int peak = 0;
            int kicked = 0;

            gacs.onflag = e =>
            {
                tst.flags++;
                tst.names.Add(e.chk + ":" + e.rsn);

                int n = gacr.flgof(e.id);

                if (n > peak) peak = n;
            };

            gacs.onkick = e => kicked++;

            float t = 100f;
            float x = 0f;
            int sq = 0;

            gactime.now = t;
            gactime.dt = 0.05f;

            for (int i = 0; i < 2000; i++)
            {
                t += 0.05f;
                x += 60f * 0.05f;
                sq++;

                gactime.now = t;
                gactime.dt = 0.05f;

                gac.upd2("c", new Vector3(x, 0f, 0f), new Vector3(60f, 0f, 0f), true, sq, t);
            }

            Assert.That(tst.flags, Is.GreaterThan(0), "detection must not have been weakened");
            Assert.That(peak, Is.GreaterThanOrEqualTo(gacp.kth),
                "a sustained speedhack must reach the kick threshold, peak was " + peak);
            Assert.That(kicked, Is.GreaterThan(0), "a sustained speedhack must be kicked");
        }
    }
}