using UnityEngine;

namespace gtagac
{
    public sealed class gacm : MonoBehaviour
    {
        public Transform root;
        public Transform hl;
        public Transform hr;
        public Rigidbody body;

        void Awake()
        {
            if (root == null) root = transform;
        }

        void Start()
        {
            gac.init(root, hl, hr, body);
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
            if (gac.me != null && gac.me.tr == root) gac.shut();
        }
    }
}