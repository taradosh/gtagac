using UnityEngine;

namespace gtagac
{
    public sealed class gacdw : MonoBehaviour
    {
        public float iv = 0.25f;

        bool on;
        float acc;

        public bool st
        {
            get { return on; }
        }

        void Update()
        {
            if (gacp.dbgo() && Input.GetKeyDown(gacp.dbgkey))
            {
                on = !on;
                gacp.dbg = on;
                gaclog.i("dbg " + on);
            }

            if (!gacp.dbgo()) return;
            if (gac.me == null) return;

            acc += Time.unscaledDeltaTime;
            if (acc < iv) return;
            acc = 0f;

            gacd d = gac.me;

            gaclog.i(
                d.id +
                " spd " + d.spd.ToString("0.0") +
                " vel " + d.v.ToString("0.0") +
                " flg " + gacr.flgof(d.id) +
                " cfl " + gacr.cfof(d.id).ToString("0.00") +
                " chk " + chks());
        }

        static string chks()
        {
            string s = "";
            for (int i = 0; i < gac.cnum; i++)
            {
                gaccheck c = gac.chk(i);
                s += c.name + (c.en ? "" : "*") + " ";
            }
            return s;
        }
    }
}