using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    [TestFixture]
    public class anticheat
    {
        [SetUp]
        public void up()
        {
            tst.boot();
        }

        static void feed(string id, float seconds, float spd)
        {
            float t = 100f;
            float x = 0f;
            int sq = 0;

            gactime.now = t;
            gactime.dt = 0.05f;

            sq++;
            gac.upd2(id, new Vector3(x, 0f, 0f), new Vector3(spd, 0f, 0f), true, sq, t);

            int n = (int)(seconds / 0.05f);

            for (int i = 0; i < n; i++)
            {
                t += 0.05f;
                x += spd * 0.05f;
                sq++;

                gactime.now = t;
                gactime.dt = 0.05f;

                gac.upd2(id, new Vector3(x, 0f, 0f), new Vector3(spd, 0f, 0f), true, sq, t);
            }
        }

        [Test]
        public void prediction_never_stops_a_reported_teleport()
        {
            gacp.pr = true;

            var l = new predlink("p");

            feed("p", 5f, 6f);

            int before = gacr.flgof("p");

            for (int i = 0; i < 200; i++)
            {
                l.t += 0.05f;

                l.put(new Vector3(500f + i * 3f, 0f, 0f), new Vector3(6f, 0f, 0f));
            }

            Assert.That(tst.flags, Is.GreaterThan(0),
                "a reported teleport must be flagged no matter what the renderer shows");
            Assert.That(gacr.flgof("p"), Is.GreaterThan(before));
        }

        [Test]
        public void prediction_does_not_hide_a_speedhack_from_the_checks()
        {
            gacp.pr = true;

            int seen = tst.flags;

            feed("q", 60f, 60f);

            Assert.That(tst.flags - seen, Is.GreaterThan(0), "the checks see reported positions only");

            gacpc s = gacpred.st(gac.find("q"));

            Assert.That(s, Is.Not.Null, "the model still tracks the player");
        }

[Test]
        public void a_bound_correction_is_not_validation()
        {
            gacp.pr = true;

            feed("r", 5f, 6f);

            gacpc s = gacpred.st(gac.find("r"));

            Assert.That(s, Is.Not.Null);

            for (int i = 0; i < 5; i++)
            {
                gactime.now += 0.05f;

                gacpred.step(gac.find("r"),
                    gac.find("r").p + new Vector3(400f, 0f, 0f), new Vector3(6f, 0f, 0f), 0.05f);
            }

            Assert.That(s.off.magnitude, Is.LessThanOrEqualTo(gacp.prdmaxoff + 0.001f),
                "the caps bound the correction, they do not validate anything");

Assert.That(tst.flags, Is.EqualTo(0),
                "and the checks never saw it, because it was injected outside upd2");
        }

        [Test]
        public void prediction_is_off_by_default_so_the_risk_is_opt_in()
        {
            Assert.That(gacp.pr, Is.False);
        }

        [Test]
        public void the_full_pipeline_still_works_without_prediction()
        {
            Assert.That(gacpred.pos("nobody"), Is.EqualTo(Vector3.zero));
            Assert.That(gacpred.err("nobody"), Is.EqualTo(0f));

            feed("s", 10f, 6f);

            Assert.That(gacpred.err("s"), Is.EqualTo(0f), "no prediction, no model");
        }

        sealed class predlink
        {
            public string id;
            public float t;
            public int sq;

            public predlink(string pid)
            {
                id = pid;
                t = 100f;
                sq = 0;

                gactime.now = t;
                gactime.dt = 0.05f;

                put(Vector3.zero, Vector3.zero);
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