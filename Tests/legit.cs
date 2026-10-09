using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    public sealed class legit
    {
        [SetUp]
        public void up()
        {
            tst.boot();
        }

        [TearDown]
        public void dn()
        {
            gac.shut();
        }

        [Test]
        public void walking_slow_is_silent()
        {
            tst.walk(30f, 0.05f, 1.5f);

            Assert.That(tst.flags, Is.EqualTo(0), "slow walk: " + gacr.flgof("p"));
        }

        [Test]
        public void walking_fast_is_silent()
        {
            tst.walk(30f, 0.05f, 10.5f);

            Assert.That(tst.flags, Is.EqualTo(0), "fast walk: " + gacr.flgof("p"));
        }

        [Test]
        public void standing_still_is_silent()
        {
            tst.walk(20f, 0.05f, 0f);

            Assert.That(tst.flags, Is.EqualTo(0));
        }

        [Test]
        public void jumping_is_silent()
        {
            tst.jump(3f, 0.05f, 5f);

            Assert.That(tst.flags, Is.EqualTo(0), "jump: " + gacr.flgof("p") + " | " + tst.What());
        }

        [Test]
        public void climbing_is_silent()
        {
            tst.climb(10f, 0.05f, 2f);

            Assert.That(tst.flags, Is.EqualTo(0), "climb: " + gacr.flgof("p"));
        }

        [Test]
        public void vine_swing_is_silent()
        {
            tst.swing(20f, 0.05f, 8f, 1.2f, 3.5f);

            Assert.That(tst.flags, Is.EqualTo(0), "swing: " + gacr.flgof("p") + " | " + tst.What());
        }

        [Test]
        public void controller_jitter_is_silent()
        {
            tst.noisy(30f, 0.05f, 3f, 0.08f, 11);

            Assert.That(tst.flags, Is.EqualTo(0), "jitter: " + gacr.flgof("p"));
        }

        [Test]
        public void fast_falling_onto_ground_is_silent()
        {
            var s = new sim();
            float y = 60f;
            float vy = 0f;

            s.at(0f, y, 0f);
            s.vel(0f, vy, 0f);
            s.gnd(false);
            s.step(0.05f);
            gac.evl(s.d);

            for (int i = 0; i < 160; i++)
            {
                vy -= gacp.grav * 0.05f;
                y += vy * 0.05f;

                bool land = y <= 0f;

                if (land) { y = 0f; vy = 0f; }

                s.vel(0f, vy, 0f);
                s.at(0f, y, 0f);
                s.gnd(land);
                s.step(0.05f);
                gac.evl(s.d);
            }

            Assert.That(tst.flags, Is.EqualTo(0), "fall: " + gacr.flgof("p") + " | " + tst.What());
        }

        [Test]
        public void collision_push_is_silent()
        {
            var s = new sim();

            s.at(0f, 0f, 0f);
            s.step(0.05f);
            gac.evl(s.d);

            for (int i = 0; i < 4; i++)
            {
                s.mv(new Vector3(1.2f, 0f, 0f));
                s.gnd(true);
                s.step(0.05f);
                gac.evl(s.d);
            }

            for (int i = 0; i < 40; i++)
            {
                s.mv(new Vector3(0.05f, 0f, 0f));
                s.gnd(true);
                s.step(0.05f);
                gac.evl(s.d);
            }

            Assert.That(tst.flags, Is.EqualTo(0), "push: " + gacr.flgof("p"));
        }
    }
}