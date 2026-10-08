using UnityEngine;

namespace gtagac
{
    public sealed class gactp : gacc
    {
        public gactp()
        {
            nm = "teleport";
            weight = 3f;
            cooldown = 0.4f;
        }

        public override void tick(gacd d, float dt)
        {
            if (d.h.n < 2) return;

            float dl = gacmath.len(d.h.now - d.h.at(1));
            float dlt = d.h.dlt(0);

            if (dlt <= gacp.minid) return;

            float sp = dl / dlt;

            float lim = gacp.tpd;

            float cf = 0f;

            if (dl > lim)
            {
                cf = gacmath.cl01(gacmath.over(dl, lim) * 0.5f);
            }
            else if (sp > gacp.maxvel * 2f)
            {
                cf = gacmath.cl01(gacmath.over(sp, gacp.maxvel * 2f) * 0.35f);
            }
            else
            {
                return;
            }

            if (cf < 0.1f) return;

            int ov = 0;
            int k = gacp.tps;
            for (int i = 0; i < k; i++)
            {
                if (gacmath.len(d.h.at(i) - d.h.at(i + 1)) > lim) ov++;
            }

            if (ov == 0 && dl < lim * 1.6f) return;

            d.hsp = sp;

            flg(d, cf, "dist " + dl.ToString("0.00"));
        }
    }
}