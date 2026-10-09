using gtagac;
using NUnit.Framework;

namespace gtagac.test
{
    [TestFixture]
    public class evidence
    {
        [Test]
        public void an_empty_player_has_no_evidence()
        {
            tst.boot();

            Assert.That(gacr.evind("nobody"), Is.EqualTo(0));
            Assert.That(gacr.cfof("nobody"), Is.EqualTo(0f));
        }

        [Test]
        public void the_gate_is_transparent_when_off()
        {
            tst.boot();

            gacp.evon = false;

            Assert.That(gacr.evok("nobody"), Is.True, "an unknown player must not be blocked when the gate is off");
        }

        [Test]
        public void an_unknown_player_is_blocked_when_the_gate_is_on()
        {
            tst.boot();

            gacp.evon = true;

            Assert.That(gacr.evok("nobody"), Is.False);
        }

        [Test]
        public void confidence_below_the_bar_blocks_the_gate()
        {
            tst.boot();

            gacp.evon = true;
            gacp.evmin = 1;
            gacp.evcf = 0.5f;

            gacper p = gacr.get("z");

            p.f.add("speed", 1, 1f, 0.2f, gactime.now, 1f);

            Assert.That(gacr.evind("z"), Is.EqualTo(1), "one check is enough here");
            Assert.That(gacr.cfof("z"), Is.LessThan(0.5f), "confidence is below the bar");
            Assert.That(gacr.evok("z"), Is.False, "confidence must gate");
        }

        [Test]
        public void enough_confidence_and_checks_pass_the_gate()
        {
            tst.boot();

            gacp.evon = true;
            gacp.evmin = 2;
            gacp.evcf = 0.3f;

            gacper p = gacr.get("z");

            p.f.add("speed", 1, 1f, 0.9f, gactime.now, 1f);
            p.f.add("noclip", 1, 3f, 0.9f, gactime.now, 1f);

            Assert.That(gacr.evind("z"), Is.EqualTo(2));
            Assert.That(gacr.cfof("z"), Is.GreaterThanOrEqualTo(0.3f));
            Assert.That(gacr.evok("z"), Is.True);
        }

        [Test]
        public void one_check_of_two_does_not_pass()
        {
            tst.boot();

            gacp.evon = true;
            gacp.evmin = 2;
            gacp.evcf = 0.3f;

            gacper p = gacr.get("z");

            p.f.add("speed", 1, 1f, 0.9f, gactime.now, 1f);

            Assert.That(gacr.evok("z"), Is.False);
        }

        [Test]
        public void one_check_several_times_is_still_one()
        {
            var f = new gacf();

            f.add("speed", 1, 1f, 0.5f, 10f, 1f);
            f.add("speed", 1, 1f, 0.5f, 11f, 1f);
            f.add("speed", 1, 1f, 0.5f, 12f, 1f);

            Assert.That(f.ind(13f, 30f), Is.EqualTo(1), "three flags of one check are one piece of evidence");
            Assert.That(f.total(), Is.EqualTo(3));
        }

        [Test]
        public void two_checks_are_two()
        {
            var f = new gacf();

            f.add("speed", 1, 1f, 0.5f, 10f, 1f);
            f.add("noclip", 1, 3f, 0.5f, 11f, 1f);

            Assert.That(f.ind(12f, 30f), Is.EqualTo(2));
            Assert.That(f.ncf(), Is.EqualTo(2), "both still hold live evidence");
        }

        [Test]
        public void the_window_drops_stale_checks()
        {
            var f = new gacf();

            f.add("speed", 1, 1f, 0.5f, 10f, 1f);
            f.add("noclip", 1, 3f, 0.5f, 100f, 1f);

            Assert.That(f.ind(105f, 30f), Is.EqualTo(1), "only the recent check counts");
            Assert.That(f.ind(200f, 30f), Is.EqualTo(0));
        }

        [Test]
        public void decay_drops_live_evidence_but_keeps_the_record()
        {
            var f = new gacf();

            f.add("speed", 1, 1f, 0.5f, 10f, 1f);

            f.decay(0.6f, 5f, 20f);

            Assert.That(f.ncf(), Is.EqualTo(0), "live evidence decays away");
            Assert.That(f.ind(20f, 30f), Is.EqualTo(1), "the record of what fired is kept");
            Assert.That(f.total(), Is.EqualTo(0));
        }

        [Test]
        public void a_reset_clears_everything()
        {
            var f = new gacf();

            f.add("speed", 1, 1f, 0.5f, 10f, 1f);
            f.add("noclip", 1, 3f, 0.5f, 10f, 1f);

            f.reset();

            Assert.That(f.ind(11f, 30f), Is.EqualTo(0));
            Assert.That(f.len, Is.EqualTo(0));
            Assert.That(f.total(), Is.EqualTo(0));
            Assert.That(f.conf(), Is.EqualTo(0f));
        }
    }
}