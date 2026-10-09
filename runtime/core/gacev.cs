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

    public interface igace
    {
        void onflag(gace e);
        void onwarning(gace e);
        void onkick(gace e);
        void onban(gace e);
    }

    public sealed class gacsub : igace
    {
        public System.Action<gace> flag;
        public System.Action<gace> warning;
        public System.Action<gace> kick;
        public System.Action<gace> ban;

        void igace.onflag(gace e)
        {
            var h = flag;
            if (h != null) h(e);
        }

        void igace.onwarning(gace e)
        {
            var h = warning;
            if (h != null) h(e);
        }

        void igace.onkick(gace e)
        {
            var h = kick;
            if (h != null) h(e);
        }

        void igace.onban(gace e)
        {
            var h = ban;
            if (h != null) h(e);
        }
    }

    public static class gacs
    {
        public static System.Action<gace> onflag;
        public static System.Action<gace> onwarning;
        public static System.Action<gace> onkick;
        public static System.Action<gace> onban;

        static igace[] subs;
        static int nsub;

        public static void bind(igace s)
        {
            if (s == null) return;

            if (subs == null) subs = new igace[4];

            for (int i = 0; i < nsub; i++) if (ReferenceEquals(subs[i], s)) return;

            if (nsub < subs.Length) subs[nsub++] = s;
        }

        public static void unbind(igace s)
        {
            for (int i = 0; i < nsub; i++)
            {
                if (!ReferenceEquals(subs[i], s)) continue;

                subs[i] = subs[nsub - 1];
                nsub--;
                subs[nsub] = null;
                break;
            }
        }

        public static int bnd
        {
            get { return nsub; }
        }

        static void fan(int k, gace e)
        {
            for (int i = 0; i < nsub; i++)
            {
                igace s = subs[i];

                if (k == 0) s.onflag(e);
                else if (k == 1) s.onwarning(e);
                else if (k == 2) s.onkick(e);
                else s.onban(e);
            }
        }

        public static void flag(gace e)
        {
            var h = onflag;
            if (h != null) h(e);

            fan(0, e);
        }

        public static void warning(gace e)
        {
            var h = onwarning;
            if (h != null) h(e);

            fan(1, e);
        }

        public static void kick(gace e)
        {
            var h = onkick;
            if (h != null) h(e);

            fan(2, e);
        }

        public static void ban(gace e)
        {
            var h = onban;
            if (h != null) h(e);

            fan(3, e);
        }
    }
}