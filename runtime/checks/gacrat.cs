namespace gtagac
{
    public sealed class gacrat : gacc
    {
        readonly float[] win = new float[gacp.slots];

        public gacrat()
        {
            nm = "rate";
            weight = 2f;
            cooldown = 1f;
        }

        public override void tick(gacd d, float dt)
        {
            int x = d.ind;
            if (x < 0 || x >= gacp.slots) return;

            float t = gactime.now;

            if (win[x] <= 0f)
            {
                win[x] = t + 1f;
                d.upd = 0f;
                return;
            }

            if (t < win[x]) return;

            float r = d.upd / 1f;

            win[x] = t + 1f;
            d.upd = 0f;

            if (r <= gacp.maxupd) return;

            float cf = gacmath.cl01(gacmath.over(r, gacp.maxupd) * 0.5f);
            if (cf < 0.2f) return;

            flg(d, cf, "rate " + r.ToString("0"));
        }
    }
}