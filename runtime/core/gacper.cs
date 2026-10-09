namespace gtagac
{
    public sealed class gacper
    {
        public gacf f = new gacf();
        public int warn;
        public int kick;
        public int ban;
        public float kickt;
        public float bant;
        public bool sent;
        public float lgw;
        public int lgn;

        public void reset()
        {
            f.reset();
            warn = 0;
            kick = 0;
            ban = 0;
            kickt = 0f;
            bant = 0f;
            sent = false;
            lgw = -999f;
            lgn = 0;
        }
    }
}