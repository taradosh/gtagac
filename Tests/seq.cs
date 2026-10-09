using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    public sealed class seqtests
    {
        [SetUp]
        public void up()
        {
            gac.shut();
            gacp.reset();
            gacp.devby = false;
            gactime.init();
        }

        [TearDown]
        public void dn()
        {
            gac.shut();
        }

        static float Clock(float v)
        {
            gactime.now = v;
            return v;
        }

        [Test]
        public void strict_off_accepts_anything_for_backward_compatibility()
        {
            gacp.strict = false;

            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 5, 0.0), Is.True);
            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 0.0), Is.True);
            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 99, 0.0), Is.True);
        }

        [Test]
        public void strict_on_rejects_a_replayed_sequence()
        {
            gacp.strict = true;

            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 0.0), Is.True);
            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 2, 0.0), Is.True);
            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 2, 0.0), Is.False);
            Assert.That(gac.dropped("a"), Is.EqualTo(1));
        }

        [Test]
        public void strict_on_rejects_out_of_order_updates()
        {
            gacp.strict = true;

            gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 0.0);
            gac.upd2("a", Vector3.zero, Vector3.zero, true, 5, 0.0);

            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 4, 0.0), Is.False);
            Assert.That(gac.outoforder("a"), Is.EqualTo(1));
        }

        [Test]
        public void a_large_forward_gap_is_tolerated_and_counted()
        {
            gacp.strict = true;
            gacp.seqgap = 8;

            gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 0.0);

            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 40, 0.0), Is.True);

            gacd d = gac.find("a");

            Assert.That(d, Is.Not.Null);
            Assert.That(d!.lost, Is.EqualTo(38));
            Assert.That(d.sus, Is.EqualTo(1));
            Assert.That(d.drp, Is.EqualTo(0), "a stalled connection must not drop updates");
        }

        [Test]
        public void strict_on_accepts_a_small_forward_gap_as_packet_loss()
        {
            gacp.strict = true;
            gacp.seqgap = 8;

            gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 0.0);

            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 4, 0.0), Is.True);

            gacd d = gac.find("a");

            Assert.That(d, Is.Not.Null);
            Assert.That(d!.lost, Is.EqualTo(2));
        }

        [Test]
        public void strict_on_rejects_a_stale_client_timestamp()
        {
            gacp.strict = true;
            gacp.stale = 0.35f;

            Clock(100f);
            gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 100.0);

            Clock(100.1f);
            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 2, 100.1), Is.True);

            Clock(101f);
            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 3, 100.2), Is.False);
        }

        [Test]
        public void strict_on_rejects_a_timestamp_from_the_future()
        {
            gacp.strict = true;
            gacp.fut = 0.5f;

            Clock(100f);
            gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 100.0);

            Clock(100.1f);
            Assert.That(gac.upd2("a", Vector3.zero, Vector3.zero, true, 2, 103.0), Is.False);
        }

        [Test]
        public void clock_offset_is_learned_from_the_first_packet()
        {
            gacp.strict = true;

            Clock(5000f);
            gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 12.0);

            gacd d = gac.find("a");

            Assert.That(d, Is.Not.Null);
            Assert.That(d!.off, Is.EqualTo(5000.0 - 12.0).Within(0.001));
        }

        [Test]
        public void steady_stream_keeps_zero_drops()
        {
            gacp.strict = true;

            Clock(100f);
            gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 100.0);

            for (int i = 2; i <= 50; i++)
            {
                Clock(100f + (i - 1) * 0.05f);

                Assert.That(gac.upd2("a", new Vector3(i * 0.1f, 0f, 0f), Vector3.zero, true, i, 100.0 + (i - 1) * 0.05f), Is.True);
            }

            Assert.That(gac.dropped("a"), Is.EqualTo(0));
            Assert.That(gac.outoforder("a"), Is.EqualTo(0));
        }

        [Test]
        public void jitter_raises_the_distance_slack()
        {
            gacp.strict = true;
            gacp.jitter = 1f;

            Clock(100f);
            gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 100.0);

            float[] iv = { 0.05f, 0.05f, 0.05f, 0.20f, 0.05f, 0.05f, 0.20f, 0.05f, 0.05f, 0.05f };

            float t = 100f;

            for (int i = 0; i < iv.Length; i++)
            {
                t += iv[i];
                Clock(t);
                gac.upd2("a", Vector3.zero, Vector3.zero, true, i + 2, t);
            }

            gacd d = gac.find("a");

            Assert.That(d, Is.Not.Null);
            Assert.That(d!.jit, Is.GreaterThan(0.01f));
            Assert.That(d.slack, Is.GreaterThan(0.5f));
            Assert.That(d.slack, Is.LessThanOrEqualTo(gacp.maxslack));
        }

        [Test]
        public void a_smooth_stream_produces_no_slack()
        {
            gacp.strict = true;

            Clock(100f);
            gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 100.0);

            float t = 100f;

            for (int i = 0; i < 20; i++)
            {
                t += 0.05f;
                Clock(t);
                gac.upd2("a", Vector3.zero, Vector3.zero, true, i + 2, t);
            }

            gacd d = gac.find("a");

            Assert.That(d, Is.Not.Null);
            Assert.That(d!.jit, Is.LessThan(0.001f));
            Assert.That(d.slack, Is.LessThan(0.01f));
        }

        [Test]
        public void packet_loss_raises_the_slack()
        {
            gacp.strict = true;
            gacp.seqgap = 64;
            gacp.loss = 1f;

            Clock(100f);
            gac.upd2("a", Vector3.zero, Vector3.zero, true, 1, 100.0);

            float t = 100f;

            for (int i = 0; i < 6; i++)
            {
                t += 0.05f;
                Clock(t);
                gac.upd2("a", Vector3.zero, Vector3.zero, true, i + 8, t);
            }

            gacd d = gac.find("a");

            Assert.That(d, Is.Not.Null);
            Assert.That(d!.slack, Is.GreaterThan(0.1f));
        }

        [Test]
        public void legacy_upd_still_works_with_no_sequence()
        {
            gacp.strict = false;

            for (int i = 0; i < 20; i++)
            {
                gac.upd("legacy", new Vector3(i * 0.05f, 0f, 0f), Vector3.zero, true);
            }

            gacd d = gac.find("legacy");

            Assert.That(d, Is.Not.Null);
            Assert.That(d!.h.n, Is.EqualTo(20));
            Assert.That(d.drp, Is.EqualTo(0));
        }

        [Test]
        public void slot_limit_is_enforced()
        {
            int want = gacp.slots;

            for (int i = 0; i < want + 8; i++)
                gac.upd("p" + i, Vector3.zero, Vector3.zero, true);

            Assert.That(gac.ply.Count, Is.EqualTo(want));
        }
    }
}