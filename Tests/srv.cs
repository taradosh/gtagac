using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    [TestFixture]
    public class srv
    {
        [SetUp]
        public void up()
        {
            tst.boot();
            gacsrv.init();
            gacsrv.ban = null;
        }

        [TearDown]
        public void dn()
        {
            gacsrv.net.clr();
        }

        [Test]
        public void a_join_registers_the_player()
        {
            gacsrv.join("a");

            Assert.That(gac.find("a"), Is.Not.Null);
            Assert.That(gacsrv.net.cnt, Is.EqualTo(1));
        }

        [Test]
        public void a_join_twice_does_not_duplicate()
        {
            gacsrv.join("a");
            gacsrv.join("a");

            Assert.That(gacsrv.net.cnt, Is.EqualTo(1));
            Assert.That(gac.ply.Count, Is.EqualTo(2), "the local player plus one remote, nothing more");
        }

        [Test]
        public void a_leave_removes_the_player()
        {
            gacsrv.join("a");
            gacsrv.leave("a");

            Assert.That(gacsrv.net.cnt, Is.EqualTo(0));
            Assert.That(gac.find("a"), Is.Null);
        }

        [Test]
        public void the_timestamp_is_no_longer_discarded()
        {
            gacsrv.join("a");

            gacp.strict = true;

            gactime.now = 100f;

            Assert.That(gacsrv.up("a", Vector3.zero, Vector3.zero, true, 100.0), Is.True,
                "a fresh packet must be accepted");

            Assert.That(gacsrv.up("a", Vector3.zero, Vector3.zero, true, 10.0), Is.False,
                "a stale timestamp must now be rejected");

            Assert.That(gacsrv.up("a", Vector3.zero, Vector3.zero, true, 500.0), Is.False,
                "a timestamp from the future must be rejected");
        }

        [Test]
        public void the_legacy_path_still_works()
        {
            gacsrv.join("a");

            gacsrv.net.push("a", Vector3.zero, Vector3.zero, true);

            Assert.That(gac.find("a"), Is.Not.Null);
        }

        [Test]
        public void an_explicit_sequence_is_honoured()
        {
            gacsrv.join("a");

            gacp.strict = true;

            gactime.now = 100f;

            Assert.That(gacsrv.up("a", Vector3.zero, Vector3.zero, true, 100.0, 1), Is.True);
            Assert.That(gacsrv.up("a", Vector3.zero, Vector3.zero, true, 100.0, 1), Is.False,
                "a replayed sequence must be rejected");
            Assert.That(gacsrv.up("a", Vector3.zero, Vector3.zero, true, 100.0, 2), Is.True);
        }

        [Test]
        public void an_explicit_sequence_survives_a_gap()
        {
            gacsrv.join("a");

            gacp.strict = true;
            gacp.seqgap = 8;

            gactime.now = 100f;

            Assert.That(gacsrv.up("a", Vector3.zero, Vector3.zero, true, 100.0, 1), Is.True);
            Assert.That(gacsrv.up("a", Vector3.zero, Vector3.zero, true, 100.0, 30), Is.True,
                "a forward gap is loss, not an attack");
        }

        [Test]
        public void a_kicked_player_is_not_readmitted()
        {
            gacsrv.join("a");

            gac.kick("a", "test");

            gacsrv.join("a");

            Assert.That(gacsrv.net.cnt, Is.EqualTo(1), "the slot must still be known");
            Assert.That(gac.find("a"), Is.Not.Null, "but the player stays out");

            Assert.That(gacsrv.up("a", Vector3.zero, Vector3.zero, true, gactime.now), Is.False,
                "a kicked player must not be able to send state");
        }

        [Test]
        public void a_banned_player_is_not_readmitted()
        {
            gac.store = new gacbm();

            gacsrv.join("a");

            gac.ban("a", "test", 60f);

            gacsrv.leave("a");
            gacsrv.join("a");

            Assert.That(gac.find("a"), Is.Null, "a banned player must not come back");
        }

        [Test]
        public void the_slot_limit_holds()
        {
            for (int i = 0; i < gacp.slots + 5; i++) gacsrv.join("p" + i);

            Assert.That(gacsrv.net.cnt, Is.LessThanOrEqualTo(gacp.slots));

            for (int i = 0; i < gacp.slots + 5; i++)
            {
                gacsrv.up("p" + i, Vector3.zero, Vector3.zero, true, gactime.now);
            }

            Assert.That(gac.ply.Count, Is.LessThanOrEqualTo(gacp.slots + 1));
        }

        [Test]
        public void rtt_is_recorded_per_player()
        {
            gacsrv.join("a");
            gacsrv.join("b");

            gacsrv.rtt("a", 300f);

            Assert.That(gac.find("a").pin, Is.GreaterThan(0f));
            Assert.That(gac.find("b").pin, Is.EqualTo(0f));
        }

        [Test]
        public void a_manual_warning_still_fires()
        {
            int n = 0;

            gacs.onwarning = e => n++;

            gacsrv.warn("a", "manual");

            Assert.That(n, Is.EqualTo(1));
        }

        [Test]
        public void a_push_registers_an_unannounced_player()
        {
            Assert.That(gacsrv.up("ghost", Vector3.zero, Vector3.zero, true, gactime.now), Is.True,
                "a real server sees the packet before the join handshake completes");

            Assert.That(gac.find("ghost"), Is.Not.Null);
        }

        [Test]
        public void an_explicit_sequence_needs_no_join()
        {
            gacp.strict = true;

            Assert.That(gacsrv.up("ghost", Vector3.zero, Vector3.zero, true, gactime.now, 1), Is.True);
            Assert.That(gacsrv.up("ghost", Vector3.zero, Vector3.zero, true, gactime.now, 1), Is.False,
                "with strict on a replay is rejected");
        }
    }
}