using UnityEngine;

namespace gtagac.smp
{
    public sealed class gacboot : MonoBehaviour
    {
        public Transform root;
        public Transform lefthand;
        public Transform righthand;
        public Rigidbody body;
        public KeyCode dbgkey = KeyCode.F1;

        void Awake()
        {
            gacp.maxspeed = 12f;
            gacp.maxvel = 20f;
            gacp.tpd = 3f;
            gacp.fth = 10;
            gacp.ivl = 0.05f;
            gacp.dbgf = Debug.isDebugBuild;
            gacp.dbgkey = dbgkey;

            gacs.onflag = ev;
            gacs.onwarning = ev;
            gacs.onkick = ev;
            gacs.onban = ev;
        }

        void Start()
        {
            if (root == null) root = transform;
            gac.init(root, lefthand, righthand, body);
        }

        void Update()
        {
            gac.tick();
        }

        void FixedUpdate()
        {
            gac.phys();
        }

        void OnDestroy()
        {
            gac.shut();
        }

        static void ev(gace e)
        {
            Debug.Log("[gtagac] " + e.id + " " + e.chk + " " + e.rsn +
                " cf=" + e.cf.ToString("0.00") +
                " fl=" + e.cfl +
                " t=" + e.tm.ToString("0.0"));
        }
    }
}