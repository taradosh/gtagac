using System.Collections.Generic;
using UnityEngine;

namespace gtagac
{
    public static class gacr
    {
        public static igacbanstore store;
        public static gacw wgt = new gacw();

        static readonly Dictionary<string, gacper> pp = new Dictionary<string, gacper>(gacp.slots);
        static readonly List<string> lk = new List<string>(gacp.slots);
        static readonly List<gaccheck> rk = new List<gaccheck>(16);

        public static void clr()
        {
            pp.Clear();
            lk.Clear();
            rk.Clear();
        }

        public static gacper get(string id)
        {
            gacper p;
            if (pp.TryGetValue(id, out p)) return p;

            if (lk.Count >= gacp.slots)
            {
                string old = lk[0];

                lk.RemoveAt(0);
                pp.Remove(old);
            }

            p = new gacper();
            pp.Add(id, p);
            lk.Add(id);
            return p;
        }

        public static void drop(string id)
        {
            if (!pp.Remove(id)) return;
            for (int i = 0; i < lk.Count; i++)
            {
                if (lk[i] == id)
                {
                    lk.RemoveAt(i);
                    break;
                }
            }
        }

        public static void reg(gaccheck c)
        {
            for (int i = 0; i < rk.Count; i++)
            {
                if (rk[i].name == c.name) return;
            }
            rk.Add(c);
        }

        public static void unreg(string n)
        {
            for (int i = rk.Count - 1; i >= 0; i--)
            {
                if (rk[i].name == n) rk.RemoveAt(i);
            }
        }

        public static int cnum
        {
            get { return rk.Count; }
        }

        public static gaccheck cat(int i)
        {
            return rk[i];
        }

        public static bool evok(string id)
        {
            if (!gacp.evon) return true;

            gacper p;

            if (!pp.TryGetValue(id, out p)) return false;

            if (p.f.conf() < gacp.evcf) return false;

            return p.f.ind(gactime.now, gacp.evwin) >= gacp.evmin;
        }

        public static int evind(string id)
        {
            gacper p;

            if (!pp.TryGetValue(id, out p)) return 0;

            return p.f.ind(gactime.now, gacp.evwin);
        }

        public static bool canflg(gacd d, gaccheck c, float cf)
        {
            if (d == null || c == null) return false;
            if (!c.en) return false;
            if (cf <= 0f) return false;

            gacper p;

            if (!pp.TryGetValue(d.id, out p))
            {
                get(d.id);
                return true;
            }

            if (p.sent) return false;

            return p.f.cdn(c.name, gactime.now);
        }

        public static void flg(gacd d, gaccheck c, float cf, string r)
        {
            if (!canflg(d, c, cf)) return;

            gacper p = get(d.id);

            float t = gactime.now;

            int n = 1;
            float w = c.wgt;
            p.f.add(c.name, n, w, cf, t, c.cdt);

            int tot = p.f.total();
            float cs = p.f.conf();

            gace e = new gace(d.id, c.name, r, cf, n, tot, t);

            gacs.flag(e);

            if (gacp.dbgo()) gaclog.f(d, c.name, cf, tot);

            if (tot >= gacp.fth && p.warn < gacp.maxwarn && gacp.pun && gacp.pwn)
            {
                p.warn++;
                gacs.warning(new gace(d.id, c.name, r, cs, p.warn, tot, t));

                if (gacp.dbgo()) gaclog.w(d.id + " warning " + p.warn);
            }

            if (gacp.pun && gacp.pkk && tot >= gacp.kth && p.kick == 0)
            {
                if (!evok(d.id))
                {
                    gaclog.k(gaclog.lgate, d.id, c.name, r, cs, tot);
                }
                else
                {
                    p.kick++;
                    gacs.kick(new gace(d.id, c.name, r, cs, p.kick, tot, t));

                    if (gacp.dbgo()) gaclog.w(d.id + " kick");

                    if (d.net != null) d.net.kick(r);
                    if (!gacp.srv) p.sent = true;
                }
            }

            if (gacp.pun && gacp.pkb && (tot >= gacp.bth || cs >= gacp.bcf) && p.ban == 0)
            {
                if (!evok(d.id))
                {
                    gaclog.k(gaclog.lgate, d.id, c.name, r, cs, tot);
                }
                else
                {
                    p.ban++;
                    float m = gacp.bper ? 0f : gacp.btmp;
                    if (m <= 0f && !gacp.pbm) m = 60f;
                    gacs.ban(new gace(d.id, c.name, r, cs, p.ban, tot, t));

                    if (gacp.dbgo()) gaclog.w(d.id + " ban " + m);
                    if (store != null) store.add(d.id, r, m);
                    if (d.net != null) d.net.ban(r, m);
                    if (!gacp.srv) p.sent = true;
                }
            }
        }

        public static void tick(gacd d, float dt)
        {
            if (d == null) return;
            gacper p = get(d.id);
            p.f.decay(gacp.decay, dt, gactime.now);
        }

        public static int flgof(string id)
        {
            gacper p;
            if (!pp.TryGetValue(id, out p)) return 0;
            return p.f.total();
        }

        public static float cfof(string id)
        {
            gacper p;
            if (!pp.TryGetValue(id, out p)) return 0f;
            return p.f.conf();
        }

        public static int warnof(string id)
        {
            gacper p;
            if (!pp.TryGetValue(id, out p)) return 0;
            return p.warn;
        }

        public static bool kkd(string id)
        {
            gacper p;
            if (!pp.TryGetValue(id, out p)) return false;
            return p.kick > 0;
        }

        public static bool bnd(string id)
        {
            gacper p;
            if (!pp.TryGetValue(id, out p)) return false;
            return p.ban > 0;
        }
    }

    public sealed class gacw
    {
        public float spd = 1f;
        public float tpd = 3f;
        public float jmp = 1f;
        public float fly = 1f;
        public float grv = 1f;
        public float vel = 2f;
        public float arm = 1f;
        public float pos = 2f;
        public float rat = 2f;
        public float env = 4f;
        public float con = 2f;
        public float nc = 3f;

        public float of(string n)
        {
            switch (n)
            {
                case "speed": return spd;
                case "teleport": return tpd;
                case "jump": return jmp;
                case "fly": return fly;
                case "gravity": return grv;
                case "velocity": return vel;
                case "arm": return arm;
                case "position": return pos;
                case "rate": return rat;
                case "env": return env;
                case "consistency": return con;
                case "noclip": return nc;
            }
            return 1f;
        }

        public void set(string n, float v)
        {
            switch (n)
            {
                case "speed": spd = v; break;
                case "teleport": tpd = v; break;
                case "jump": jmp = v; break;
                case "fly": fly = v; break;
                case "gravity": grv = v; break;
                case "velocity": vel = v; break;
                case "arm": arm = v; break;
                case "position": pos = v; break;
                case "rate": rat = v; break;
                case "env": env = v; break;
                case "consistency": con = v; break;
                case "noclip": nc = v; break;
            }
        }
    }
}