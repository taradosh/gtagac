using System.Text;

namespace gtagac
{
    public static class gacdiag
    {
        public static string txt()
        {
            var sb = new StringBuilder();

            sb.Append("gtagac diag v").Append(gac.ver).Append('\n');
            sb.Append("cfg ").Append(gacn.f(gacp.hash(), "0")).Append('\n');
            sb.Append("mode ").Append(gac.srv ? "server" : "client");
            sb.Append(' ').Append(gac.loc ? "local" : "remote");
            sb.Append(' ').Append(gacp.dbgo() ? "debug" : "release").Append('\n');
            sb.Append("checks ").Append(gacr.cnum).Append('\n');
            sb.Append("events ").Append(gaclog.kept);
            if (gaclog.lost > 0) sb.Append(" dropped ").Append(gaclog.lost);
            sb.Append('\n');

            for (int i = 0; i < gac.ply.Count; i++)
            {
                ply(sb, gac.ply[i]);
            }

            sb.Append("--\n");

            string e = gaclog.txt();

            if (e.Length > 0) sb.Append(e);

            return sb.ToString();
        }

        static void ply(StringBuilder sb, gacd d)
        {
            gacper p = gacr.get(d.id);

            sb.Append('\n').Append("ply ").Append(d.id).Append('\n');
            sb.Append("  flags ").Append(gacr.flgof(d.id));
            sb.Append(" conf ").Append(gacn.f(gacr.cfof(d.id), "0.00"));
            sb.Append(" warn ").Append(p.warn);
            sb.Append(" kick ").Append(p.kick);
            sb.Append(" ban ").Append(p.ban).Append('\n');

            sb.Append("  chk ");

            for (int i = 0; i < p.f.len; i++)
            {
                gacfe e = p.f.at(i);

                sb.Append(e.chk).Append('=').Append(e.n);
                sb.Append('/').Append(gacn.f(e.cf, "0.00")).Append(' ');
            }

            sb.Append('\n');

            sb.Append("  net seq ").Append(d.sq);
            sb.Append(" drops ").Append(d.drp);
            sb.Append(" ooo ").Append(d.ol);
            sb.Append(" lost ").Append(d.lost);
            sb.Append(" susp ").Append(d.sus).Append('\n');

            sb.Append("  jit ").Append(gacn.f(d.jit, "0.000"));
            sb.Append(" lag ").Append(gacn.f(d.lag, "0.000"));
            sb.Append(" last ").Append(gacn.f(d.lst, "0.000"));
            sb.Append(" off ").Append(gacn.f(d.off, "0.000"));
            sb.Append(" slack ").Append(gacn.f(d.slack, "0.00"));
            sb.Append(" unc ").Append(gacn.f(d.unc, "0.00")).Append('\n');

            sb.Append("  spd ").Append(gacn.f(d.spd, "0.00"));
            sb.Append(" air ").Append(gacn.f(d.air, "0.00"));
            sb.Append(" upd ").Append(gacn.f(d.upd, "0"));
            sb.Append(" n ").Append(d.h.n);
            sb.Append(" ground ").Append(d.ingnd() ? 1 : 0).Append('\n');

            if (gacp.evon)
            {
                sb.Append("  gate ").Append(gacr.evok(d.id) ? "pass" : "block");
                sb.Append(" ind ").Append(gacr.evind(d.id));
                sb.Append('/').Append(gacn.f(gacp.evmin, "0"));
                sb.Append(" cf ").Append(gacn.f(gacr.cfof(d.id), "0.00"));
                sb.Append('/').Append(gacn.f(gacp.evcf, "0.00")).Append('\n');
            }
        }

        public static string csv()
        {
            return gaclog.csv();
        }

        public static bool save(string path)
        {
            try
            {
                System.IO.File.WriteAllText(path, txt());

                return true;
            }
            catch (System.Exception e)
            {
                gaclog.e("diag save " + e.Message);

                return false;
            }
        }
    }
}