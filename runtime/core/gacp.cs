using System;
using UnityEngine;

namespace gtagac
{
    public static class gacp
    {
        public const int slots = 32;
        public const int wnd = 8;

        public static float maxspeed = 18f;
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
        public static float flyhold = 1.5f;
        public static float grvhold = 1.5f;
        public static float poshold = 0.2f;
        public static float maxacc = 200f;
        public static float swgsp = 2f;

        public static float conacc = 60f;
        public static float consmp = 3f;
        public static float conunc = 0.08f;
        public static float conratio = 1.35f;

        public static bool strict = false;
        public static float stale = 0.35f;
        public static float jitter = 0.05f;
        public static float loss = 0.1f;
        public static float pin = 0f;
        public static float pingmax = 2f;
        public static float netslack = 1.25f;
        public static float seqwin = 32;
        public static float seqgap = 8;
        public static float fut = 0.5f;
        public static float maxun = 0.5f;
        public static float maxslack = 6f;

        public static float ncviol = 2f;
        public static float ncsmpl = 1f;
        public static float nccap = 0.05f;
        public static float ncahead = 0.15f;
        public static float nccf = 0.5f;
        public static float ncdot = 0.1f;
        public static int ncmask = -1;
        public static bool ncloc = false;

        public static bool prd = false;
        public static float prdmax = 1f;
        public static float prdmaxoff = 3f;
        public static float prdrate = 4f;

        public static bool evon = false;
        public static float evmin = 2f;
        public static float evcf = 0.35f;
        public static float evwin = 30f;

        public static float loglim = 8f;
        public static float logwin = 10f;
        public static int logkeep = 512;
        public static bool logon = true;
        public static bool logn = false;

        public static int evkeep = 64;

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
        public static bool cn = true;
        public static bool pr = false;

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
            uint h = 2166136261u;

            h = mix(h, maxspeed);
            h = mix(h, maxvel);
            h = mix(h, tpd);
            h = mix(h, maxjmp);
            h = mix(h, maxair);
            h = mix(h, maxhand);
            h = mix(h, maxupd);
            h = mix(h, fth);
            h = mix(h, kth);
            h = mix(h, bth);
            h = mix(h, bcf);
            h = mix(h, maxwarn);
            h = mix(h, decay);
            h = mix(h, ivl);
            h = mix(h, burst);
            h = mix(h, grav);
            h = mix(h, gtol);
            h = mix(h, gmin);
            h = mix(h, hov);
            h = mix(h, maxacc);
            h = mix(h, swgsp);
            h = mix(h, grdoff);
            h = mix(h, grdlen);
            h = mix(h, grdmask);
            h = mix(h, btmp);
            h = mix(h, smp);
            h = mix(h, tps);
            h = mix(h, strict ? 1f : 0f);
            h = mix(h, stale * 1000f);
            h = mix(h, jitter * 1000f);
            h = mix(h, loss * 1000f);
            h = mix(h, pin * 1000f);
            h = mix(h, pingmax * 1000f);
            h = mix(h, loglim * 1000f);
            h = mix(h, logwin * 1000f);
            h = mix(h, logkeep);
            h = mix(h, logon ? 1f : 0f);
            h = mix(h, logn ? 1f : 0f);
            h = mix(h, evon ? 1f : 0f);
            h = mix(h, evmin * 1000f);
            h = mix(h, evcf * 1000f);
            h = mix(h, evwin * 1000f);
            h = mix(h, prdmax * 1000f);
            h = mix(h, prdmaxoff * 1000f);
            h = mix(h, prdrate * 1000f);
            h = mix(h, ncviol * 1000f);
            h = mix(h, ncsmpl * 1000f);
            h = mix(h, nccap * 1000f);
            h = mix(h, ncahead * 1000f);
            h = mix(h, nccf * 1000f);
            h = mix(h, ncdot * 1000f);
            h = mix(h, ncmask);

            h = mix(h, cs ? 1f : 0f);
            h = mix(h, ct ? 1f : 0f);
            h = mix(h, cj ? 1f : 0f);
            h = mix(h, cf ? 1f : 0f);
            h = mix(h, cg ? 1f : 0f);
            h = mix(h, cv ? 1f : 0f);
            h = mix(h, ca ? 1f : 0f);
            h = mix(h, cp ? 1f : 0f);
            h = mix(h, cr ? 1f : 0f);
            h = mix(h, ce ? 1f : 0f);
            h = mix(h, cn ? 1f : 0f);
            h = mix(h, pr ? 1f : 0f);
            h = mix(h, ncloc ? 1f : 0f);

            h = mix(h, pun ? 1f : 0f);
            h = mix(h, pwn ? 1f : 0f);
            h = mix(h, pkk ? 1f : 0f);
            h = mix(h, pkb ? 1f : 0f);
            h = mix(h, pbm ? 1f : 0f);
            h = mix(h, bper ? 1f : 0f);
            h = mix(h, devby ? 1f : 0f);
            h = mix(h, loc ? 1f : 0f);
            h = mix(h, allowsc ? 1f : 0f);
            h = mix(h, wlst ? 1f : 0f);

            if (wl != null)
            {
                for (int i = 0; i < wl.Length; i++)
                {
                    string s = wl[i] ?? "";

                    for (int j = 0; j < s.Length; j++)
                        h = (h ^ s[j]) * 16777619u;

                    h = (h ^ 10u) * 16777619u;
                }
            }

            float f = h & 0x00ffffffu;

            return f == 0f ? 1f : f;
        }

        static uint mix(uint h, float v)
        {
            unchecked
            {
                return (h ^ (uint)BitConverter.SingleToInt32Bits(v)) * 16777619u;
            }
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
                case "noclip": cn = !cn; return cn;
                case "predict": pr = !pr; return pr;
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
                case "noclip": return cn;
                case "predict": return pr;
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
            maxspeed = 18f;
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
            flyhold = 1.5f;
            grvhold = 1.5f;
            poshold = 0.2f;
            maxacc = 200f;
            swgsp = 2f;
            conacc = 60f;
            consmp = 3f;
            conunc = 0.08f;
            conratio = 1.35f;
            grdoff = 0.35f;
            grdlen = 1.2f;
            grdmask = -1;
            grdchk = true;
            btmp = 1440f;
            allowsc = false;
            cs = true; ct = true; cj = true; cf = true; cg = true;
            cv = true; ca = true; cp = true; cr = true; ce = true; cc = true;
            cn = true; pr = false;
            strict = false;
            stale = 0.35f;
            jitter = 0.05f;
            loss = 0.1f;
pin = 0f;
            pingmax = 2f;
            netslack = 1.25f;
            seqwin = 32;
            seqgap = 8;
            fut = 0.5f;
            maxun = 0.5f;
            maxslack = 6f;
            ncviol = 2f;
            ncsmpl = 1f;
            nccap = 0.05f;
            ncahead = 0.15f;
            nccf = 0.5f;
            ncdot = 0.1f;
            ncmask = -1;
            ncloc = false;
            prdmax = 1f;
            prdmaxoff = 3f;
            prdrate = 4f;
            evon = false;
            evmin = 2f;
            evcf = 0.35f;
            evwin = 30f;
            loglim = 8f;
            logwin = 10f;
            logkeep = 512;
            logon = true;
            logn = false;
            evkeep = 64;
            dbg = false; dbgf = false; dbgi = 0.25f;
            srv = false; loc = true; devby = true;
            pun = true; pwn = true; pkk = true; pkb = true; pbm = true;
            bper = false;
            wlst = true;
        }
    }
}