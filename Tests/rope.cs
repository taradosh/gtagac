using System;
using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    [TestFixture]
    public class rope
    {
        [SetUp]
        public void up()
        {
            tst.boot();
        }

        static void swing(link l, float seconds, float dt, float len, float amp, float period)
        {
            float w = 2f * (float)Math.PI / period;

            int n = (int)(seconds / dt);

            for (int i = 0; i <= n; i++)
            {
                float t = i * dt;
                float a = amp * (float)Math.Sin(w * t);

                float x = len * (float)Math.Sin(a);
                float y = -len * (float)Math.Cos(a);

                float vx = len * (float)Math.Cos(a) * amp * w * (float)Math.Cos(w * t);
                float vy = len * (float)Math.Sin(a) * amp * w * (float)Math.Cos(w * t);

                l.t += dt;

                l.put(new Vector3(x, y, 0f), new Vector3(vx, vy, 0f));
            }
        }

        static float floorp(float len, float amp)
        {
            return 2f * (float)Math.PI /
                (float)Math.Sqrt(2f * gacp.grav * (1f - (float)Math.Cos(amp)) / (len * amp * amp));
        }

        static void drop(link l, float seconds, float dt, float len)
        {
            int n = (int)(seconds / dt);

            float y = 0f;
            float vy = 0f;

            for (int i = 0; i < n; i++)
            {
                vy -= gacp.grav * dt;
                y += vy * dt;

                if (y < -len) { y = -len; vy = 0f; }

                l.t += dt;

                l.put(new Vector3(0f, y, 0f), new Vector3(0f, vy, 0f));
            }
        }

        static void runSwing(float len, float amp)
        {
            var l = new link("p", new Vector3(0f, -len, 0f), Vector3.zero);

            swing(l, 40f, 0.05f, len, amp, floorp(len, amp));

            Assert.That(tst.flags, Is.EqualTo(0),
                "a " + len + " m rope at " + amp + " rad must stay silent: " + tst.What());
        }

        [Test]
        public void the_default_limit_is_eighteen()
        {
            Assert.That(gacp.maxspeed, Is.EqualTo(18f), "the documented default must match the code");
        }

        [Test]
        public void the_default_covers_the_fastest_physical_swing()
        {
            float worst = 0f;

            foreach (float len in new float[] { 6f, 8f, 10f, 12f, 16f })
            {
                foreach (float amp in new float[] { 1f, 1.2f, 1.4f })
                {
                    float w = 2f * (float)Math.PI / floorp(len, amp);

                    if (len * amp * w > worst) worst = len * amp * w;
                }
            }

            Assert.That(worst, Is.LessThan(gacp.maxspeed),
                "the fastest energy consistent swing peaks at " + worst + " m over s, the limit is " + gacp.maxspeed);
        }

        [Test]
        public void a_walk_at_the_limit_is_silent()
        {
            tst.walk(40f, 0.05f, gacp.maxspeed * gacp.burst * 0.95f);

            Assert.That(tst.flags, Is.EqualTo(0), "walking at the burst ceiling must be silent: " + tst.What());
        }

        [Test]
        public void a_six_metre_rope_is_silent()
        {
            runSwing(6f, 1f);
        }

        [Test]
        public void an_eight_metre_rope_is_silent()
        {
            runSwing(8f, 1.2f);
        }

        [Test]
        public void a_ten_metre_rope_is_silent()
        {
            runSwing(10f, 1.2f);
        }

        [Test]
        public void a_twelve_metre_rope_is_silent()
        {
            runSwing(12f, 1.4f);
        }

        [Test]
        public void a_sixteen_metre_rope_is_silent()
        {
            runSwing(16f, 1.4f);
        }

        [Test]
        public void a_wide_arc_is_silent()
        {
            var l = new link("p", new Vector3(0f, -8f, 0f), Vector3.zero);

            swing(l, 40f, 0.05f, 8f, 1.4f, floorp(8f, 1.4f) * 1.1f);

            Assert.That(tst.flags, Is.EqualTo(0), "a wider arc at a slower period must stay silent: " + tst.What());
        }

        [Test]
        public void a_pumped_swing_within_the_tolerance_is_silent()
        {
            var l = new link("p", new Vector3(0f, -8f, 0f), Vector3.zero);

            swing(l, 40f, 0.05f, 8f, 1.2f, floorp(8f, 1.2f) * 0.62f);

            Assert.That(tst.flags, Is.EqualTo(0),
                "a pumped swing under the burst ceiling must stay silent: " + tst.What());
        }

        [Test]
        public void a_swing_while_grappling_is_silent()
        {
            var s = new sim("p");

            float w = 2f * (float)Math.PI / floorp(8f, 1.2f);

            for (int i = 0; i <= 800; i++)
            {
                float t = i * 0.05f;
                float a = 1.2f * (float)Math.Sin(w * t);

                float x = 8f * (float)Math.Sin(a);
                float y = -8f * (float)Math.Cos(a);

                float vx = 8f * (float)Math.Cos(a) * 1.2f * w * (float)Math.Cos(w * t);
                float vy = 8f * (float)Math.Sin(a) * 1.2f * w * (float)Math.Cos(w * t);

                s.vel(vx, vy, 0f).at(x, y, 0f).st(false, false, false, false, true).step(0.05f);

                gac.evl(s.d);
            }

            Assert.That(tst.flags, Is.EqualTo(0), "a swinging grapple must stay silent: " + tst.What());
        }

        [Test]
        public void hanging_still_on_a_rope_is_silent()
        {
            var s = new sim("p");

            for (int i = 0; i < 600; i++)
            {
                s.vel(0f, -20f, 0f).at(0f, -8f, 0f).st(false, false, true, false, false).step(0.05f);

                gac.evl(s.d);
            }

            Assert.That(tst.flags, Is.EqualTo(0), "hanging on a rope must stay silent: " + tst.What());
        }

        [Test]
        public void a_rope_drop_is_silent()
        {
            var l = new link("p");

            drop(l, 30f, 0.05f, 8f);

            Assert.That(tst.flags, Is.EqualTo(0), "falling down a rope must stay silent: " + tst.What());
        }

        [Test]
        public void consecutive_rope_uses_are_silent()
        {
            var l = new link("p", new Vector3(0f, -6f, 0f), Vector3.zero);

            swing(l, 25f, 0.05f, 6f, 1f, floorp(6f, 1f));

            gac.grace("p", 2f);

            l.t += 1f;

            l.put(new Vector3(0f, -12f, 0f), new Vector3(0f, 0f, 0f));

            swing(l, 25f, 0.05f, 12f, 1.4f, floorp(12f, 1.4f));

            gac.grace("p", 2f);

            l.t += 1f;

            l.put(new Vector3(0f, -16f, 0f), new Vector3(0f, 0f, 0f));

            drop(l, 20f, 0.05f, 16f);

            Assert.That(tst.flags, Is.EqualTo(0),
                "several rope uses in one session must stay silent: " + tst.What());
        }

        [Test]
        public void rope_use_on_a_bad_link_is_silent()
        {
            var l = new link("p", new Vector3(0f, -8f, 0f), Vector3.zero);

            var rnd = new Random(4);

            float w = 2f * (float)Math.PI / floorp(8f, 1.2f);

            for (int i = 0; i <= 2000; i++)
            {
                float t = i * 0.05f;
                float a = 1.2f * (float)Math.Sin(w * t);

                float x = 8f * (float)Math.Sin(a);
                float y = -8f * (float)Math.Cos(a);

                float vx = 8f * (float)Math.Cos(a) * 1.2f * w * (float)Math.Cos(w * t);
                float vy = 8f * (float)Math.Sin(a) * 1.2f * w * (float)Math.Cos(w * t);

                l.t += 0.05f + (float)(rnd.NextDouble() - 0.5) * 0.3f;

                if (rnd.Next(100) < 20) continue;

                l.put(new Vector3(x, y, 0f), new Vector3(vx, vy, 0f));
            }

            Assert.That(tst.flags, Is.EqualTo(0), "a swing over a bad link must stay silent: " + tst.What());
        }

        [Test]
        public void a_speedhack_is_still_caught_at_the_raised_limit()
        {
            walk2(60f, 60f);

            Assert.That(tst.flags, Is.GreaterThan(0), "a real speedhack must still be caught: " + tst.What());
        }

        [Test]
        public void a_speedhack_still_escalates_to_a_kick()
        {
            int kicked = 0;

            gacs.onkick = e => kicked++;

            walk2(120f, 60f);

            Assert.That(kicked, Is.GreaterThan(0), "a sustained speedhack must still be kicked");
        }

        [Test]
        public void a_speedhack_still_escalates_to_a_ban()
        {
            int banned = 0;

            gacs.onban = e => banned++;

            gacp.pkk = false;

            walk2(120f, 60f);

            Assert.That(banned, Is.GreaterThan(0), "a sustained speedhack must still be bannable");
        }

        [Test]
        public void a_speedhack_is_caught_even_with_a_consistent_velocity()
        {
            walk2(60f, 60f);

            int n = 0;

            foreach (string s in tst.names) if (s.StartsWith("speed:")) n++;

            Assert.That(n, Is.GreaterThan(0), "the speed check must fire even when the velocity matches");
        }

        [Test]
        public void a_moderate_speedhack_is_caught()
        {
            walk2(60f, 27f);

            Assert.That(tst.flags, Is.GreaterThan(0),
                "27 m over s is above the burst ceiling of " + gacp.maxspeed * gacp.burst);
        }

        [Test]
        public void movement_just_under_the_burst_ceiling_is_not_caught()
        {
            walk2(60f, gacp.maxspeed * gacp.burst * 0.95f);

            Assert.That(tst.flags, Is.EqualTo(0),
                "this is the documented tolerance, not a detection gap: " + tst.What());
        }

        [Test]
        public void a_teleport_is_still_caught()
        {
            var l = new link("p");

            for (int i = 0; i < 600; i++)
            {
                float x = i * 0.15f;

                if (i % 40 == 0) x += 25f;

                l.t += 0.05f;

                l.put(new Vector3(x, 0f, 0f), new Vector3(3f, 0f, 0f));
            }

            Assert.That(tst.flags, Is.GreaterThan(0), "a teleport must still be caught: " + tst.What());
        }

        static void walk2(float seconds, float spd)
        {
            var l = new link("p");

            int n = (int)(seconds / 0.05f);

            float x = 0f;

            for (int i = 0; i < n; i++)
            {
                x += spd * 0.05f;
                l.t += 0.05f;

                l.put(new Vector3(x, 0f, 0f), new Vector3(spd, 0f, 0f));
            }
        }

        sealed class link
        {
            public string id;
            public float t;
            public int sq;

            public link(string pid)
            {
                id = pid;
                t = 100f;
                sq = 0;

                gactime.now = t;
                gactime.dt = 0.05f;

                put(Vector3.zero, Vector3.zero);
            }

            public link(string pid, Vector3 p0, Vector3 v0)
            {
                id = pid;
                t = 100f;
                sq = 0;

                gactime.now = t;
                gactime.dt = 0.05f;

                put(p0, v0);
            }

            public void put(Vector3 p, Vector3 v)
            {
                gactime.now = t;
                gactime.dt = 0.05f;

                sq++;

                gac.upd2(id, p, v, true, sq, t);
            }
        }
    }
}