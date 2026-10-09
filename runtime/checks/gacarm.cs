using UnityEngine;

namespace gtagac
{
    public sealed class gacarm : gacc
    {
        const int cap = 16;

        readonly float[][] hs = new float[gacp.slots][];
        readonly int[] hd = new int[gacp.slots];
        readonly Vector3[] pl = new Vector3[gacp.slots];
        readonly Vector3[] pr = new Vector3[gacp.slots];
        readonly bool[] il = new bool[gacp.slots];
        readonly bool[] ir = new bool[gacp.slots];

        public gacarm()
        {
            nm = "arm";
            weight = 1f;
            cooldown = 0.6f;
        }

        public override void tick(gacd d, float dt)
        {
            int x = d.ind;

            if (x < 0 || x >= gacp.slots) return;
            if (d.h.n < 2) return;

            if (hs[x] == null) hs[x] = new float[cap];

            float dlt = d.h.dlt(0);
            if (dlt <= gacp.minid) return;

            float a = 0f;

            if (d.hl != null)
            {
                Vector3 p = d.hl.position;
                if (il[x]) a = gacmath.len(p - pl[x]) / dlt;
                pl[x] = p;
                il[x] = true;
            }

            if (d.hr != null)
            {
                Vector3 p = d.hr.position;
                if (ir[x])
                {
                    float s = gacmath.len(p - pr[x]) / dlt;
                    if (s > a) a = s;
                }
                pr[x] = p;
                ir[x] = true;
            }

            if (a <= 0f) return;

            int h = (hd[x] + 1) % cap;
            hd[x] = h;
            hs[x][h] = a;

            d.hs = a;

            if (a <= gacp.maxhand) return;

            int k = gacp.wnd;
            int ov = 0;

            for (int i = 0; i < k; i++)
            {
                int j = h - i;
                if (j < 0) j += cap;
                if (hs[x][j] > gacp.maxhand) ov++;
            }

            if (ov < gacp.smp) return;

            float cf = gacmath.cl01(gacmath.over(a, gacp.maxhand) * (float)ov / k);
            if (cf < 0.15f) return;

            flg(d, cf, a, "hand", "0.0");
        }
    }
}