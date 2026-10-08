using UnityEngine;

namespace gtagac
{
    public sealed class gacfly : gacc
    {
        public gacfly()
        {
            nm = "fly";
            weight = 1f;
            cooldown = 1f;
        }

        public override void tick(gacd d, float dt)
        {
            if (d.clb || d.swm || d.grb) return;

            if (d.ingnd())
            {
                d.air = 0f;
                d.fy = false;
                return;
            }

            d.air += dt;

            if (d.air <= gacp.maxair)
            {
                d.fy = false;
                return;
            }

            float vy = d.h.vel.y;
            float dlt = d.h.dlt(0);
            float sp = gacmath.flen(d.h.now - d.h.at(1)) / (dlt > gacp.minid ? dlt : gacp.minid);

            bool rise = vy > gacp.hov;
            bool fall = vy < -gacp.hov;
            bool move = sp > gacp.maxspeed * 0.4f;

            if (rise || fall || move) return;

            float ov = gacmath.over(d.air, gacp.maxair);
            float cf = gacmath.cl01(ov * 0.35f);

            if (cf < 0.15f) return;

            d.fy = true;

            flg(d, cf, "air " + d.air.ToString("0.0"));
        }
    }
}