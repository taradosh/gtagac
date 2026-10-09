using System.Collections.Generic;
using UnityEngine;

namespace gtagac
{
    public sealed class gacnet : igacnet
    {
        readonly Dictionary<string, gacd> mp = new Dictionary<string, gacd>(gacp.slots);

        public string id
        {
            get { return "srv"; }
        }

        public bool srv
        {
            get { return true; }
        }

        public Vector3 pos
        {
            get { return Vector3.zero; }
        }

        public Vector3 vel
        {
            get { return Vector3.zero; }
        }

        public float t
        {
            get { return gactime.now; }
        }

        public bool grnd
        {
            get { return true; }
        }

        public void kick(string r) { }

        public void ban(string r, float m) { }

        public void add(string i)
        {
            join(i);
        }

        public void rem(string i)
        {
            gacd d = gac.find(i);

            if (d != null && (d.bnd || d.kkd))
            {
                mp.Remove(i);
                gac.leave(i);
                return;
            }

            if (!mp.Remove(i)) return;

            gac.leave(i);
        }

        public void clr()
        {
            mp.Clear();
        }

        public int sq;

        public void push(string i, Vector3 p, Vector3 v, bool g)
        {
            gac.upd(i, p, v, g);
        }

        public bool push(string i, Vector3 p, Vector3 v, bool g, double t)
        {
            gacd d = slot(i);

            if (d == null) return false;

            sq++;

            return gac.upd2(d, p, v, g, sq, t);
        }

        public bool push(string i, Vector3 p, Vector3 v, bool g, double t, int seq)
        {
            gacd d = slot(i);

            if (d == null) return false;

            return gac.upd2(d, p, v, g, seq, t);
        }

        public void rtt(string i, float rtt)
        {
            gac.ping(i, rtt);
        }

        gacd slot(string i)
        {
            gacd d;

            if (mp.TryGetValue(i, out d))
            {
                gacd l = gac.find(i);

                if (l != null && (l.bnd || l.kkd)) return null;

                return d;
            }

            if (!join(i)) return null;

            return mp[i];
        }

        bool join(string i)
        {
            if (mp.ContainsKey(i)) return true;

            if (gac.store != null && gac.store.has(i)) return false;

            if (mp.Count >= gacp.slots) return false;

            gacd d = new gacd(i, mp.Count);
            d.srv = true;
            d.reset();
            mp.Add(i, d);
            gac.ply.Add(d);

            return true;
        }

        public int cnt
        {
            get { return mp.Count; }
        }
    }

    public static class gacsrv
    {
        public static gacnet net = new gacnet();
        public static gacban ban;

        public static void init()
        {
            gac.init(net);
            ban = new gacban();
            ban.load();
            gac.store = ban;
        }

        public static bool up(string id, Vector3 p, Vector3 v, bool g, double t)
        {
            return net.push(id, p, v, g, t);
        }

        public static bool up(string id, Vector3 p, Vector3 v, bool g, double t, int seq)
        {
            return net.push(id, p, v, g, t, seq);
        }

        /// <summary>
        /// the server is not the client's time source, a client timestamp is only meaningful
        /// when the client proves it can send sequence numbers, so enabling strict rejection is
        /// an explicit opt in and belongs here.
        /// </summary>
        public static void seq(bool on)
        {
            gacp.strict = on;
        }

        public static void rtt(string id, float rtt)
        {
            net.rtt(id, rtt);
        }

        public static void join(string id)
        {
            net.add(id);
        }

        public static void leave(string id)
        {
            net.rem(id);
        }

        public static void warn(string id, string r)
        {
            gacs.warning(new gace(id, "manual", r, 1f, 1, gacr.flgof(id), gactime.now));
        }

        public static void kick(string id, string r)
        {
            gac.kick(id, r);
        }

        public static void banp(string id, string r, float m)
        {
            gac.ban(id, r, m);
        }
    }
}