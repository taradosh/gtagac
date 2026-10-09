using UnityEngine;

namespace gtagac
{
    public sealed class gacvel : gacc
    {
        public gacvel()
        {
            nm = "velocity";
            weight = 2f;
            cooldown = 0.5f;
        }

        public override void tick(gacd d, float dt)
        {
            if (d.h.n < 2) return;
            if (d.grb) return;

            Vector3 v = d.h.vel;

            float fh = gacmath.flen(v);
            float fy = v.y;

            float lim = gacp.maxvel + d.sspd;

            float ylim = lim + gacp.grav * Mathf.Max(0f, d.air);

            if (fh > lim)
            {
                float c1 = gacmath.cl01(gacmath.over(fh, lim) * 0.5f);
                if (c1 > 0.1f)
                {
                    flg(d, c1, fh, "hvel", "0.0");
                    return;
                }
            }

            if (fy > lim * 0.9f || fy < -ylim)
            {
                float r = fy > 0f ? gacmath.over(fy, lim * 0.9f) : gacmath.over(-fy, ylim);
                float c2 = gacmath.cl01(r * 0.6f);
                if (c2 > 0.15f)
                {
                    flg(d, c2, fy, "yvel", "0.0");
                    return;
                }
            }
        }
    }
}