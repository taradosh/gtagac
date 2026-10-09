using System;
using System.Collections.Generic;
using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    public static class world
    {
        public static float wx = 0f;
        public static bool on;

        public static void wall()
        {
            on = true;

            Physics.RaycastHitHook = (Vector3 o, Vector3 d, float m, int mask, out RaycastHit h) =>
            {
                if (!on || d.x <= 0.05f && d.x >= -0.05f) { h = default; return false; }

                float t = (wx - o.x) / d.x;

                if (t < 0f || t > m) { h = default; return false; }

                h = new RaycastHit
                {
                    distance = t,
                    normal = new Vector3(d.x > 0f ? -1f : 1f, 0f, 0f),
                    point = o + d * t
                };

                return true;
            };

            Physics.RaycastHook = (o, d, m, mask) =>
            {
                RaycastHit h;

                return Physics.RaycastHitHook(o, d, m, mask, out h);
            };
        }

        public static void off()
        {
            on = false;
            Physics.RaycastHook = null;
            Physics.RaycastHitHook = null;
        }
    }

    [TestFixture]
    public class noclip
    {
        [SetUp]
        public void up()
        {
            tst.boot();
            world.wall();
            gacp.srv = true;
            gacp.cn = true;
            gac.sync();
        }

        [TearDown]
        public void dn()
        {
            world.off();
            gacp.srv = false;
        }

        static void seed(sim s, float x, float dt)
        {
            s.vel(0f, 0f, 0f).gnd(true).at(x, 0f, 0f).step(dt);
            gac.evl(s.d);
        }

        static void step(sim s, float x, float dt)
        {
            world.wx = 3f;

            float from = s.d.h.n > 0 ? s.d.p.x : x;

            s.vel((x - from) / dt, 0f, 0f).gnd(true).at(x, 0f, 0f).step(dt);

            gac.evl(s.d);
        }

        static void cross(sim s, int n, float from, float dt)
        {
            seed(s, from, dt);

            for (int i = 1; i <= n; i++) step(s, from + i * 0.2f, dt);
        }

        static bool hit(string c)
        {
            foreach (string n in tst.names) if (n.StartsWith(c + ":")) return true;

            return false;
        }

        [Test]
        public void approaching_a_wall_is_silent()
        {
            var s = new sim();

            seed(s, 2f, 0.05f);

            for (int i = 0; i < 200; i++) step(s, Math.Min(2.9f, 2f + i * 0.005f), 0.05f);

            Assert.That(hit("noclip"), Is.False, "stopping at a wall must be silent: " + tst.What());
        }

        [Test]
        public void standing_against_a_wall_is_silent()
        {
            var s = new sim();

            seed(s, 2.9f, 0.05f);

            for (int i = 0; i < 200; i++) step(s, 2.9f, 0.05f);

            Assert.That(hit("noclip"), Is.False, "standing against a wall must be silent: " + tst.What());
        }

        [Test]
        public void crossing_a_wall_is_detected()
        {
            cross(new sim(), 60, 2.9f, 0.05f);

            Assert.That(hit("noclip"), Is.True, "walking through a wall must be caught: " + tst.What());
        }

        [Test]
        public void crossing_the_other_way_is_detected()
        {
            var s = new sim();

            seed(s, 4f, 0.05f);

            for (int i = 1; i <= 60; i++) step(s, 4f - i * 0.2f, 0.05f);

            Assert.That(hit("noclip"), Is.True, "walking back through a wall must be caught: " + tst.What());
        }

        [Test]
        public void one_crossing_is_evidence_but_no_punishment()
        {
            int warned = 0;
            int kicked = 0;

            gacs.onwarning = e => warned++;
            gacs.onkick = e => kicked++;

            var s = new sim();

            seed(s, 2.9f, 0.05f);

            step(s, 3.5f, 0.05f);

            Assert.That(hit("noclip"), Is.True, "a clean crossing is still evidence: " + tst.What());
            Assert.That(warned, Is.EqualTo(0), "one crossing must never warn");
            Assert.That(kicked, Is.EqualTo(0), "one crossing must never kick");
        }

        [Test]
        public void repeated_crossing_escalates_to_a_kick()
        {
            int kicked = 0;

            gacs.onkick = e => kicked++;

            var s = new sim();

            seed(s, 2.9f, 0.05f);

            for (int i = 1; i <= 400; i++)
            {
                step(s, i % 2 == 0 ? 3.5f : 2.5f, 0.05f);
            }

            Assert.That(hit("noclip"), Is.True, "repeated crossings must be caught: " + tst.What());
            Assert.That(kicked, Is.GreaterThan(0), "repeated crossings must escalate to a kick");
        }

        [Test]
        public void suspicion_clears_after_clean_samples()
        {
            gacp.ncsmpl = 6f;

            var s = new sim();

            seed(s, 2.9f, 0.05f);

            for (int i = 1; i <= 5; i++) step(s, 2.9f + i * 0.2f, 0.05f);

            Assert.That(hit("noclip"), Is.False, "five samples are below the threshold: " + tst.What());

            for (int i = 0; i < 200; i++) step(s, 2.9f, 0.05f);

            Assert.That(hit("noclip"), Is.False, "clean samples must clear suspicion: " + tst.What());

            for (int i = 1; i <= 12; i++) step(s, i % 2 == 0 ? 3.5f : 2.5f, 0.05f);

            Assert.That(hit("noclip"), Is.True, "the threshold must apply again after a clear: " + tst.What());
        }

        [Test]
        public void suspicion_accumulates_over_the_whole_run()
        {
            var s = new sim();

            seed(s, 2.9f, 0.05f);

            for (int i = 1; i <= 60; i++)
            {
                world.wx = i % 2 == 0 ? 3f : 4.5f;

                step(s, 2.9f + i * 0.2f, 0.05f);

                world.wx = 3f;
            }

            Assert.That(hit("noclip"), Is.True,
                "a wall that appears and vanishes every sample is still a noclip signature: " + tst.What());
        }

        [Test]
        public void back_faces_do_not_count()
        {
            Physics.RaycastHitHook = (Vector3 o, Vector3 d, float m, int mask, out RaycastHit h) =>
            {
                float t = (3f - o.x) / d.x;

                h = new RaycastHit { distance = t, normal = new Vector3(1f, 0f, 0f), point = o + d * t };

                return true;
            };

            Physics.RaycastHook = (o, d, m, mask) =>
            {
                RaycastHit h;

                return Physics.RaycastHitHook(o, d, m, mask, out h);
            };

            cross(new sim(), 60, 2.9f, 0.05f);

            Assert.That(hit("noclip"), Is.False,
                "a back face is the signature of a ray leaving solid geometry: " + tst.What());
        }

        [Test]
        public void the_check_is_off_without_server_colliders()
        {
            gacp.srv = false;
            gacp.ncloc = false;

            cross(new sim(), 60, 2.9f, 0.05f);

            Assert.That(hit("noclip"), Is.False, "client colliders are not evidence: " + tst.What());
        }

        [Test]
        public void the_check_can_run_on_a_host()
        {
            gacp.srv = false;
            gacp.ncloc = true;

            cross(new sim(), 60, 2.9f, 0.05f);

            Assert.That(hit("noclip"), Is.True, "ncloc must enable the check: " + tst.What());
        }

        [Test]
        public void the_mask_can_exclude_the_layer()
        {
            gacp.ncmask = 0;

            cross(new sim(), 60, 2.9f, 0.05f);

            Assert.That(hit("noclip"), Is.False, "an empty mask must disable the query: " + tst.What());
        }

        [Test]
        public void grace_suppresses_the_check()
        {
            var s = new sim();

            seed(s, 2.9f, 0.05f);

            s.d.grace(10f);

            for (int i = 1; i <= 60; i++) step(s, 2.9f + i * 0.2f, 0.05f);

            Assert.That(hit("noclip"), Is.False, "grace must suppress detection: " + tst.What());
        }

        [Test]
        public void a_step_outside_the_sample_window_is_ignored()
        {
            var s = new sim();

            seed(s, 2.9f, 0.5f);

            for (int i = 0; i < 60; i++) step(s, 2.9f, 0.5f);

            Assert.That(hit("noclip"), Is.False, "a 0.5 s step is not a sample: " + tst.What());
        }

        [Test]
        public void noclip_can_be_switched_off()
        {
            gacp.cn = false;
            gac.sync();

            cross(new sim(), 60, 2.9f, 0.05f);

            Assert.That(hit("noclip"), Is.False, "cn=false must disable the check: " + tst.What());
        }
    }
}