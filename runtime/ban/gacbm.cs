namespace gtagac
{
    public sealed class gacbm : igacbanstore
    {
        readonly string[] id = new string[256];
        readonly string[] rs = new string[256];
        readonly float[] mn = new float[256];
        int n;

        public int count
        {
            get { return n; }
        }

        public bool has(string i)
        {
            int x = fnd(i);
            return x >= 0;
        }

        public void add(string i, string r, float m)
        {
            int x = fnd(i);
            if (x >= 0)
            {
                rs[x] = r;
                mn[x] = m;
                return;
            }
            if (n >= id.Length) return;
            id[n] = i;
            rs[n] = r;
            mn[n] = m;
            n++;
        }

        public void rem(string i)
        {
            int x = fnd(i);
            if (x < 0) return;
            for (int j = x; j < n - 1; j++)
            {
                id[j] = id[j + 1];
                rs[j] = rs[j + 1];
                mn[j] = mn[j + 1];
            }
            n--;
            id[n] = null;
            rs[n] = null;
            mn[n] = 0f;
        }

        int fnd(string i)
        {
            for (int j = 0; j < n; j++)
            {
                if (id[j] == i) return j;
            }
            return -1;
        }

        public void save() { }

        public void load() { }

        public string get(string i)
        {
            int x = fnd(i);
            return x < 0 ? null : rs[x];
        }

        public float len(string i)
        {
            int x = fnd(i);
            return x < 0 ? 0f : mn[x];
        }

        public void clr()
        {
            for (int j = 0; j < n; j++)
            {
                id[j] = null;
                rs[j] = null;
                mn[j] = 0f;
            }
            n = 0;
        }
    }
}