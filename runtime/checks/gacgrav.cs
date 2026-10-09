namespace gtagac
{
    public sealed class gacgrav : gacc
    {
        readonly float[] dev = new float[gacp.slots];
        readonly int[] cnt = new int[gacp.slots];

        public gacgrav()
        {
            nm = "gravity";
            weight = 1f;
            cooldown = 0.8f;
        }

        public override void tick(gacd d, float dt)
        {
            int x = d.ind;

            if (x < 0 || x >= gacp.slots) return;
            if (d.ingnd() || d.clb || d.swm || d.grb) return;
            if (d.air < gacp.gmin) return;
            if (d.swg()) return;

            float vy = d.h.vel.y;
            float ex = d.gy;
            float hsp = gacmath.flen(d.h.now - d.h.at(1)) / gacmath.maxs(d.h.dlt(0), gacp.minid);

            if (hsp > gacp.swgsp) return;

            if (ex <= gacp.gtol || vy <= ex + gacp.gtol)
            {
                cnt[x] = 0;
                return;
            }

            dev[x] = gacmath.damp(dev[x], vy - ex, 8f, dt);
            cnt[x]++;

            if (cnt[x] * dt < gacp.grvhold) return;

            cnt[x] = 0;

            float cf = gacmath.cl01(gacmath.over(dev[x], gacp.maxvel) * 0.5f);

            if (cf < 0.15f) return;

            flg(d, cf, dev[x], "hang", "0.0");
        }
    }
}