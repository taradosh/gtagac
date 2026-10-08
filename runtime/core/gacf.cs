using UnityEngine;

namespace gtagac
{
    public struct gacfe
    {
        public string chk;
        public int n;
        public float w;
        public float cf;
        public float cnt;
        public float t;
        public float cd;
    }

    public sealed class gacf
    {
        public const int cap = 16;

        readonly gacfe[] e = new gacfe[cap];
        int used;

        public int len
        {
            get { return used; }
        }

        public gacf()
        {
            reset();
        }

        public void reset()
        {
            for (int i = 0; i < cap; i++)
            {
                e[i].chk = null;
                e[i].n = 0;
                e[i].w = 0f;
                e[i].cf = 0f;
                e[i].cnt = 0f;
                e[i].t = -999f;
                e[i].cd = -999f;
            }
            used = 0;
        }

        int fnd(string chk)
        {
            for (int i = 0; i < used; i++)
            {
                if (e[i].chk == chk) return i;
            }
            return -1;
        }

        public bool cdn(string chk, float t)
        {
            int i = fnd(chk);
            return i < 0 || t >= e[i].cd;
        }

        public void add(string chk, int n, float w, float cf, float t, float cd)
        {
            int i = fnd(chk);
            if (i < 0)
            {
                if (used >= cap) i = 0;
                else i = used++;
                e[i].chk = chk;
                e[i].n = 0;
                e[i].cnt = 0f;
                e[i].w = w;
                e[i].cf = 0f;
                e[i].t = t;
                e[i].cd = t + cd;
            }
            else
            {
                e[i].n += n;
                e[i].cnt += n;
                e[i].w = w;
                e[i].t = t;
                e[i].cd = t + cd;
            }
            if (cf > e[i].cf) e[i].cf = cf;
        }

        public void decay(float rate, float dt, float t)
        {
            if (rate <= 0f) return;
            for (int i = 0; i < used; i++)
            {
                if (e[i].n <= 0) continue;
                if (t - e[i].t <= gacp.actw) continue;
                e[i].cnt -= rate * dt;
                e[i].n = e[i].cnt > 0f ? (int)e[i].cnt : 0;
            }
        }

        public int cnt(string chk)
        {
            int i = fnd(chk);
            return i < 0 ? 0 : e[i].n;
        }

        public float conf(string chk)
        {
            int i = fnd(chk);
            return i < 0 ? 0f : e[i].cf;
        }

        public int total()
        {
            int s = 0;
            for (int i = 0; i < used; i++) s += e[i].n * (int)e[i].w;
            return s;
        }

        public float conf()
        {
            float s = 0f;
            int n = 0;
            for (int i = 0; i < used; i++)
            {
                if (e[i].n <= 0) continue;
                s += e[i].cf;
                n++;
            }
            return n > 0 ? gacmath.cl01(s / n + (n > 1 ? 0.1f * (n - 1) : 0f)) : 0f;
        }

        public int act(float t, float w)
        {
            int s = 0;
            for (int i = 0; i < used; i++)
            {
                if (e[i].n > 0 && t - e[i].t <= w) s++;
            }
            return s;
        }

        public gacfe at(int i)
        {
            return e[i];
        }
    }
}