using UnityEngine;

namespace gtagac
{
    public interface igacnet
    {
        string id { get; }
        bool srv { get; }
        Vector3 pos { get; }
        Vector3 vel { get; }
        float t { get; }
        bool grnd { get; }
        void kick(string r);
        void ban(string r, float m);
    }

    public interface igacbanstore
    {
        bool has(string id);
        void add(string id, string r, float m);
        void rem(string id);
        int count { get; }
        void save();
        void load();
    }
}