namespace gtagac
{
    public sealed class gacspd : gacc
    {
        public gacspd()
        {
            nm = "speed";
            weight = 1f;
            cooldown = 0.6f;
        }

        public override void tick(gacd d, float dt)
        {
            if (d.h.n < 3) return;
            if (d.grb) return;

            int k = gacp.wnd;
            float lim = gacp.maxspeed + d.sspd;

            float av = 0f;

            for (int i = 0; i < k; i++)
            {
                av += d.h.fspd(i);
            }

            av /= k;

            d.spd = av;

            if (av <= lim * gacp.burst) return;

            int ov = 0;

            for (int i = 0; i < k; i++)
            {
                if (d.h.fspd(i) > lim) ov++;
            }

            if (ov < gacp.smp) return;

            float ex = gacmath.over(av, lim);

            if (ex <= 0.05f) return;

            float cf = gacmath.cl01(ex * (float)ov / k);

            flg(d, cf, av, "hspd", "0.0");
        }
    }
}