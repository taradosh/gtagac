using System.Collections.Generic;
using UnityEngine;

namespace gtagac
{
    public static class gac
    {
        public static bool run;
        public static bool srv;
        public static bool loc;

        public static readonly List<gacd> ply = new List<gacd>(gacp.slots);
        public static gacd me;
        public static igacnet net;

        static float acc;
        static float accf;

        public static int ver = 1;

        public static void init()
        {
            init(null, null, null, null);
        }

        public static void init(Transform tr)
        {
            init(tr, null, null, null);
        }

        public static void init(Transform tr, Transform hl, Transform hr, Rigidbody bd)
        {
            if (run) return;

            gactime.init();
            if (store == null) store = new gacbm();

            loc = tr != null;

            me = new gacd("me", 0);
            me.tr = tr;
            me.hl = hl;
            me.hr = hr;
            me.bd = bd;
            me.loc = true;
            me.reset();
            ply.Add(me);

            reg(new gacspd());
            reg(new gactp());
            reg(new gacjmp());
            reg(new gacfly());
            reg(new gacgrav());
            reg(new gacvel());
            reg(new gacarm());
            reg(new gacpos());
            reg(new gacrat());
            reg(new gacenv());

            run = true;
            gaclog.i("init v" + ver + " loc=" + loc);
        }

        public static void init(igacnet n)
        {
            init(null, null, null, null);

            net = n;

            if (n == null) return;

            me.id = n.id;
            me.net = n;
            me.loc = !n.srv;
            srv = n.srv;

            gaclog.i("net " + n.id + " srv=" + srv);
        }

        public static igacbanstore store;

        public static void reg(gacc c)
        {
            if (c == null) return;
            c.on = gacp.on(c.name);
            gacr.wgt.set(c.name, c.weight);
            gacr.reg(c);
        }

        public static void unreg(string n)
        {
            gacr.unreg(n);
        }

        public static void sync()
        {
            for (int i = 0; i < gacr.cnum; i++)
            {
                gaccheck c = gacr.cat(i);
                c.on = gacp.on(c.name);
            }
            gaclog.i("sync");
        }

        public static void shut()
        {
            run = false;
            ply.Clear();
            gacr.clr();
            me = null;
            net = null;
            acc = 0f;
            accf = 0f;
            gaclog.i("shut");
        }

        public static void tick()
        {
            if (!run) return;

            gactime.step();
            gactime.dec();

            if (me == null) return;
            if (srv) return;
            if (me.tr == null && me.net == null) return;

            acc += gactime.dt;

            if (acc < gacp.ivl) return;

            acc = 0f;

            if (skip(me)) return;

            smp(me);
            evl(me);
        }

        public static void evl(gacd d)
        {
            float dt = gactime.dt;

            if (dt <= 0f) return;

            d.tck(dt);
            d.fall(dt);
            d.gy = d.ingnd() ? 0f : d.gy - gacp.grav * dt;

            gacr.tick(d, dt);

            if (d.grc > 0f) return;

            for (int i = 0; i < gacr.cnum; i++)
            {
                gaccheck c = gacr.cat(i);
                if (!c.en) continue;
                c.tick(d, dt);
            }
        }

        public static void smp(gacd d)
        {
            Vector3 p;
            Vector3 v;

            if (d.net != null)
            {
                p = d.net.pos;
                v = d.net.vel;
                d.grnd = d.net.grnd;
            }
            else if (d.tr != null)
            {
                p = d.tr.position;
                v = d.bd != null ? gacd.bvel(d.bd) : (d.h.n > 0 ? (p - d.h.now) / gactime.dt : Vector3.zero);
            }
            else
            {
                return;
            }

            float t = gactime.now;
            d.push(p, v, t);
            d.upd += 1f;

            if (d.h.n < 2) return;

            d.spd = gacmath.damp(d.spd, d.h.spd(0), 10f, gactime.dt);
            d.fspd = gacmath.damp(d.fspd, d.h.fspd(0), 10f, gactime.dt);
            d.vspd = gacmath.damp(d.vspd, Mathf.Abs(v.y), 10f, gactime.dt);
            d.acc = gacmath.damp(d.acc, gacmath.len(v - d.h.vat(1)), 10f, gactime.dt);
        }

        public static void phys()
        {
            if (!run) return;
            accf += Time.fixedDeltaTime;
            if (accf < gacp.ivl) return;
            accf = 0f;
            if (me == null) return;
            if (me.tr != null && gacp.grdchk) grd(me);
        }

        static void grd(gacd d)
        {
            d.grnd = Physics.CheckSphere(d.tr.position + Vector3.up * gacp.grdoff, 0.05f, gacp.grdmask, QueryTriggerInteraction.Ignore);
        }

        public static bool skip(gacd d)
        {
            if (d == null) return true;
            if (d.kkd || d.bnd) return true;
            if (d.adm || d.tru) return true;
            if (gacp.wlst && gacp.adm(d.id)) return true;
            if (gacp.devby && Debug.isDebugBuild && !gacp.dbgf) return true;
            if (!gacp.loc && d.loc) return true;
            return false;
        }

        public static void upd(string id, Vector3 p, Vector3 v, bool g)
        {
            gacd d = find(id);

            if (d == null)
            {
                if (ply.Count >= gacp.slots) return;
                d = new gacd(id, ply.Count);
                d.srv = true;
                d.reset();
                ply.Add(d);
            }

            if (d.net == null) d.net = net;
            if (d.ind >= gacp.slots) return;

            if (!d.chkd)
            {
                d.chkd = true;
                if (store != null && store.has(id))
                {
                    d.bnd = true;
                    gacs.ban(new gace(id, "store", "banned", 1f, 1, 0, gactime.now));
                    return;
                }
            }

            d.grnd = g;
            d.push(p, v, gactime.now);
            d.upd += 1f;

            if (skip(d)) return;

            evl(d);
        }

        public static bool st(string id)
        {
            gacd d = find(id);
            if (d == null) return true;
            return !d.kkd && !d.bnd;
        }

        public static Vector3 pos(string id)
        {
            gacd d = find(id);
            return d == null ? Vector3.zero : d.p;
        }

        public static Vector3 vel(string id)
        {
            gacd d = find(id);
            return d == null ? Vector3.zero : d.v;
        }

        public static gacd find(string id)
        {
            for (int i = 0; i < ply.Count; i++)
            {
                if (ply[i].id == id) return ply[i];
            }
            return null;
        }

        public static gacd join(string id)
        {
            return find(id);
        }

        public static void leave(string id)
        {
            gacd d = find(id);
            if (d == null) return;
            d.lft = true;
            ply.Remove(d);
            gacr.drop(id);
        }

        public static void adm(string id, bool a)
        {
            gacd d = find(id);
            if (d != null) d.adm = a;
        }

        public static void tru(string id, bool a)
        {
            gacd d = find(id);
            if (d != null) d.tru = a;
        }

        public static void grace(string id, float t)
        {
            gacd d = find(id);
            if (d != null) d.grace(t);
        }

        public static void st(bool g, bool j, bool c, bool s, bool b)
        {
            if (me == null) return;
            me.st(g, j, c, s, b);
        }

        public static void hl(Transform t)
        {
            if (me != null) me.hl = t;
        }

        public static void hr(Transform t)
        {
            if (me != null) me.hr = t;
        }

        public static void bd(Rigidbody b)
        {
            if (me != null) me.bd = b;
        }

        public static void kick(string id, string r)
        {
            gacd d = find(id);

            gacs.kick(new gace(id, "kick", r, 1f, 1, gacr.flgof(id), gactime.now));

            if (d == null) return;

            d.kkd = true;
            if (d.net != null) d.net.kick(r);
        }

        public static void ban(string id, string r, float m)
        {
            gacd d = find(id);

            gacs.ban(new gace(id, "ban", r, 1f, 1, gacr.flgof(id), gactime.now));

            if (store != null) store.add(id, r, m);

            if (d == null) return;

            d.bnd = true;
            if (d.net != null) d.net.ban(r, m);
        }

        public static gaccheck chk(int i)
        {
            return gacr.cat(i);
        }

        public static int cnum
        {
            get { return gacr.cnum; }
        }

        public static void reset()
        {
            gacr.clr();
            for (int i = 0; i < ply.Count; i++) ply[i].reset();
            acc = 0f;
            accf = 0f;
        }
    }
}