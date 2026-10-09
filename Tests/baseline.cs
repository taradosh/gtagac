using gtagac;
using NUnit.Framework;

namespace gtagac.test
{
    public sealed class baseline
    {
        [Test]
        public void config_reset_gives_documented_defaults()
        {
            gacp.reset();

            Assert.That(gacp.maxspeed, Is.EqualTo(18f));
            Assert.That(gacp.maxvel, Is.EqualTo(20f));
            Assert.That(gacp.tpd, Is.EqualTo(3f));
            Assert.That(gacp.fth, Is.EqualTo(10f));
            Assert.That(gacp.ivl, Is.EqualTo(0.05f));
        }

        [Test]
        public void config_hash_is_stable_when_untouched()
        {
            gacp.reset();
            float a = gacp.hash();
            float b = gacp.hash();

            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void config_hash_changes_when_a_limit_changes()
        {
            gacp.reset();
            float before = gacp.hash();

            gacp.maxspeed = 99f;

            Assert.That(gacp.hash(), Is.Not.EqualTo(before));
        }

        [Test]
        public void config_toggle_round_trips()
        {
            gacp.reset();

            Assert.That(gacp.on("speed"), Is.True);

            gacp.sw("speed");

            Assert.That(gacp.on("speed"), Is.False);

            gacp.sw("speed");

            Assert.That(gacp.on("speed"), Is.True);
        }

        [Test]
        public void weight_table_covers_every_registered_check()
        {
            gacp.reset();

            Assert.That(gacr.wgt.of("speed"), Is.EqualTo(1f));
            Assert.That(gacr.wgt.of("teleport"), Is.EqualTo(3f));
            Assert.That(gacr.wgt.of("velocity"), Is.EqualTo(2f));
            Assert.That(gacr.wgt.of("position"), Is.EqualTo(2f));
            Assert.That(gacr.wgt.of("rate"), Is.EqualTo(2f));
            Assert.That(gacr.wgt.of("consistency"), Is.EqualTo(2f));
        }

        [Test]
        public void whitelist_lookup_is_case_sensitive_on_id()
        {
            gacp.reset();
            gacp.wl = new[] { "Admin" };

            Assert.That(gacp.adm("Admin"), Is.True);
            Assert.That(gacp.adm("admin"), Is.False);
            Assert.That(gacp.adm("bob"), Is.False);
        }

        [Test]
        public void debug_output_is_off_unless_forced()
        {
            gacp.reset();
            gacp.dbg = true;

            Assert.That(gacp.dbgo(), Is.EqualTo(UnityEngine.Debug.isDebugBuild));

            gacp.dbgf = true;

            Assert.That(gacp.dbgo(), Is.True);
        }

        [Test]
        public void whitelist_changes_the_config_hash()
        {
            gacp.reset();
            gacp.wl = new[] { "a" };
            float before = gacp.hash();

            gacp.wl = new[] { "a", "b" };

            Assert.That(gacp.hash(), Is.Not.EqualTo(before));
        }
    }
}