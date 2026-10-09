using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    [TestFixture]
    public class noclipfp
    {
        [SetUp]
        public void up()
        {
            tst.boot();
        }

        [TearDown]
        public void dn()
        {
            world.off();
            gacp.srv = false;
        }

        static int count(string c)
        {
            int n = 0;

            foreach (string s in tst.names) if (s.StartsWith(c + ":")) n++;

            return n;
        }

        static void sweep(bool wall, float wx)
        {
            gacp.srv = true;

            if (wall) world.wall();
            else world.off();

            float clock = 100f;
            float w = 2f * (float)Math.PI / 8f;

            gactime.now = clock;
            gactime.dt = 0.05f;

            gac.upd2("c", new Vector3(3f, 0f, 0f), Vector3.zero, true, 1, clock);

            int n = (int)(120f / 0.05f);

            for (int i = 1; i <= n; i++)
            {
                float t = i * 0.05f;

                float x = 3f + 4f * (float)Math.Sin(w * t);
                float v = 4f * w * (float)Math.Cos(w * t);

                clock += 0.05f;
                gactime.now = clock;
                gactime.dt = 0.05f;

                gac.upd2("c", new Vector3(x, 0f, 0f), new Vector3(v, 0f, 0f), true, i + 1, clock);
            }
        }

        [Test]
        public void a_smooth_sweep_through_a_wall_trips_only_the_wall_check()
        {
            sweep(true, 3f);

            Assert.That(count("noclip"), Is.GreaterThan(10), "the sweep must cross the wall many times");
            Assert.That(tst.flags, Is.EqualTo(count("noclip")),
                "only the wall check may fire: " + tst.What());
            Assert.That(gacr.evind("c"), Is.EqualTo(1), "one check is one piece of evidence");
        }

        [Test]
        public void the_same_motion_without_a_wall_is_silent()
        {
            sweep(false, 3f);

            Assert.That(tst.flags, Is.EqualTo(0), "the same motion without a wall must be silent: " + tst.What());
        }
    }
}