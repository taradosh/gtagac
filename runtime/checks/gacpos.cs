using UnityEngine;

namespace gtagac
{
    public sealed class gacpos : gacc
    {
        readonly float[] err = new float[gacp.slots];
        readonly float[] cnt = new float[gacp.slots];

        public gacpos()
        {
            nm = "position";
            weight = 2f;
            cooldown = 0.5f;
        }

        public override void tick(gacd d, float dt)
        {
            int x = d.ind;

            if (x < 0 || x >= gacp.slots) return;
            if (d.h.n < 2) return;
            if (d.grb) return;

            float dlt = d.h.dlt(0);

            if (dlt <= gacp.minid) return;
            if (dlt > gacp.maxid) return;

            Vector3 vel = d.h.vel;

            Vector3 step = vel * dlt;

            Vector3 e = d.h.now - d.h.at(1) - step;

            float m = gacmath.len(e);

            err[x] = gacmath.damp(err[x], m, 10f, dt);

            float tol = (gacp.maxspeed + gacp.maxvel) * dlt + d.slack;

            if (err[x] <= tol) return;

            cnt[x] += dt;

            if (cnt[x] < gacp.poshold) return;

            float cf = gacmath.cl01(gacmath.over(err[x], tol) * 0.5f);

            if (cf < 0.2f) return;

            cnt[x] = 0f;

            flg(d, cf, err[x], "pverr", "0.00");
        }
    }
}