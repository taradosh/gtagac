namespace gtagac
{
    public sealed class gacpos : gacc
    {
        const int cap = 8;

        readonly float[] hs = new float[gacp.slots * cap];
        readonly int[] hd = new int[gacp.slots];

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

            float s = gacmath.len(d.h.now - d.h.at(1)) / dlt;

            int o = x * cap;
            int h = (hd[x] + 1) % cap;
            hd[x] = h;
            hs[o + h] = s;

            if (s <= gacp.maxspeed * gacp.burst) return;

            int k = gacp.wnd;
            int ov = 0;
            float av = 0f;
            float mx = 0f;

            for (int i = 0; i < k; i++)
            {
                int j = h - i;
                if (j < 0) j += cap;
                float v = hs[o + j];
                av += v;
                if (v > mx) mx = v;
                if (v > gacp.maxspeed) ov++;
            }

            av /= k;

            if (ov < gacp.smp) return;
            if (mx < gacp.tpd * 0.5f) return;

            float cf = gacmath.cl01(gacmath.over(av, gacp.maxspeed) * (float)ov / k);
            if (cf < 0.15f) return;

            flg(d, cf, "pos " + mx.ToString("0.0"));
        }
    }
}