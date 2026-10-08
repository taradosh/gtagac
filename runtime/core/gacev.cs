using UnityEngine;

namespace gtagac
{
    public struct gace
    {
        public string id;
        public string chk;
        public string rsn;
        public float cf;
        public float fl;
        public float cfl;
        public float tm;

        public gace(string pid, string check, string reason, float conf, float flags, float total, float time)
        {
            id = pid;
            chk = check;
            rsn = reason;
            cf = conf;
            fl = flags;
            cfl = total;
            tm = time;
        }
    }

    public static class gacs
    {
        public static System.Action<gace> onflag;
        public static System.Action<gace> onwarning;
        public static System.Action<gace> onkick;
        public static System.Action<gace> onban;

        public static void flag(gace e)
        {
            var h = onflag;
            if (h != null) h(e);
        }

        public static void warning(gace e)
        {
            var h = onwarning;
            if (h != null) h(e);
        }

        public static void kick(gace e)
        {
            var h = onkick;
            if (h != null) h(e);
        }

        public static void ban(gace e)
        {
            var h = onban;
            if (h != null) h(e);
        }
    }
}