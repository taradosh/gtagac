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
            if (mp.ContainsKey(i)) return;
            gacd d = new gacd(i, mp.Count);
            d.srv = true;
            d.reset();
            mp.Add(i, d);
            gac.upd(i, Vector3.zero, Vector3.zero, true);
        }

        public void rem(string i)
        {
            if (!mp.Remove(i)) return;
            gac.leave(i);
        }

        public void clr()
        {
            mp.Clear();
        }

        public void push(string i, Vector3 p, Vector3 v, bool g)
        {
            gac.upd(i, p, v, g);
        }

        public void push(string i, Vector3 p, Vector3 v, bool g, float t)
        {
            gac.upd(i, p, v, g);
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

        public static void up(string id, Vector3 p, Vector3 v, bool g, float t)
        {
            net.push(id, p, v, g, t);
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