using UnityEngine;

namespace gtagac
{
    public sealed class gaccon : gacc
    {
        readonly float[] dev = new float[gacp.slots];
        readonly float[] cnt = new float[gacp.slots];

        public gaccon()
        {
            nm = "consistency";
            weight = 2f;
            cooldown = 0.6f;
        }

        public override void tick(gacd d, float dt)
        {
            int x = d.ind;

            if (x < 0 || x >= gacp.slots) return;
            if (d.h.n < 3) return;
            if (d.grb) return;
            if (d.grnd) return;
            if (d.clb || d.swm) return;

            float d0 = d.h.dlt(0);
            float d1 = d.h.dlt(1);

            if (d0 <= gacp.minid || d1 <= gacp.minid) return;
            if (d0 > gacp.maxid || d1 > gacp.maxid) return;
            if (d.unc > gacp.conunc) return;

            float mn = gacp.maxid;
            float mx = gacp.minid;

            for (int i = 0; i < 4; i++)
            {
                float q = d.h.dlt(i);

                if (q <= gacp.minid || q > gacp.maxid) return;

                if (q < mn) mn = q;
                if (q > mx) mx = q;
            }

            if (mx > mn * gacp.conratio) return;

            Vector3 r0 = (d.h.at(0) - d.h.at(1)) / d0;
            Vector3 r1 = (d.h.at(1) - d.h.at(2)) / d1;

            Vector3 a0 = (r0 - r1) / ((d0 + d1) * 0.5f);

            dev[x] = gacmath.damp(dev[x], gacmath.len(a0), 8f, dt);

            if (dev[x] <= gacp.conacc) return;

            cnt[x] += 1f;

            if (cnt[x] < gacp.consmp) return;

            float cf = gacmath.cl01(gacmath.over(dev[x], gacp.conacc) * 0.5f);

            if (cf < 0.15f) return;

            cnt[x] = 0f;

            flg(d, cf, dev[x], "acc", "0");
        }
    }
}