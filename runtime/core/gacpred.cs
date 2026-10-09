using UnityEngine;

namespace gtagac
{
    public sealed class gacpc
    {
        public bool ok;
        public Vector3 p;
        public Vector3 v;
        public Vector3 off;
        public Vector3 last;
        public float step;
        public float cstep;
        public float err;
        public float peak;
        public float corr;

        public void reset()
        {
            ok = false;
            p = Vector3.zero;
            v = Vector3.zero;
            off = Vector3.zero;
            last = Vector3.zero;
            step = 0f;
            cstep = 0f;
            err = 0f;
            peak = 0f;
            corr = 0f;
        }

        public void basep()
        {
            reset();
        }
    }

    public static class gacpred
    {
        public static void clr()
        {
            if (pcc == null) return;

            for (int i = 0; i < pcc.Length; i++)
            {
                if (pcc[i] != null) pcc[i].reset();
            }
        }

public static void drop(string id)
        {
            gacd d = gac.find(id);

            if (d != null) drop(d);
        }

        public static void drop(gacd d)
        {
            if (d == null) return;

            gacpc s = of(d.ind);

            if (s != null) s.reset();
        }

        public static gacpc of(int slot)
        {
            if (slot < 0 || slot >= gacp.slots) return null;
            if (pcc == null) pcc = new gacpc[gacp.slots];

            if (pcc[slot] == null) pcc[slot] = new gacpc();

            return pcc[slot];
        }

        static gacpc[] pcc;

        public static gacpc st(gacd d)
        {
            return d == null ? null : of(d.ind);
        }

        public static void step(gacd d, Vector3 p, Vector3 v, float dt)
        {
            if (d == null) return;

            gacpc s = of(d.ind);

            if (s == null) return;

            if (!s.ok || dt <= gacp.minid || dt > gacp.maxid)
            {
                s.ok = true;
                s.p = p;
                s.v = v;
                s.off = Vector3.zero;
                s.last = p;
                s.step = 0f;
                s.cstep = 0f;
                s.err = 0f;
                s.peak = 0f;
                s.corr = 0f;
                return;
            }

            Vector3 sv = gacmath.clv(v, gacp.maxvel);

            Vector3 po = s.off;

            s.p += sv * dt;

            Vector3 e = p - s.p;

            float m = gacmath.len(e);

            if (m > gacp.prdmax) e *= gacp.prdmax / m;

            s.off += e;

            float om = gacmath.len(s.off);

            if (om > gacp.prdmaxoff)
                s.off *= gacp.prdmaxoff / om;

            s.p = p;
            s.v = sv;

            s.off = gacmath.damp3(s.off, Vector3.zero, gacp.prdrate, dt);

            Vector3 vis = p + s.off;

            s.step = gacmath.len(vis - s.last);
            s.cstep = gacmath.len(s.off - po);
            s.last = vis;

            s.err = m;

            if (m > s.peak) s.peak = m;

            if (gacmath.len(e) > s.corr) s.corr = gacmath.len(e);
        }

        public static Vector3 pos(string id)
        {
            gacd d = gac.find(id);

            if (d == null) return Vector3.zero;

            return pos(d);
        }

        public static Vector3 pos(gacd d)
        {
            gacpc s = st(d);

            if (s == null || !s.ok) return d.p;

            return d.p + s.off;
        }

        public static Vector3 off(string id)
        {
            gacd d = gac.find(id);

            if (d == null) return Vector3.zero;

            gacpc s = st(d);

            return s == null ? Vector3.zero : s.off;
        }

        public static float err(string id)
        {
            gacd d = gac.find(id);

            gacpc s = st(d);

            return s == null ? 0f : s.err;
        }

        public static float peak(string id)
        {
            gacd d = gac.find(id);

            gacpc s = st(d);

            return s == null ? 0f : s.peak;
        }

        public static float cstep(string id)
        {
            gacd d = gac.find(id);

            gacpc s = st(d);

            return s == null ? 0f : s.cstep;
        }

        public static bool on()
        {
            return gacp.pr;
        }
    }
}