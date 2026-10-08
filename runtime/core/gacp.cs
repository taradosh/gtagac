using System;
using UnityEngine;

namespace gtagac
{
    public static class gacp
    {
        public const int slots = 32;
        public const int wnd = 8;

        public static float maxspeed = 12f;
        public static float maxvel = 20f;
        public static float tpd = 3f;
        public static float maxjmp = 9f;
        public static float maxair = 8f;
        public static float maxhand = 14f;
        public static float maxupd = 60f;

        public static float fth = 10f;
        public static float kth = 25f;
        public static float bth = 45f;
        public static float bcf = 0.9f;
        public static float maxwarn = 3f;
        public static float decay = 0.6f;

        public static int smp = 3;
        public static int tps = 2;

        public static float ivl = 0.05f;
        public static float burst = 1.35f;
        public static float actw = 0.6f;
        public static float minid = 0.008f;
        public static float maxid = 0.15f;
        public static float grav = 9.81f;

        public static float gtol = 2f;
        public static float gmin = 0.2f;
        public static float hov = 0.5f;
        public static float maxacc = 200f;
        public static float swgsp = 2f;

        public static float conacc = 60f;
        public static float consmp = 3f;

        public static float grdoff = 0.35f;
        public static float grdlen = 1.2f;
        public static int grdmask = -1;
        public static bool grdchk = true;

        public static float btmp = 1440f;
        public static bool allowsc = false;

        public static bool cs = true;
        public static bool ct = true;
        public static bool cj = true;
        public static bool cf = true;
        public static bool cg = true;
        public static bool cv = true;
        public static bool ca = true;
        public static bool cp = true;
        public static bool cr = true;
        public static bool ce = true;
        public static bool cc = true;

        public static bool dbg = false;
        public static bool dbgf = false;
        public static float dbgi = 0.25f;
        public static bool srv = false;
        public static bool loc = true;
        public static bool devby = true;
        public static bool pun = true;
        public static bool pwn = true;
        public static bool pkk = true;
        public static bool pkb = true;
        public static bool pbm = true;
        public static bool bper = false;

        public static string[] wl = new string[0];
        public static bool wlst = true;

        public static bool adm(string id)
        {
            for (int i = 0; i < wl.Length; i++)
            {
                if (wl[i] == id) return true;
            }
            return false;
        }

        public static float hash()
        {
            float[] a =
            {
                maxspeed, maxvel, tpd, maxjmp, maxair, maxhand, maxupd,
                fth, kth, bth, bcf, maxwarn, decay,
                ivl, burst, grav, gtol, gmin, hov, maxacc, swgsp,
                grdoff, grdlen, grdmask, btmp,
                smp, tps, grdchk ? 1f : 0f,
                cs ? 1f : 0f, ct ? 1f : 0f, cj ? 1f : 0f, cf ? 1f : 0f,
                cg ? 1f : 0f, cv ? 1f : 0f, ca ? 1f : 0f, cp ? 1f : 0f,
                cr ? 1f : 0f, ce ? 1f : 0f,
                pun ? 1f : 0f, pwn ? 1f : 0f, pkk ? 1f : 0f,
                pkb ? 1f : 0f, pbm ? 1f : 0f, bper ? 1f : 0f,
                devby ? 1f : 0f, loc ? 1f : 0f, allowsc ? 1f : 0f,
                wlst ? 1f : 0f
            };

            float h = 2166136261f;

            for (int i = 0; i < a.Length; i++)
            {
                int b = BitConverter.SingleToInt32Bits(a[i]);
                h = mx(h + b);
            }

            int n = wl != null ? wl.Length : 0;

            for (int i = 0; i < n; i++)
            {
                string s = wl[i] ?? "";
                for (int j = 0; j < s.Length; j++)
                {
                    h = mx(h + s[j]);
                }
                h = mx(h + 10);
            }

            return h;
        }

        static float mx(float v)
        {
            const float m = 16777619f;
            v *= m;
            return v - Mathf.Floor(v / m) * m;
        }

        public static bool sw(string n)
        {
            switch (n)
            {
                case "speed": cs = !cs; return cs;
                case "teleport": ct = !ct; return ct;
                case "jump": cj = !cj; return cj;
                case "fly": cf = !cf; return cf;
                case "gravity": cg = !cg; return cg;
                case "velocity": cv = !cv; return cv;
                case "arm": ca = !ca; return ca;
                case "position": cp = !cp; return cp;
                case "rate": cr = !cr; return cr;
                case "env": ce = !ce; return ce;
                case "consistency": cc = !cc; return cc;
            }
            return false;
        }

        public static bool on(string n)
        {
            switch (n)
            {
                case "speed": return cs;
                case "teleport": return ct;
                case "jump": return cj;
                case "fly": return cf;
                case "gravity": return cg;
                case "velocity": return cv;
                case "arm": return ca;
                case "position": return cp;
                case "rate": return cr;
                case "env": return ce;
                case "consistency": return cc;
            }
            return false;
        }

        public static KeyCode dbgkey = KeyCode.F1;

        public static bool dbgo()
        {
            return dbgf || (dbg && Debug.isDebugBuild);
        }

        public static void reset()
        {
            maxspeed = 12f;
            maxvel = 20f;
            tpd = 3f;
            maxjmp = 9f;
            maxair = 8f;
            maxhand = 14f;
            maxupd = 60f;
            fth = 10f;
            kth = 25f;
            bth = 45f;
            bcf = 0.9f;
            maxwarn = 3f;
            decay = 0.6f;
            ivl = 0.05f;
            burst = 1.35f;
            actw = 0.6f;
            minid = 0.008f;
            maxid = 0.15f;
            grav = 9.81f;
            smp = 3;
            tps = 2;
            gtol = 2f;
            gmin = 0.2f;
            hov = 0.5f;
            maxacc = 200f;
            swgsp = 2f;
            conacc = 60f;
            consmp = 3f;
            grdoff = 0.35f;
            grdlen = 1.2f;
            grdmask = -1;
            grdchk = true;
            btmp = 1440f;
            allowsc = false;
            cs = true; ct = true; cj = true; cf = true; cg = true;
            cv = true; ca = true; cp = true; cr = true; ce = true; cc = true;
            dbg = false; dbgf = false; dbgi = 0.25f;
            srv = false; loc = true; devby = true;
            pun = true; pwn = true; pkk = true; pkb = true; pbm = true;
            bper = false;
            wlst = true;
        }
    }
}