namespace gtagac
{
    public interface gaccheck
    {
        string name { get; }
        bool en { get; }
        bool on { get; set; }
        int flags { get; }
        float wgt { get; }
        float cdt { get; }
        void tick(gacd d, float dt);
        void flg(gacd d, float cf, string r);
    }

    public abstract class gacc : gaccheck
    {
        public string nm = "chk";
        public bool on = true;
        public int nflg;
        public float weight = 1f;
        public float cooldown = 0.5f;

        protected static readonly float[] zer = new float[gacp.slots];

        public string name
        {
            get { return nm; }
        }

        public bool en
        {
            get { return on; }
        }

        bool gaccheck.on
        {
            get { return on; }
            set { on = value; }
        }

        public int flags
        {
            get { return nflg; }
        }

        public float wgt
        {
            get { return gacr.wgt.of(nm); }
        }

        public float cdt
        {
            get { return cooldown; }
        }

        public int add()
        {
            nflg++;
            return nflg;
        }

        public abstract void tick(gacd d, float dt);

        public bool can(gacd d, float cf)
        {
            return gacr.canflg(d, this, cf);
        }

        public void flg(gacd d, float cf, string r)
        {
            gacr.flg(d, this, cf, r);
        }

        public void flg(gacd d, float cf, float a, string pre, string fmt)
        {
            if (!can(d, cf)) return;

            flg(d, cf, pre + gacn.f(a, fmt));
        }
    }
}