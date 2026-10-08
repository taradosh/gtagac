using UnityEngine;

namespace gtagac
{
    public sealed class gacgui : MonoBehaviour
    {
        public int sz = 13;
        public Color col = Color.white;
        public float w = 340f;
        public float h = 250f;

        GUIStyle st;
        string nm = "";

        void OnGUI()
        {
            if (!gacp.dbgo()) return;
            if (gac.me == null) return;

            if (st == null)
            {
                st = new GUIStyle();
                st.fontSize = sz;
                st.wordWrap = false;
                st.alignment = TextAnchor.UpperLeft;
                st.normal.textColor = col;
            }

            st.normal.textColor = col;

            GUILayout.BeginArea(new Rect(8f, 8f, w, h), st);

            GUILayout.Label("gtag ac " + gac.me.id);
            GUILayout.Label("spd " + gac.me.spd.ToString("0.0") + "  hspd " + gac.me.fspd.ToString("0.0") + "  vspd " + gac.me.vspd.ToString("0.0"));
            GUILayout.Label("vel " + gac.me.v.ToString("0.00"));
            GUILayout.Label("pos " + gac.me.p.ToString("0.00"));
            GUILayout.Label("flg " + gacr.flgof(gac.me.id) + "  cfl " + gacr.cfof(gac.me.id).ToString("0.00") + "  wrn " + gacr.warnof(gac.me.id));
            GUILayout.Label("air " + gac.me.air.ToString("0.0") + "  grnd " + gac.me.grnd + "  hnd " + gac.me.hs.ToString("0.0"));
            GUILayout.Label(nm);

            GUILayout.EndArea();
        }

        void Update()
        {
            if (!gacp.dbgo()) return;
            if (gac.me == null) return;

            float acc = Time.unscaledDeltaTime;

            if (acc < gacp.dbgi) return;

            string s = "";

            for (int i = 0; i < gac.cnum; i++)
            {
                gaccheck c = gac.chk(i);
                s += c.name + (c.en ? "" : "*") + " ";
            }

            nm = s;
        }
    }
}