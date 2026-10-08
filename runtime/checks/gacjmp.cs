using UnityEngine;

namespace gtagac
{
    public sealed class gacjmp : gacc
    {
        public gacjmp()
        {
            nm = "jump";
            weight = 1f;
            cooldown = 0.5f;
        }

        public override void tick(gacd d, float dt)
        {
            if (d.h.n < 2) return;

            float vy = d.h.vel.y;

            if (d.ingnd())
            {
                d.jy = vy;
                return;
            }

            float jv = Mathf.Abs(vy);

            if (jv <= gacp.maxjmp) return;

            float dl = Mathf.Abs(d.h.now.y - d.h.at(1).y);
            float dlt = d.h.dlt(0);

            if (dlt <= gacp.minid) return;

            float sp = dl / dlt;
            float lim = gacp.maxjmp * 1.5f;

            if (sp <= lim) return;

            float c1 = gacmath.cl01(gacmath.over(sp, lim) * 0.6f);
            if (c1 < 0.15f) return;

            d.jy = vy;

            flg(d, c1, "jmp " + sp.ToString("0.0"));
        }
    }
}