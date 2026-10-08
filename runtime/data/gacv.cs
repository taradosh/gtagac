using UnityEngine;

namespace gtagac
{
    public sealed class gacv
    {
        public const int cap = 32;

        public readonly Vector3[] p = new Vector3[cap];
        public readonly Vector3[] v = new Vector3[cap];
        public readonly float[] t = new float[cap];

        public int n;
        public int hd;

        public Vector3 now
        {
            get { return p[hd]; }
        }

        public Vector3 vel
        {
            get { return v[hd]; }
        }

        public void clear()
        {
            for (int i = 0; i < cap; i++)
            {
                p[i] = Vector3.zero;
                v[i] = Vector3.zero;
                t[i] = 0f;
            }
            n = 0;
            hd = 0;
        }

        public void add(Vector3 pos, Vector3 vv, float ts)
        {
            hd = (hd + 1) % cap;
            p[hd] = pos;
            v[hd] = vv;
            t[hd] = ts;
            if (n < cap) n++;
        }

        public Vector3 at(int i)
        {
            if (i < 0 || i >= n) return p[hd];
            int k = hd - i;
            if (k < 0) k += cap;
            return p[k];
        }

        public Vector3 vat(int i)
        {
            if (i < 0 || i >= n) return v[hd];
            int k = hd - i;
            if (k < 0) k += cap;
            return v[k];
        }

        public float tat(int i)
        {
            if (i < 0 || i >= n) return t[hd];
            int k = hd - i;
            if (k < 0) k += cap;
            return t[k];
        }

        public float dlt(int i)
        {
            if (i < 0 || i + 1 >= n) return 0f;
            return tat(i) - tat(i + 1);
        }

        public float win(int k)
        {
            if (n < 2) return 0f;
            int c = k < n ? k : n;
            return tat(0) - tat(c - 1);
        }

        public float spd(int i)
        {
            float d = dlt(i);
            return d > gacp.minid ? gacmath.len(p[ix(i)] - p[ix(i + 1)]) / d : 0f;
        }

        public float fspd(int i)
        {
            float d = dlt(i);
            return d > gacp.minid ? gacmath.flen(p[ix(i)] - p[ix(i + 1)]) / d : 0f;
        }

        public float avs(int k)
        {
            if (n < 2) return 0f;
            int c = k < n ? k : n;
            float w = tat(0) - tat(c - 1);
            if (w <= gacp.minid) return 0f;
            return gacmath.len(p[ix(0)] - p[ix(c - 1)]) / w;
        }

        public float afs(int k)
        {
            if (n < 2) return 0f;
            int c = k < n ? k : n;
            float w = tat(0) - tat(c - 1);
            if (w <= gacp.minid) return 0f;
            return gacmath.flen(p[ix(0)] - p[ix(c - 1)]) / w;
        }

        int ix(int i)
        {
            int k = hd - i;
            if (k < 0) k += cap;
            return k;
        }
    }
}