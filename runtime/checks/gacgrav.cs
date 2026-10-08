namespace gtagac
{
    public sealed class gacgrav : gacc
    {
        int cnt;

        public gacgrav()
        {
            nm = "gravity";
            weight = 1f;
            cooldown = 0.8f;
        }

        public override void tick(gacd d, float dt)
        {
            if (d.ingnd() || d.clb || d.swm || d.grb) return;
            if (d.air < gacp.gmin) return;
            if (d.swg()) return;

            float vy = d.h.vel.y;
            float ex = d.gy;

            if (ex > gacp.gtol && vy > ex + gacp.gtol)
            {
                float cf = gacmath.cl01(gacmath.over(vy - ex, gacp.maxvel) * 0.5f);
                if (cf > 0.15f)
                {
                    cnt = 0;
                    flg(d, cf, "hang " + vy.ToString("0.0"));
                    return;
                }
            }

            if (d.air > gacp.maxair * 0.5f && vy < gacp.gtol && d.gy < -gacp.maxvel * 0.5f)
            {
                cnt++;
                if (cnt >= 3)
                {
                    cnt = 0;
                    flg(d, 0.4f, "nograv");
                }
            }
            else
            {
                cnt = 0;
            }
        }
    }
}