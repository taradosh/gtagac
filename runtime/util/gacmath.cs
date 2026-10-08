using UnityEngine;

namespace gtagac
{
    public static class gacmath
    {
        public const float eps = 1e-5f;

        public static float len(Vector3 v)
        {
            return Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        public static float flen(Vector3 v)
        {
            return Mathf.Sqrt(v.x * v.x + v.z * v.z);
        }

        public static Vector3 fl(Vector3 v)
        {
            v.y = 0f;
            return v;
        }

        public static float cl01(float v)
        {
            if (v < 0f) return 0f;
            if (v > 1f) return 1f;
            return v;
        }

        public static float cl(float v, float a, float b)
        {
            if (v < a) return a;
            if (v > b) return b;
            return v;
        }

        public static float over(float v, float lim)
        {
            if (lim <= eps) return 0f;
            return (v - lim) / lim;
        }

        public static float damp(float cur, float tgt, float k, float dt)
        {
            float t = 1f - Mathf.Exp(-k * dt);
            return cur + (tgt - cur) * t;
        }

        public static float hold(float cur, bool up, float dt, float rate)
        {
            return up ? cur + dt : Mathf.Max(0f, cur - dt * rate);
        }

        public static float peak(float cur, float v, float k)
        {
            return cur * k > v ? cur * k : v;
        }

        public static float maxs(float a, float b)
        {
            return a > b ? a : b;
        }

        public static float abs(float a)
        {
            return a < 0f ? -a : a;
        }
    }
}