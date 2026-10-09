using UnityEngine;

namespace gtagac
{
    public sealed class gacnc : gacc
    {
        readonly float[] bad = new float[gacp.slots];
        readonly float[] ok = new float[gacp.slots];

        RaycastHit hit;

        public gacnc()
        {
            nm = "noclip";
            weight = 3f;
            cooldown = 1f;
        }

        public override void tick(gacd d, float dt)
        {
            int x = d.ind;

            if (x < 0 || x >= gacp.slots) return;
            if (d.h.n < 2) return;
            if (d.grb) return;
            if (d.grc > 0f) return;
            if (d.clb || d.swm) return;
            if (!gacp.srv && !gacp.ncloc) return;
            if (gacp.ncmask == 0) return;

            float dlt = d.h.dlt(0);

            if (dlt <= gacp.minid) return;
            if (dlt > gacp.maxid) return;
            if (d.unc > gacp.conunc) return;

            Vector3 a = d.h.at(1);

            Vector3 seg = d.h.now - a;

            float len = gacmath.flen(seg);

            if (len < gacp.minid) return;

            Vector3 dir = seg / len;

            float org = gacp.nccap;
            float far = len + gacp.ncahead - org;

            if (far <= 0f) return;

            bool h1 = Physics.Raycast(a + dir * org, dir, out hit, far, gacp.ncmask, QueryTriggerInteraction.Ignore);

            if (h1 && Vector3.Dot(hit.normal, dir) > -gacp.ncdot) h1 = false;

            if (!h1)
            {
                if (ok[x] >= gacp.ncviol) { bad[x] = 0f; ok[x] = 0f; }
                else ok[x] += 1f;

                return;
            }

            ok[x] = 0f;
            bad[x] += 1f;

            if (bad[x] < gacp.ncsmpl) return;

            bad[x] = 0f;

            flg(d, gacp.nccf, hit.distance, "wall", "0.00");
        }
    }
}