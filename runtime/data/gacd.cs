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