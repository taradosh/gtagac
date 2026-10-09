using UnityEngine;

namespace gtagac
{
    public sealed class gacfly : gacc
    {
        readonly float[] hov = new float[gacp.slots];

        public gacfly()
        {
            nm = "fly";
            weight = 1f;
            cooldown = 1f;
        }

        public override void tick(gacd d, float dt)
        {
            int x = d.ind;

            if (x < 0 || x >= gacp.slots) return;
            if (d.clb || d.swm || d.grb) return;

            if (d.ingnd())
            {
                d.air = 0f;
                d.fy = false;
                hov[x] = 0f;
                return;
            }

            d.air += dt;

            if (d.air <= gacp.maxair)
            {
                d.fy = false;
                hov[x] = 0f;
                return;
            }

            float vy = d.h.vel.y;
            float dlt = d.h.dlt(0);
            float sp = gacmath.flen(d.h.now - d.h.at(1)) / (dlt > gacp.minid ? dlt : gacp.minid);

            bool still = vy > -gacp.hov && vy < gacp.hov && sp < gacp.maxspeed * 0.4f;

            hov[x] = still ? hov[x] + dt : 0f;

            if (hov[x] < gacp.flyhold) return;

            float ov = gacmath.over(d.air, gacp.maxair);
            float cf = gacmath.cl01(ov * 0.35f);

            if (cf < 0.15f) return;

            hov[x] = 0f;
            d.fy = true;

            flg(d, cf, d.air, "air", "0.0");
        }
    }
}