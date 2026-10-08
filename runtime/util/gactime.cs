using UnityEngine;

namespace gtagac
{
    public static class gactime
    {
        public const float lim = 0.25f;
        public const float zlim = 0.0001f;

        public static float now;
        public static float dt;
        public static float ndt;
        public static float sdt;
        public static float fdt;
        public static float sc;
        public static float bsc;
        public static float spk;
        public static float zro;
        public static bool bad;

        public static void init()
        {
            now = 0f;
            dt = 0f;
            ndt = 0f;
            sdt = 0f;
            fdt = 0f;
            spk = 0f;
            zro = 0f;
            bad = false;
            sc = Time.timeScale;
            bsc = sc;
        }

        public static void step()
        {
            ndt = dt;
            dt = read();
            now += dt;
            sdt = Time.deltaTime;
            fdt = Time.fixedUnscaledDeltaTime;
            float t = Time.timeScale;
            if (t != sc)
            {
                sc = t;
                bad = true;
            }
            if (dt >= lim) spk += dt;
            if (dt <= zlim) zro += 0.05f;
        }

        static float read()
        {
            float d = Time.unscaledDeltaTime;
            if (d < 0f) d = 0f;
            if (d > lim) d = lim;
            return d;
        }

        public static float r()
        {
            return dt > zlim ? 1f / dt : 0f;
        }

        public static float anom()
        {
            float v = 0f;
            if (bad) v += 0.5f;
            if (spk > 0.5f) v += 0.25f;
            if (zro > 1f) v += 0.25f;
            return gacmath.cl01(v);
        }

        public static void dec()
        {
            if (spk > 0f) spk -= dt;
            if (zro > 0f) zro -= dt;
            if (bad) bad = false;
        }
    }
}