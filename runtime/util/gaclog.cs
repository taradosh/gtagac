using System.Globalization;
using System.Text;
using UnityEngine;

namespace gtagac
{
    public static class gacn
    {
        public static string f(float v, string m)
        {
            return v.ToString(m, CultureInfo.InvariantCulture);
        }

        public static string f(double v, string m)
        {
            return v.ToString(m, CultureInfo.InvariantCulture);
        }
    }

    public struct gacl
    {
        public float t;
        public string id;
        public string lvl;
        public string chk;
        public string rsn;
        public float cf;
        public int tot;
        public int seq;
        public int drp;
    }

    public sealed class gaclogsub : igace
    {
        void igace.onflag(gace e)
        {
            gaclog.k(gaclog.lflag, e.id, e.chk, e.rsn, e.cf, (int)e.cfl);
        }

        void igace.onwarning(gace e)
        {
            gaclog.k(gaclog.lwarn, e.id, e.chk, e.rsn, e.cf, (int)e.cfl);
        }

        void igace.onkick(gace e)
        {
            gaclog.k(gaclog.lkick, e.id, e.chk, e.rsn, e.cf, (int)e.cfl);
        }

        void igace.onban(gace e)
        {
            gaclog.k(gaclog.lban, e.id, e.chk, e.rsn, e.cf, (int)e.cfl);
        }
    }

    public static class gaclog
    {
        public const string lflag = "flag";
        public const string lwarn = "warn";
        public const string lkick = "kick";
        public const string lban = "ban";
        public const string lnet = "net";
        public const string lcfg = "cfg";
        public const string lgate = "gate";

        static gacl[] buf;
        static int cur;
        static int cnt;
        static int drop;

        public static int kept
        {
            get { return cnt < buflen() ? cnt : buflen(); }
        }

        public static int lost
        {
            get { return drop; }
        }

        static int buflen()
        {
            return buf == null ? 0 : buf.Length;
        }

        static void ring()
        {
            int n = Mathf.Clamp(gacp.logkeep, 8, 65536);

            if (buf == null)
            {
                buf = new gacl[n];
                return;
            }

            if (buf.Length == n) return;

            var b = new gacl[n];

            int keep = kept;

            for (int i = 0; i < keep; i++) b[(cur + i) % n] = buf[i];

            cnt = keep;
            cur = 0;
            buf = b;
        }

        static bool lim(string id)
        {
            if (id == null) return false;

            gacper p = gacr.get(id);
            float t = gactime.now;

            if (t - p.lgw >= gacp.logwin)
            {
                p.lgw = t;
                p.lgn = 0;
            }

            if (p.lgn >= gacp.loglim) return true;

            p.lgn++;

            return false;
        }

        public static void k(string lvl, string id, string chk, string rsn, float cf, int tot)
        {
            if (!gacp.logon) return;

            bool rate = lvl == lflag || lvl == lnet;

            if (rate && lim(id)) return;

            ring();

            int n = buflen();

            if (cnt >= n) drop++;

            buf[cur].t = gactime.now;
            buf[cur].id = id;
            buf[cur].lvl = lvl;
            buf[cur].chk = chk;
            buf[cur].rsn = rsn;
            buf[cur].cf = cf;
            buf[cur].tot = tot;
            buf[cur].seq = 0;
            buf[cur].drp = 0;

            cur = (cur + 1) % n;
            if (cnt < n) cnt++;
        }

        public static void n(string id, string rsn)
        {
            k(lnet, id, lnet, rsn, 0f, 0);
        }

        public static void c(string rsn)
        {
            k(lcfg, null, lcfg, rsn, 0f, 0);
        }

        public static gacl at(int i)
        {
            ring();

            int n = buflen();

            if (i < 0 || i >= kept) return default;

            return buf[(cur - 1 - i + n * 2) % n];
        }

        public static void clr()
        {
            cur = 0;
            cnt = 0;
            drop = 0;
        }

        public static string txt()
        {
            var sb = new StringBuilder();

            for (int i = 0; i < kept; i++)
            {
                gacl e = at(i);

                sb.Append(gacn.f(e.t, "0.00")).Append(' ');
                sb.Append(e.lvl).Append(' ');
                sb.Append(e.id ?? "-").Append(' ');
                sb.Append(e.chk ?? "-").Append(' ');
                sb.Append(e.rsn ?? "-");
                sb.Append(" cf=").Append(gacn.f(e.cf, "0.00"));
                sb.Append(" f=").Append(e.tot);

                sb.Append('\n');
            }

            return sb.ToString();
        }

        public static string csv()
        {
            var sb = new StringBuilder();

            sb.Append("t,level,id,check,confidence,flags,reason\n");

            for (int i = kept - 1; i >= 0; i--)
            {
                gacl e = at(i);

                sb.Append(gacn.f(e.t, "0.00")).Append(',');
                sb.Append(e.lvl).Append(',');
                sb.Append(q(e.id)).Append(',');
                sb.Append(q(e.chk)).Append(',');
                sb.Append(gacn.f(e.cf, "0.000")).Append(',');
                sb.Append(e.tot).Append(',');
                sb.Append(q(e.rsn)).Append('\n');
            }

            return sb.ToString();
        }

        static string q(string s)
        {
            if (s == null) return "";

            var sb = new StringBuilder(s.Length + 2);

            sb.Append('"');

            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];

                if (c == '"') sb.Append('"');

                sb.Append(c);
            }

            sb.Append('"');

            return sb.ToString();
        }

        public static void i(string m)
        {
            if (!gacp.dbgo()) return;
            Debug.Log("[gtagac] " + m);
        }

        public static void w(string m)
        {
            if (!gacp.dbgo()) return;
            Debug.LogWarning("[gtagac] " + m);
        }

        public static void e(string m)
        {
            Debug.LogError("[gtagac] " + m);
        }

        public static void f(gacd d, string c, float cf, int fl)
        {
            if (!gacp.dbgo()) return;
            w(d.id + " " + c + " c" + gacn.f(cf, "0.00") + " f" + fl);
        }
    }
}