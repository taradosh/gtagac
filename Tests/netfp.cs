using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    public sealed class netfp
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

        static void feed(string id, float seconds, float spd, float dt, float jit, int loss, int seed)
        {
            var rnd = new Random(seed);

            float now = 100f;
            float x = 0f;
            int seq = 0;
            float prev = now;

            gactime.now = now;
            gactime.dt = dt;
            seq++;
            gac.upd2(id, new Vector3(x, 0f, 0f), new Vector3(spd, 0f, 0f), true, seq, now);

            int n = (int)(seconds / dt);

            for (int i = 0; i < n; i++)
            {
                now += dt + ((float)rnd.NextDouble() - 0.5f) * jit;
                x += spd * dt;

                if (rnd.Next(100) < loss) { seq++; prev = now; continue; }

                seq++;
                gactime.now = now;
                gactime.dt = now - prev;
                prev = now;
                gac.upd2(id, new Vector3(x, 0f, 0f), new Vector3(spd, 0f, 0f), true, seq, now);
            }
        }

        [Test]
        public void clean_stream_over_network_is_silent()
        {
            feed("a", 40f, 6f, 0.05f, 0f, 0, 1);

            Assert.That(tst.flags, Is.EqualTo(0), "clean: " + gacr.flgof("a"));
        }

        [Test]
        public void mild_jitter_is_silent()
        {
            feed("a", 40f, 6f, 0.05f, 0.04f, 0, 2);

            Assert.That(tst.flags, Is.EqualTo(0), "mild jitter: " + gacr.flgof("a"));
        }

        [Test]
        public void bad_wifi_jitter_is_silent()
        {
            feed("a", 40f, 6f, 0.05f, 0.3f, 0, 3);

            Assert.That(tst.flags, Is.EqualTo(0), "bad wifi: " + gacr.flgof("a"));
        }

        [Test]
        public void packet_loss_is_silent()
        {
            feed("a", 40f, 6f, 0.05f, 0.05f, 15, 4);

            Assert.That(tst.flags, Is.EqualTo(0), "loss: " + gacr.flgof("a") + " | " + tst.What());
        }

        [Test]
        public void loss_and_jitter_together_are_silent()
        {
            feed("a", 40f, 8f, 0.05f, 0.25f, 20, 5);

            Assert.That(tst.flags, Is.EqualTo(0), "loss+jitter: " + gacr.flgof("a") + " | " + tst.What());
        }

        [Test]
        public void a_long_stall_then_catchup_is_silent()
        {
            feed("a", 20f, 6f, 0.05f, 0.05f, 0, 6);

            gacd d = gac.find("a");
            Assert.That(d, Is.Not.Null);

            float now = gactime.now + 1.5f;
            float x = d!.p.x;

            for (int i = 0; i < 10; i++)
            {
                now += 0.05f;
                x += 6f * 0.05f;
                gactime.now = now;
                gactime.dt = 0.05f;
                gac.upd2("a", new Vector3(x, 0f, 0f), new Vector3(6f, 0f, 0f), true, d.sq + i + 1, now);
            }

            Assert.That(tst.flags, Is.EqualTo(0), "stall: " + gacr.flgof("a") + " | " + tst.What());
        }

        [Test]
        public void strict_clean_stream_is_silent()
        {
            gacp.strict = true;

            feed("a", 40f, 6f, 0.05f, 0f, 0, 7);

            Assert.That(tst.flags, Is.EqualTo(0), "strict clean: " + gacr.flgof("a"));
            Assert.That(gac.dropped("a"), Is.EqualTo(0));
        }

        [Test]
        public void strict_bad_wifi_is_silent()
        {
            gacp.strict = true;

            feed("a", 40f, 6f, 0.05f, 0.3f, 0, 8);

            Assert.That(tst.flags, Is.EqualTo(0), "strict wifi: " + gacr.flgof("a"));
        }

        [Test]
        public void strict_packet_loss_is_silent()
        {
            gacp.strict = true;

            feed("a", 40f, 6f, 0.05f, 0.05f, 15, 9);

            Assert.That(tst.flags, Is.EqualTo(0), "strict loss: " + gacr.flgof("a"));
        }

        [Test]
        public void strict_a_long_stall_is_accepted_not_muted()
        {
            gacp.strict = true;

            feed("a", 20f, 6f, 0.05f, 0.05f, 0, 10);

            gacd d = gac.find("a");
            Assert.That(d, Is.Not.Null);

            float x = d!.p.x;
            float now = gactime.now + 1.5f;

            int acc = 0;

            for (int i = 0; i < 10; i++)
            {
                now += 0.05f;
                x += 6f * 0.05f;
                gactime.now = now;

                if (gac.upd2("a", new Vector3(x, 0f, 0f), new Vector3(6f, 0f, 0f), true, d.sq + 40, now))
                    acc++;
            }

            Assert.That(tst.flags, Is.EqualTo(0), "strict stall flags: " + gacr.flgof("a"));
            Assert.That(acc, Is.EqualTo(10), "stall updates were dropped, player gets muted");
            Assert.That(d.p.x, Is.EqualTo(x).Within(0.001f), "last accepted position is not the reported one");
        }

        [Test]
        public void strict_rejects_replay_even_while_a_player_is_legitimate()
        {
            gacp.strict = true;

            feed("a", 10f, 6f, 0.05f, 0f, 0, 11);

            gacd d = gac.find("a");
            Assert.That(d, Is.Not.Null);

            int n = d!.h.n;
            int sq = d.sq;

            Assert.That(gac.upd2("a", d.p, d.v, true, sq, gactime.now), Is.False);
            Assert.That(gac.upd2("a", d.p, d.v, true, sq - 5, gactime.now), Is.False);
            Assert.That(d.h.n, Is.EqualTo(n), "replayed packets must not enter the history");
        }
    }
}