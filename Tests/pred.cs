using System;
using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    [TestFixture]
    public class pred
    {
        [SetUp]
        public void up()
        {
            tst.boot();
            gacp.pr = true;
            gacpred.clr();
        }

        sealed class link
        {
            public string id;
            public gacd d;
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

                d = gac.find(id);
            }

            public float cstep()
            {
                gacpc s = gacpred.st(d);

                return s == null ? 0f : s.cstep;
            }

            public float off()
            {
                gacpc s = gacpred.st(d);

                return s == null ? 0f : s.off.magnitude;
            }
        }

        static float worst(link l, float seconds)
        {
            float m = 0f;
            int n = (int)(seconds / 0.05f);

            for (int i = 0; i < n; i++)
            {
                float s = l.cstep();

                if (s > m) m = s;
            }

            return m;
        }

        static void walk(link l, float seconds, float spd, float jitter, int seed)
        {
            var rnd = new Random(seed);

            int n = (int)(seconds / 0.05f);
            float x = 0f;

            for (int i = 0; i < n; i++)
            {
                x += spd * 0.05f;
                l.t += 0.05f;

                float j = (float)((rnd.NextDouble() - 0.5) * jitter);

                l.put(new Vector3(x + j, 0f, 0f), new Vector3(spd, 0f, 0f));
            }
        }

        static void lossy(link l, float seconds, float spd, float jitter, int loss, int seed)
        {
            var rnd = new Random(seed);

            int n = (int)(seconds / 0.05f);
            float x = 0f;

            for (int i = 0; i < n; i++)
            {
                l.t += 0.05f + (float)(rnd.NextDouble() - 0.5) * jitter;

                if (rnd.Next(100) < loss) continue;

                x += spd * 0.05f;
                l.put(new Vector3(x, 0f, 0f), new Vector3(spd, 0f, 0f));
            }
        }

        static void swing(link l, float seconds, float len, float amp, float period)
        {
            float w = 2f * (float)Math.PI / period;

            int n = (int)(seconds / 0.05f);

            for (int i = 0; i <= n; i++)
            {
                float t = i * 0.05f;
                float a = amp * (float)Math.Sin(w * t);

                float x = len * (float)Math.Sin(a);
                float y = -len * (float)Math.Cos(a);

                float vx = len * (float)Math.Cos(a) * amp * w * (float)Math.Cos(w * t);
                float vy = len * (float)Math.Sin(a) * amp * w * (float)Math.Cos(w * t);

                l.t += 0.05f;

                l.put(new Vector3(x, y, 0f), new Vector3(vx, vy, 0f));
            }
        }

        static void turns(link l, float seconds, float spd, float turn)
        {
            int n = (int)(seconds / 0.05f);

            float x = l.d.p.x;
            float z = l.d.p.z;
            float a = 0f;

            for (int i = 0; i < n; i++)
            {
                a += turn;

                float dx = (float)Math.Cos(a);
                float dz = (float)Math.Sin(a);

                x += dx * spd * 0.05f;
                z += dz * spd * 0.05f;

                l.t += 0.05f;

                l.put(new Vector3(x, 0f, z), new Vector3(dx * spd, 0f, dz * spd));
            }
        }

        [Test]
        public void prediction_is_off_by_default()
        {
            gacp.reset();

            Assert.That(gacp.pr, Is.False, "prediction must be opt in");
        }

        [Test]
        public void prediction_does_nothing_when_off()
        {
            gacp.pr = false;

            var l = new link("p");

            walk(l, 10f, 6f, 0f, 1);

            Assert.That(gacpred.err("p"), Is.EqualTo(0f), "no prediction means no residual");
            Assert.That(l.off(), Is.EqualTo(0f), "and no offset");
        }

        [Test]
        public void a_steady_walk_needs_no_correction()
        {
            var l = new link("p");

            walk(l, 20f, 6f, 0f, 1);

            Assert.That(gacpred.err("p"), Is.LessThan(0.01f), "a perfect stream needs no correction");
            Assert.That(l.off(), Is.LessThan(0.01f), "and no visible offset");
        }

        [Test]
        public void high_ping_does_not_punish()
        {
            var l = new link("p");

            gac.ping("p", 300f);

            walk(l, 60f, 6f, 0f, 1);

            Assert.That(tst.flags, Is.EqualTo(0), "ping alone must not flag: " + tst.What());
        }

        [Test]
        public void high_ping_does_not_teleport_the_player()
        {
            var l = new link("p");

            gac.ping("p", 300f);

            walk(l, 60f, 6f, 0.2f, 7);

            float m = worst(l, 60f);

            Assert.That(m, Is.LessThan(0.5f), "a correction must stay under 0.5 m, got " + m);
        }

        [Test]
        public void an_out_of_range_ping_is_capped()
        {
            var l = new link("p");

            gac.ping("p", 60000f);

            Assert.That(gacpred.err("p"), Is.EqualTo(0f));
            Assert.That(l.d.pin, Is.LessThanOrEqualTo(gacp.pingmax),
                "a silly rtt must not widen every tolerance");
        }

        [Test]
        public void a_negative_ping_is_ignored()
        {
            var l = new link("p");

            gac.ping("p", -1f);

            Assert.That(l.d.pin, Is.EqualTo(0f));
        }

        [Test]
        public void ping_is_per_player_not_global()
        {
            var a = new link("a");
            var b = new link("b");

            gac.ping("a", 300f);

            Assert.That(a.d.pin, Is.GreaterThan(0f));
            Assert.That(b.d.pin, Is.EqualTo(0f), "one slow player must not loosen everyone");
        }

        [Test]
        public void packet_loss_does_not_punish()
        {
            var l = new link("p");

            lossy(l, 120f, 6f, 0.3f, 20, 5);

            Assert.That(tst.flags, Is.EqualTo(0), "20 percent loss must not flag: " + tst.What());
        }

        [Test]
        public void packet_loss_does_not_teleport_the_player()
        {
            var l = new link("p");

            lossy(l, 120f, 6f, 0.3f, 20, 5);

            float m = worst(l, 120f);

            Assert.That(m, Is.LessThan(0.5f), "a correction must stay under 0.5 m, got " + m);
        }

        [Test]
        public void a_stall_resets_the_model_instead_of_snapping()
        {
            var l = new link("p");

            walk(l, 5f, 6f, 0f, 1);

            gacpc s = gacpred.st(l.d);

            Assert.That(s, Is.Not.Null);

            l.t += 5f;

            l.put(new Vector3(30f, 0f, 0f), Vector3.zero);

            Assert.That(s.off, Is.EqualTo(Vector3.zero), "the visual offset must be cleared");
            Assert.That(s.cstep, Is.EqualTo(0f), "and no correction may be applied");
        }

        [Test]
        public void sharp_turns_do_not_punish()
        {
            var l = new link("p");

            turns(l, 60f, 6f, 0.25f);

            Assert.That(tst.flags, Is.EqualTo(0), "sharp turns must not flag: " + tst.What());
        }

        [Test]
        public void sharp_turns_do_not_teleport_the_player()
        {
            var l = new link("p");

            turns(l, 60f, 6f, 0.25f);

            float m = worst(l, 60f);

            Assert.That(m, Is.LessThan(0.5f), "a correction must stay under 0.5 m, got " + m);
        }

        [Test]
        public void gorilla_locomotion_does_not_punish()
        {
            var l = new link("p", new Vector3(0f, -8f, 0f), Vector3.zero);

            swing(l, 60f, 8f, 1.2f, 3.5f);

            Assert.That(tst.flags, Is.EqualTo(0), "gorilla locomotion must not flag: " + tst.What());
        }

        [Test]
        public void gorilla_locomotion_does_not_teleport_the_player()
        {
            var l = new link("p", new Vector3(0f, -8f, 0f), Vector3.zero);

            swing(l, 60f, 8f, 1.2f, 3.5f);

            float m = worst(l, 60f);

            Assert.That(m, Is.LessThan(0.5f), "a correction must stay under 0.5 m, got " + m);
        }

        [Test]
        public void a_long_rope_swing_does_not_punish()
        {
            var l = new link("p", new Vector3(0f, -12f, 0f), Vector3.zero);

            swing(l, 60f, 12f, 1.2f, 4f);

            Assert.That(tst.flags, Is.EqualTo(0), "a longer rope must not flag: " + tst.What());
        }

        [Test]
        public void the_visible_offset_is_always_bounded()
        {
            var l = new link("p");

            lossy(l, 120f, 6f, 0.3f, 20, 5);

            Assert.That(l.off(), Is.LessThanOrEqualTo(gacp.prdmaxoff + 0.001f),
                "the offset must never exceed the cap, got " + l.off());
        }

        [Test]
        public void prediction_never_feeds_the_checks()
        {
            var l = new link("p");

            lossy(l, 60f, 6f, 0.3f, 20, 5);

            Vector3 raw = l.d.p;

            gacpred.step(l.d, raw + new Vector3(5f, 0f, 0f), Vector3.zero, 0.05f);

            Assert.That(l.d.p, Is.EqualTo(raw), "the detection history must hold reported positions only");
        }

        [Test]
        public void the_offset_bleeds_back_to_zero()
        {
            var l = new link("p");

            walk(l, 5f, 6f, 0f, 1);

            gacpc s = gacpred.st(l.d);

            l.t += 0.05f;

            l.put(new Vector3(100f, 0f, 0f), new Vector3(6f, 0f, 0f));

            Assert.That(s.off.magnitude, Is.GreaterThan(0f), "a jump must produce an offset");

            for (int i = 0; i < 200; i++)
            {
                l.t += 0.05f;
                l.put(new Vector3(100f + i * 0.3f, 0f, 0f), new Vector3(6f, 0f, 0f));
            }

            Assert.That(s.off.magnitude, Is.LessThan(0.01f), "the offset must bleed away, got " + s.off.magnitude);
        }

        [Test]
        public void the_correction_per_update_is_capped()
        {
            var l = new link("p");

            walk(l, 5f, 6f, 0f, 1);

            gacpc s = gacpred.st(l.d);

            l.t += 0.05f;

            l.put(new Vector3(500f, 0f, 0f), new Vector3(6f, 0f, 0f));

            Assert.That(s.off.magnitude, Is.LessThanOrEqualTo(gacp.prdmaxoff + 0.001f));
            Assert.That(s.cstep, Is.LessThanOrEqualTo(gacp.prdmax + 0.001f),
                "one update may not correct more than prdmax, got " + s.cstep);
        }

        [Test]
        public void a_reset_rebases_immediately()
        {
            var l = new link("p");

            walk(l, 5f, 6f, 0f, 1);

            gacpc s = gacpred.st(l.d);

            gacpred.drop("p");

            Assert.That(s.ok, Is.False, "the model must forget");
            Assert.That(s.off, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void the_rendered_position_tracks_the_reported_one()
        {
            var l = new link("p");

            lossy(l, 60f, 6f, 0.3f, 20, 5);

            Assert.That(gacmath.len(gacpred.pos("p") - l.d.p), Is.LessThanOrEqualTo(gacp.prdmaxoff + 0.001f),
                "the rendered position must stay near the reported one");
        }

        [Test]
        public void the_rendered_position_never_lags_far_behind()
        {
            var l = new link("p");

            lossy(l, 60f, 6f, 0.3f, 20, 5);

            for (int i = 0; i < 1200; i++)
            {
                Vector3 v = gacpred.pos("p");

                Assert.That(gacmath.len(v - l.d.p), Is.LessThanOrEqualTo(gacp.prdmaxoff + 0.001f));

                l.t += 0.05f;
            }
        }

        [Test]
        public void an_unknown_player_has_no_prediction()
        {
            Assert.That(gacpred.pos("nobody"), Is.EqualTo(Vector3.zero));
            Assert.That(gacpred.err("nobody"), Is.EqualTo(0f));
            Assert.That(gacpred.off("nobody"), Is.EqualTo(Vector3.zero));
            Assert.That(gacpred.peak("nobody"), Is.EqualTo(0f));
        }

        [Test]
        public void a_bad_slot_is_rejected()
        {
            Assert.That(gacpred.of(-1), Is.Null);
            Assert.That(gacpred.of(gacp.slots), Is.Null);
            Assert.That(gacpred.st(null), Is.Null);
        }

        [Test]
        public void high_ping_with_loss_and_turns_is_still_clean()
        {
            var l = new link("p");

            gac.ping("p", 300f);

            for (int i = 0; i < 12; i++) turns(l, 5f, 6f, 0.25f);

            lossy(l, 60f, 6f, 0.3f, 20, 9);

            Assert.That(tst.flags, Is.EqualTo(0),
                "a bad link plus sharp turns must not punish: " + tst.What());

            float m = worst(l, 60f);

            Assert.That(m, Is.LessThan(0.5f), "and must not teleport, got " + m);
        }

        [Test]
        public void everything_at_once_is_still_clean()
        {
            var a = new link("a");

            gac.ping("a", 300f);

            walk(a, 10f, 6f, 0.2f, 3);
            turns(a, 20f, 6f, 0.25f);

            var b = new link("b", new Vector3(0f, -8f, 0f), Vector3.zero);

            gac.ping("b", 300f);

            swing(b, 20f, 8f, 1.2f, 3.5f);

            var c = new link("c");

            gac.ping("c", 300f);

            lossy(c, 60f, 6f, 0.3f, 20, 11);

            Assert.That(tst.flags, Is.EqualTo(0),
                "a bad link through every movement must not punish: " + tst.What());
        }
    }
}