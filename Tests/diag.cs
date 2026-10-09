using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    [TestFixture]
    public class diag
    {
        [Test]
        public void a_stopping_sample_with_old_velocity_is_rejected()
        {
            tst.boot();

            var s = new sim();

            for (int i = 0; i < 40; i++)
            {
                s.vel(0f, -30f, 0f);
                s.at(0f, -40f + i * 0.1f, 0f);
                s.gnd(true);
                s.step(0.05f);
                gac.evl(s.d);
            }

            s.gnd(true);
            s.step(0.05f);
            gac.evl(s.d);

            int quiet = tst.flags;

            for (int i = 0; i < 40; i++)
            {
                s.vel(0f, -30f, 0f);
                s.at(0f, -40f + i * 0.1f, 0f);
                s.gnd(true);
                s.step(0.05f);
                gac.evl(s.d);
            }

            s.at(0f, -40f, 0f);
            s.vel(0f, 0f, 0f);
            s.gnd(true);
            s.step(0.05f);
            gac.evl(s.d);

            Assert.That(tst.flags, Is.GreaterThan(quiet),
                "a sample whose position stops while it still reports -30 m/s must be caught | " + tst.What());
        }
    }
}