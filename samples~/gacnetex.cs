using UnityEngine;

namespace gtagac.smp
{
    public sealed class gacnetl : igacnet
    {
        public string pid = "p1";
        public bool svr;
        public Vector3 p;
        public Vector3 v;
        public bool g;

        public string id
        {
            get { return pid; }
        }

        public bool srv
        {
            get { return svr; }
        }

        public Vector3 pos
        {
            get { return p; }
        }

        public Vector3 vel
        {
            get { return v; }
        }

        public float t
        {
            get { return gactime.now; }
        }

        public bool grnd
        {
            get { return g; }
        }

        public void set(Vector3 np, Vector3 nv, bool ng)
        {
            p = np;
            v = nv;
            g = ng;
        }

        public void kick(string r) { }

        public void ban(string r, float m) { }
    }

    public sealed class gacnetex : MonoBehaviour
    {
        public Transform root;
        public Rigidbody body;
        public string pid = "p1";

        gacnetl net;

        void Start()
        {
            net = new gacnetl();
            net.pid = pid;
            gac.init(net);
        }

        void Update()
        {
            gac.tick();
        }

        void FixedUpdate()
        {
            gac.phys();
        }

        public void late()
        {
            if (root == null) return;
            Vector3 p = root.position;
            Vector3 v = body != null ? body.velocity : Vector3.zero;
            bool g = body != null ? body.IsGrounded() : true;
            net.set(p, v, g);
        }

        public void kick(string r)
        {
            gac.kick(pid, r);
        }

        public void ban(string r, float m)
        {
            gac.ban(pid, r, m);
        }
    }

    public sealed class gacsrvhst : MonoBehaviour
    {
        public void Start()
        {
            gacsrv.init();
        }

        public void onjoin(string id)
        {
            gacsrv.join(id);
        }

        public void onmove(string id, Vector3 p, Vector3 v, bool g, float t)
        {
            gacsrv.up(id, p, v, g, t);
        }

        public void onleave(string id)
        {
            gacsrv.leave(id);
        }
    }
}