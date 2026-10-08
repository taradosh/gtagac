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

        public void flg(gacd d, float cf, string r)
        {
            gacr.flg(d, this, cf, r);
        }
    }
}