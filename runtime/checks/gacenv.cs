using UnityEngine;

namespace gtagac
{
    public sealed class gacenv : gacc
    {
        float hs;
        float sc;
        int cc;
        int cv;

        public gacenv()
        {
            nm = "env";
            weight = 4f;
            cooldown = 2f;
            hs = gacp.hash();
            sc = Time.timeScale;
        }

        public override void tick(gacd d, float dt)
        {
            float h = gacp.hash();

            if (h != hs)
            {
                hs = h;
                cc++;
                if (cc >= 2)
                {
                    cc = 0;
                    flg(d, 0.9f, "cfg");
                }
                return;
            }

            float t = Time.timeScale;

            if (!gacp.allowsc && t != sc)
            {
                sc = t;
                cv++;
                if (cv >= 2)
                {
                    cv = 0;
                    flg(d, 0.8f, "scale " + t.ToString("0.00"));
                }
                return;
            }

            float an = gactime.anom();

            if (an <= 0.25f) return;

            flg(d, an, "time " + gactime.dt.ToString("0.000"));
        }
    }
}