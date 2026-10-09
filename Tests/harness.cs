using System.Text;
using gtagac;
using NUnit.Framework;
using UnityEngine;

namespace gtagac.test
{
    public sealed class sim
    {
        public readonly gacd d;
        public float t;

        public sim(string id = "p", int slot = 0)
        {
            d = new gacd(id, slot);
            d.reset();
            d.loc = true;
            t = 0f;
        }

public sim at(float x, float y, float z)
        {
            d.push(new Vector3(x, y, z), v, t);
            return this;
        }

public Vector3 v = Vector3.zero;

        public sim vel(float x, float y, float z)
        {
            v = new Vector3(x, y, z);
            return this;
        }

        public sim mv(Vector3 mv)
        {
            v = mv;
            d.push(d.p + mv, mv, t);
            return this;
        }

        public sim step(float dt)
        {
            t += dt;
            gactime.now = t;
            gactime.dt = dt;
            return this;
        }

        public sim gnd(bool g)
        {
            d.grnd = g;
            return this;
        }

        public sim st(bool ground, bool jump, bool climb, bool swim, bool grab)
        {
            d.st(ground, jump, climb, swim, grab);
            return this;
        }
    }

    [SetUpFixture]
    public class env
    {
        [OneTimeSetUp]
        public void up()
        {
            gac.shut();
            gacp.reset();
            gacp.devby = false;
            gacs.onflag = null;
            gacs.onwarning = null;
            gacs.onkick = null;
            gacs.onban = null;
            gac.store = null;
            gacr.store = null;
            gacr.wgt = new gacw();
            gac.init();
            gactime.init();
        }
    }

    public static class tst
    {
        public static int flags;

public static int lastflg;

        public static readonly List<gace> flgs = new List<gace>();

        public static void arm()
        {
            flags = 0;
            checks = 0;
            lastflg = 0;
            names.Clear();
            flgs.Clear();

            gacs.onflag = e => { flags++; lastflg = (int)e.cfl; names.Add(e.chk + ":" + e.rsn); flgs.Add(e); };
            gacs.onwarning = null;
            gacs.onkick = null;
            gacs.onban = null;
        }

        public static int checks;

        public static readonly List<string> names = new List<string>();

/// <summary>
/// every legitimate scenario a check must stay silent for. shared by the false positive
/// guards so a new check cannot be added without proving it against the whole set.
/// </summary>
public static void legit()
        {
            walk(30f, 0.05f, 1.5f);
            walk(30f, 0.05f, 10.5f);
            walk(20f, 0.05f, 0f);
            jump(3f, 0.05f, 5f);
            climb(10f, 0.05f, 2f);
            swing(20f, 0.05f, 8f, 1.2f, 3.5f);
            noisy(30f, 0.05f, 3f, 0.08f, 11);
        }

        public static string What()
        {
            var d = new Dictionary<string, int>();

            foreach (string n in names)
            {
                d.TryGetValue(n, out int c);
                d[n] = c + 1;
            }

            var sb = new StringBuilder();

            foreach (var kv in d) sb.Append(kv.Key).Append(' ');

            return sb.ToString();
        }

        public static void boot()
        {
            gac.shut();
            gacp.reset();
            gacp.devby = false;
            gac.init();
            gactime.init();
            gactime.now = 100f;
            gactime.dt = 0.05f;
            arm();
        }

        public static void run(float n, float dt, Action<sim> step)
        {
            var s = new sim();

            for (int i = 0; i < n; i++)
            {
                step(s);
                s.step(dt);
                gac.evl(s.d);
            }
        }

        public static void walk(float seconds, float dt, float spd)
        {
            run((int)(seconds / dt), dt, s =>
            {
                s.mv(new Vector3(spd * dt, 0f, 0f));
                s.gnd(true);
            });
        }

        public static void swing(float seconds, float dt, float len, float amp, float period)
        {
            double ph = 0;

            run((int)(seconds / dt), dt, s =>
            {
                ph += dt / period * Math.PI * 2.0;

                double a = amp * Math.Sin(ph);
                float x = (float)(len * Math.Sin(a));
                float y = (float)(-len * Math.Cos(a));

                if (s.d.h.n == 0) s.at(x, y, 0f);
                else s.at(x, y, 0f);

                s.d.hl = null;
                s.d.hr = null;
                s.gnd(false);
            });
        }

        public static void climb(float seconds, float dt, float spd)
        {
            run((int)(seconds / dt), dt, s =>
            {
                s.mv(new Vector3(0f, spd * dt, 0f));
                s.st(false, false, true, false, true);
            });
        }

        public static void jump(float seconds, float dt, float v0)
        {
            double t = 0;

            run((int)(seconds / dt), dt, s =>
            {
                t += dt;

                float y = (float)(v0 * t - 0.5 * gacp.grav * t * t);
                float vy = (float)(v0 - gacp.grav * t);

                if (s.d.h.n == 0) s.at(0f, 0f, 0f);

                s.at(s.d.p.x, y, 0f).vel(0f, vy, 0f);
                s.gnd(t > (2 * v0 / gacp.grav));
            });
        }

        public static void noisy(float seconds, float dt, float spd, float jit, int seed)
        {
            var rnd = new Random(seed);
            float x = 0f;

            run((int)(seconds / dt), dt, s =>
            {
                float n = ((float)rnd.NextDouble() - 0.5f) * jit;
                x += spd * dt;

                s.at(x + n, n * 0.5f, n * 0.5f);
                s.gnd(true);
            });
        }
    }
}