using UnityEngine;

namespace gtagac
{
    public static class gaclog
    {
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
            w(d.id + " " + c + " c" + cf.ToString("0.00") + " f" + fl);
        }
    }
}