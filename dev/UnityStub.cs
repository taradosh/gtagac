using System;

namespace UnityEngine
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector3 zero { get { return new Vector3(0f, 0f, 0f); } }
        public static Vector3 one { get { return new Vector3(1f, 1f, 1f); } }
        public static Vector3 up { get { return new Vector3(0f, 1f, 0f); } }
        public static Vector3 down { get { return new Vector3(0f, -1f, 0f); } }

        public static float Dot(Vector3 a, Vector3 b) { return a.x * b.x + a.y * b.y + a.z * b.z; }

        public static Vector3 Cross(Vector3 a, Vector3 b)
        {
            return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }

        public float magnitude
        {
            get { return Mathf.Sqrt(x * x + y * y + z * z); }
        }

        public Vector3 normalized
        {
            get
            {
                float m = magnitude;
                return m > 1e-6f ? new Vector3(x / m, y / m, z / m) : zero;
            }
        }

        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z); }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z); }
        public static Vector3 operator -(Vector3 a) { return new Vector3(-a.x, -a.y, -a.z); }
        public static Vector3 operator *(Vector3 a, float b) { return new Vector3(a.x * b, a.y * b, a.z * b); }
        public static Vector3 operator *(float b, Vector3 a) { return a * b; }
        public static Vector3 operator /(Vector3 a, float b) { return new Vector3(a.x / b, a.y / b, a.z / b); }
        public static bool operator ==(Vector3 a, Vector3 b) { return a.x == b.x && a.y == b.y && a.z == b.z; }
        public static bool operator !=(Vector3 a, Vector3 b) { return !(a == b); }

        public override bool Equals(object? o) { return o is Vector3 && this == (Vector3)o; }
        public override int GetHashCode() { return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode(); }
        public override string ToString() { return "(" + x + ", " + y + ", " + z + ")"; }
    }

    public static class Mathf
    {
        public const float Epsilon = 1.401298E-45f;
        public const float Infinity = float.PositiveInfinity;
        public const float PI = 3.14159274f;

        public static float Sqrt(float v) { return (float)Math.Sqrt(v); }
        public static float Abs(float a) { return Math.Abs(a); }
        public static float Min(float a, float b) { return a < b ? a : b; }
        public static int Min(int a, int b) { return a < b ? a : b; }
        public static float Max(float a, float b) { return a > b ? a : b; }
        public static int Max(int a, int b) { return a > b ? a : b; }
        public static int CeilToInt(float v) { return (int)Math.Ceiling(v); }
        public static int FloorToInt(float v) { return (int)Math.Floor(v); }
        public static int RoundToInt(float v) { return (int)Math.Round(v); }
        public static float Sin(float v) { return (float)Math.Sin(v); }
        public static float Cos(float v) { return (float)Math.Cos(v); }
        public static float Clamp(float v, float a, float b) { return v < a ? a : (v > b ? b : v); }
        public static int Clamp(int v, int a, int b) { return v < a ? a : (v > b ? b : v); }
        public static float Clamp01(float v) { return v < 0f ? 0f : (v > 1f ? 1f : v); }
        public static float Exp(float v) { return (float)Math.Exp(v); }
        public static float Floor(float v) { return (float)Math.Floor(v); }
        public static float Ceil(float v) { return (float)Math.Ceiling(v); }
        public static float Pow(float a, float b) { return (float)Math.Pow(a, b); }
        public static float Log(float v) { return (float)Math.Log(v); }
        public static float Sqrt2(float v) { return (float)Math.Sqrt(v); }
        public static float Lerp(float a, float b, float t) { return a + (b - a) * Clamp01(t); }
    }

    public class Object
    {
        public string name;
        public HideFlags hideFlags;

        public static void Destroy(Object o) { }
        public static void DontDestroyOnLoad(Object o) { }
    }

    public enum HideFlags
    {
        None = 0,
        HideAndDontSave = 61
    }

    public class Component : Object
    {
        public Transform transform { get { return null; } }
    }

    public class Behaviour : Component
    {
        public bool enabled;
    }

    public class MonoBehaviour : Behaviour
    {
    }

    public class Transform : Component
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Vector3 localScale { get; set; }
        public Vector3 forward { get; set; }
        public Vector3 up { get { return Vector3.up; } }
        public Vector3 right { get { return new Vector3(1f, 0f, 0f); } }
    }

    public class Rigidbody : Component
    {
        public Vector3 velocity { get; set; }
        public Vector3 linearVelocity { get; set; }
        public Vector3 worldCenterOfMass { get { return Vector3.zero; } }
        public float mass { get; set; }
        public bool isKinematic { get; set; }
        public bool useGravity { get; set; }
    }

    public class Collider : Component
    {
        public Bounds bounds { get; set; }
    }

    public struct Bounds
    {
        public Vector3 center;
        public Vector3 extents;
        public Vector3 min { get { return center - extents; } }
        public Vector3 max { get { return center + extents; } }
    }

    public struct RaycastHit
    {
        public Vector3 point;
        public Vector3 normal;
        public float distance;
        public Collider collider;
    }

    public enum QueryTriggerInteraction
    {
        UseGlobal = 0,
        Ignore = 1,
        Collide = 2
    }

    public delegate bool RaycastHitFn(Vector3 o, Vector3 d, float m, int mask, out RaycastHit h);

    public static class Physics
    {
        public static Func<Vector3, Vector3, float, int, bool>? RaycastHook;
        public static RaycastHitFn? RaycastHitHook;
        public static Func<Vector3, float, int, bool>? SphereHook;

        public static bool CheckSphere(Vector3 p, float r, int mask)
        {
            return SphereHook != null && SphereHook(p, r, mask);
        }

        public static bool CheckSphere(Vector3 p, float r, int mask, QueryTriggerInteraction q)
        {
            return CheckSphere(p, r, mask);
        }

        public static bool Raycast(Vector3 o, Vector3 d, out RaycastHit hit, float m, int mask, QueryTriggerInteraction q)
        {
            if (RaycastHitHook != null) return RaycastHitHook(o, d, m, mask, out hit);
            hit = default;
            return false;
        }

        public static bool Raycast(Vector3 o, Vector3 d, out RaycastHit hit, float m, int mask)
        {
            return Raycast(o, d, out hit, m, mask, QueryTriggerInteraction.UseGlobal);
        }

        public static bool Raycast(Vector3 o, Vector3 d, float m, int mask, QueryTriggerInteraction q)
        {
            return RaycastHook != null && RaycastHook(o, d, m, mask);
        }

        public static bool Raycast(Vector3 o, Vector3 d, float m)
        {
            return Raycast(o, d, m, ~0, QueryTriggerInteraction.UseGlobal);
        }

        public static bool ComputePenetration(
            Collider a, Vector3 pa, Quaternion ra,
            Collider b, Vector3 pb, Quaternion rb,
            out Vector3 dir, out float dist)
        {
            dir = Vector3.zero;
            dist = 0f;
            return false;
        }
    }

    public struct Quaternion
    {
        public static Quaternion identity { get { return new Quaternion(); } }
    }

    public enum KeyCode
    {
        None = 0,
        F1 = 282,
        F2 = 283
    }

    public static class Time
    {
        public static float scale = 1f;
        public static float deltaTime = 0.016f;
        public static float unscaledDeltaTime = 0.016f;
        public static float fixedDeltaTime = 0.02f;
        public static float fixedUnscaledDeltaTime = 0.02f;
        public static float time = 0f;
        public static float unscaledTime = 0f;
        public static int frameCount = 0;

        public static float timeScale
        {
            get { return scale; }
            set { scale = value; }
        }
    }

    public static class Debug
    {
        public static bool isDebugBuild = true;
        public static List<string> Sink = new List<string>();

        public static void Log(object m) { Sink.Add("log " + m); }
        public static void LogWarning(object m) { Sink.Add("warn " + m); }
        public static void LogError(object m) { Sink.Add("err " + m); }
    }

    public static class Application
    {
        public static string persistentDataPath = ".";
        public static bool isPlaying = true;
    }
}