using UnityEngine;

namespace gtagac
{
    public static class gacsq
    {
        public static bool acc(gacd d, int seq, double ct, float now)
        {
            float dt = d.lt > 0f ? now - d.lt : 0f;

            if (d.sqo && dt > 0.0001f && dt < 2f)
                d.jit = gacmath.damp(d.jit, Mathf.Abs(dt - d.lst), 4f, dt);

            if (dt > 0.0001f) d.lst = dt;

            d.lt = now;

            if (!d.sqo)
            {
                d.sqo = true;
                d.sq = seq;
                d.off = ct > 0 ? now - ct : 0.0;
                d.jit = 0f;
                d.lst = dt;
                bud(d);
                return true;
            }

            int gap = seq - d.sq - 1;

            if (gap > 0)
            {
                d.gap += gap;
                d.lost += gap;
                d.gl = gacmath.damp(d.gl, gap, 3f, dt);
            }
            else if (gap < 0)
            {
                if (gacp.strict)
                {
                    d.ol++;
                    d.drp++;

                    if (gacp.logn) gaclog.n(d.id, "seq replay " + seq);

                    return false;
                }
            }
            else
            {
                d.gl = gacmath.damp(d.gl, 0f, 3f, dt);
            }

            if (gacp.strict)
            {
                if (ct > 0.0)
                {
                    float lag = (float)(now - d.off) - (float)ct;

                    if (lag > gacp.stale)
                    {
                        d.ol++;
                        d.drp++;

                        if (gacp.logn) gaclog.n(d.id, "stale " + gacn.f(lag, "0.000"));

                        return false;
                    }

                    if (lag < -gacp.fut)
                    {
                        d.ol++;
                        d.drp++;

                        if (gacp.logn) gaclog.n(d.id, "future " + gacn.f(lag, "0.000"));

                        return false;
                    }

                    d.lag = gacmath.damp(d.lag, Mathf.Max(0f, lag), 4f, dt);
                }
            }

            if (gap > gacp.seqgap)
            {
                d.sus++;

                if (gacp.logn) gaclog.n(d.id, "gap " + gap);

                bud(d);
            }

            d.sq = seq;

            if (ct > 0.0) d.lc = ct;

            bud(d);

            return true;
        }

        public static void bud(gacd d)
        {
            float un = d.jit * gacp.jitter + Mathf.Min(d.gl, 8f) * Mathf.Max(d.lst, 0.01f)
                + gacp.pin + d.pin;

            if (un > gacp.maxun) un = gacp.maxun;

            float m = (gacp.maxvel + gacp.maxspeed) * un * gacp.netslack;

            if (m > gacp.maxslack) m = gacp.maxslack;

            d.slack = m;
            d.unc = un;
            d.sspd = m / Mathf.Max(d.lst, gacp.minid);
        }

        public static void ping(gacd d, float rtt)
        {
            if (rtt < 0f) return;
            if (rtt > gacp.pingmax) rtt = gacp.pingmax;

            d.pin = rtt * 0.5f;

            bud(d);
        }
    }
}