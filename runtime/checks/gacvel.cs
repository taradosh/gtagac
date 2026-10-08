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

            float lim = gacp.maxvel;

            if (fh > lim)
            {
                float c1 = gacmath.cl01(gacmath.over(fh, lim) * 0.5f);
                if (c1 > 0.1f)
                {
                    flg(d, c1, "hvel " + fh.ToString("0.0"));
                    return;
                }
            }

            if (fy > lim * 0.9f || fy < -lim * 1.2f)
            {
                float r = fy > 0f ? gacmath.over(fy, lim * 0.9f) : gacmath.over(-fy, lim * 1.2f);
                float c2 = gacmath.cl01(r * 0.6f);
                if (c2 > 0.15f)
                {
                    flg(d, c2, "yvel " + fy.ToString("0.0"));
                    return;
                }
            }

            float dlt = d.h.dlt(0);
            if (dlt <= gacp.minid) return;

            float rv = gacmath.len(v - d.h.vat(1)) / dlt;

            if (rv <= gacp.maxacc) return;

            float c3 = gacmath.cl01(gacmath.over(rv, gacp.maxacc) * 0.5f);
            if (c3 < 0.2f) return;

            flg(d, c3, "acc " + rv.ToString("0.0"));
        }
    }
}