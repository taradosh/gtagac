using UnityEngine;

namespace gtagac
{
    public sealed class gacd
    {
        public string id;
        public int ind;
        public bool loc;
        public bool srv;
        public bool adm;
        public bool tru;
        public bool kkd;
        public bool bnd;
        public bool lft;
        public bool chkd;

        public Transform tr;
        public Transform hl;
        public Transform hr;
        public Rigidbody bd;
        public igacnet net;

        public readonly gacv h = new gacv();

        public float spd;
        public float fspd;
        public float vspd;
        public float acc;
        public float hs;
        public float hsp;
        public float air;
        public float gy;
        public float jy;
        public float upd;
        public float grc;

        public int sq;
        public int drp;
        public int ol;
        public int lost;
        public int sus;
        public bool sqo;
        public double off;
        public double lc;
        public float lt;
        public float lst;
        public float jit;
        public float gl;
        public int gap;
        public float lag;
        public float pin;
        public float slack;
        public float sspd;
        public float unc;

        public bool grnd;
        public bool jmp;
        public bool clb;
        public bool swm;
        public bool fy;
        public bool grb;

        public gacd(string pid, int i)
        {
            id = pid;
            ind = i;
            h.clear();
        }

        public void reset()
        {
            h.clear();
            spd = 0f;
            fspd = 0f;
            vspd = 0f;
            acc = 0f;
            hs = 0f;
            hsp = 0f;
            air = 0f;
            gy = 0f;
            jy = 0f;
            upd = 0f;
            grc = 0f;
            sq = 0;
            drp = 0;
            ol = 0;
            lost = 0;
            sus = 0;
            sqo = false;
            off = 0.0;
            lc = 0.0;
            lt = 0f;
            lst = 0f;
            jit = 0f;
            gl = 0f;
            lag = 0f;
            pin = 0f;
            slack = 0f;
            sspd = 0f;
            unc = 0f;
            grnd = true;
            jmp = false;
            clb = false;
            swm = false;
            fy = false;
            grb = false;
        }

        public void st(bool ground, bool jump, bool climb, bool swim)
        {
            grnd = ground;
            jmp = jump;
            clb = climb;
            swm = swim;
        }

        public void st(bool ground, bool jump, bool climb, bool swim, bool grab)
        {
            grnd = ground;
            jmp = jump;
            clb = climb;
            swm = swim;
            grb = grab;
        }

        public bool swg()
        {
            if (grb) return true;
            if (clb) return true;
            if (hs > gacp.swgsp) return true;

            float dt = h.dlt(0);

            if (dt > gacp.minid && dt < gacp.maxid &&
                gacmath.flen(h.now - h.at(1)) / dt > gacp.swgsp) return true;

            return false;
        }

        public void grace(float t)
        {
            grc = gacmath.maxs(t, 0.1f);
        }

        public bool ingnd()
        {
            if (grnd) return true;
            if (clb) return true;
            if (swm) return true;
            if (bd != null && gacp.grdchk && gacgrd(bd)) return true;
            return false;
        }

        public static bool gacgrd(Rigidbody b)
        {
            Vector3 p = b.worldCenterOfMass;
            return Physics.Raycast(
                p + Vector3.up * gacp.grdoff,
                Vector3.down,
                gacp.grdlen,
                gacp.grdmask,
                QueryTriggerInteraction.Ignore);
        }

        public static Vector3 bvel(Rigidbody b)
        {
#if UNITY_6000_0_OR_NEWER
            return b.linearVelocity;
#else
            return b.velocity;
#endif
        }

        public void fall(float dt)
        {
            if (ingnd())
            {
                air = 0f;
                jmp = false;
                return;
            }
            air += dt;
        }

        public void push(Vector3 p, Vector3 v, float t)
        {
            h.add(p, v, t);
        }

        public void tck(float dt)
        {
            if (grc > 0f) grc -= dt;
        }

        public Vector3 p
        {
            get { return h.now; }
        }

        public Vector3 v
        {
            get { return h.vel; }
        }
    }
}