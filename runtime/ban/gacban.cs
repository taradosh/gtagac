using System;
using System.Collections.Generic;
using UnityEngine;

namespace gtagac
{
    public sealed class gacban : igacbanstore
    {
        readonly Dictionary<string, string> rs = new Dictionary<string, string>(64);
        readonly Dictionary<string, float> tm = new Dictionary<string, float>(64);
        readonly List<string> lk = new List<string>(64);
        readonly HashSet<string> perm = new HashSet<string>();

        string path;
        bool keep = true;

        public int count
        {
            get { return rs.Count; }
        }

        public gacban()
        {
            path = Application.persistentDataPath + "/gtagac_bans.txt";
        }

        public gacban(string p)
        {
            path = p;
        }

        public void setpath(string p)
        {
            path = p;
        }

        public void clr()
        {
            rs.Clear();
            tm.Clear();
            lk.Clear();
            perm.Clear();
        }

        public bool has(string i)
        {
            if (perm.Contains(i)) return true;
            float t;
            if (!tm.TryGetValue(i, out t)) return rs.ContainsKey(i);
            if (t <= 0f) return rs.ContainsKey(i);
            if (DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond > t) return false;
            return rs.ContainsKey(i);
        }

        public void add(string i, string r, float m)
        {
            if (i == null) return;
            if (!rs.ContainsKey(i))
            {
                lk.Add(i);
            }
            rs[i] = r;
            if (m <= 0f)
            {
                tm[i] = 0f;
                perm.Add(i);
            }
            else
            {
                tm[i] = DateTime.UtcNow.Ticks / TimeSpan.TicksPerSecond + m;
                perm.Remove(i);
            }
            if (keep) save();
        }

        public void rem(string i)
        {
            rs.Remove(i);
            tm.Remove(i);
            perm.Remove(i);
            for (int j = 0; j < lk.Count; j++)
            {
                if (lk[j] == i)
                {
                    lk.RemoveAt(j);
                    break;
                }
            }
            if (keep) save();
        }

        public void load()
        {
            clr();
            if (path == null) return;
            if (!System.IO.File.Exists(path)) return;

            string[] a = System.IO.File.ReadAllLines(path);

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].Length < 2) continue;
                string[] s = a[i].Split('|');
                if (s.Length < 3) continue;
                float m = 0f;
                float.TryParse(s[1], out m);
                add(s[0], s[2], m);
            }
        }

        public void save()
        {
            if (path == null) return;
            keep = false;
            string[] a = new string[rs.Count];
            int n = 0;
            for (int i = 0; i < lk.Count; i++)
            {
                string i2 = lk[i];
                if (!rs.ContainsKey(i2)) continue;
                float m = 0f;
                if (!perm.Contains(i2)) tm.TryGetValue(i2, out m);
                a[n] = i2 + "|" + m.ToString("0") + "|" + rs[i2];
                n++;
            }
            if (n != a.Length)
            {
                string[] b = new string[n];
                System.Array.Copy(a, b, n);
                a = b;
            }
            System.IO.File.WriteAllLines(path, a);
            keep = true;
        }
    }
}