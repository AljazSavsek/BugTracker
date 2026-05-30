using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BugTracker.Models;

namespace BugTracker.Helpers
{
    public static class TxtHelper
    {
        public static string Export(List<BugItem> bugs, string path = null)
        {
            if (path == null)
                path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"BugTracker_Porocilo_{DateTime.Now:yyyyMMdd_HHmm}.txt");

            var sb = new StringBuilder();

            // ── Glava poročila ─────────────────────────────────────────
            sb.AppendLine("================================================================================");
            sb.AppendLine("  BUGTRACKER – POROČILO O NAPAKAH");
            sb.AppendLine($"  Izvoženo:    {DateTime.Now:dd.MM.yyyy  HH:mm:ss}");
            sb.AppendLine($"  Skupaj napak: {bugs.Count}");
            sb.AppendLine("================================================================================");
            sb.AppendLine();

            // ── Povzetek po statusu ────────────────────────────────────
            int odprtih = 0, vDelu = 0, resenih = 0, zaprtih = 0, kriticnih = 0;
            foreach (var b in bugs)
            {
                if (b.Status    == "Odprt")    odprtih++;
                if (b.Status    == "V delu")   vDelu++;
                if (b.Status    == "Rešen")    resenih++;
                if (b.Status    == "Zaprt")    zaprtih++;
                if (b.Prioriteta== "Kritična") kriticnih++;
            }

            sb.AppendLine("  POVZETEK:");
            sb.AppendLine($"  {"Odprtih",-16} {odprtih}");
            sb.AppendLine($"  {"V delu",-16} {vDelu}");
            sb.AppendLine($"  {"Rešenih",-16} {resenih}");
            sb.AppendLine($"  {"Zaprtih",-16} {zaprtih}");
            sb.AppendLine($"  {"Kritičnih",-16} {kriticnih}");
            sb.AppendLine();
            sb.AppendLine("================================================================================");
            sb.AppendLine();

            // ── Posamezne napake ───────────────────────────────────────
            foreach (var b in bugs)
            {
                sb.AppendLine($"── NAPAKA #{b.IdNapake} {new string('─', Math.Max(0, 70 - b.IdNapake.ToString().Length))}");
                sb.AppendLine($"  Naslov:      {b.Naslov}");
                sb.AppendLine($"  Status:      {b.Status}");
                sb.AppendLine($"  Prioriteta:  {b.Prioriteta}");
                sb.AppendLine($"  Kategorija:  {b.Kategorija ?? "–"}");
                sb.AppendLine($"  Dodeljen:    {b.Dodeljen   ?? "–"}");
                sb.AppendLine($"  Ustvaril:    {b.Ustvaritelj ?? "–"}");
                sb.AppendLine($"  Ustvarjeno:  {b.DatumUstvarjeno:dd.MM.yyyy}");
                sb.AppendLine($"  Spremenjeno: {b.DatumSpremenjeno:dd.MM.yyyy}");
                if (!string.IsNullOrWhiteSpace(b.Opis))
                {
                    sb.AppendLine();
                    sb.AppendLine("  Opis:");
                    foreach (var line in b.Opis.Split('\n'))
                        sb.AppendLine($"    {line.TrimEnd()}");
                }
                sb.AppendLine();

                // Blok za ročne opombe (za poročilo)
                sb.AppendLine("  [ Opombe za poročilo: ]");
                sb.AppendLine("  ________________________________________________________________________");
                sb.AppendLine();
                sb.AppendLine();
            }

            sb.AppendLine("================================================================================");
            sb.AppendLine("  KONEC POROČILA");
            sb.AppendLine("================================================================================");

            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            return path;
        }
        public static (List<BugItem> bugs, List<string> opozorila) Import(string path)
        {
            var bugs      = new List<BugItem>();
            var opozorila = new List<string>();

            if (!File.Exists(path))
            { opozorila.Add("Datoteka ne obstaja."); return (bugs, opozorila); }

            string[] lines = File.ReadAllLines(path, Encoding.UTF8);

            BugItem   current  = null;
            var       opisBuf  = new List<string>();
            bool      inOpis   = false;

            void Flush()
            {
                if (current == null) return;
                if (string.IsNullOrWhiteSpace(current.Naslov))
                { opozorila.Add($"Bug brez naslova preskočen."); current = null; return; }
                if (opisBuf.Count > 0)
                    current.Opis = string.Join(Environment.NewLine, opisBuf).Trim();
                bugs.Add(current);
                current = null;
                opisBuf.Clear();
                inOpis  = false;
            }

            foreach (string rawLine in lines)
            {
                string line = rawLine.TrimEnd();

                if (line.TrimStart().StartsWith("── NAPAKA #") ||
                    line.TrimStart().StartsWith("-- NAPAKA #"))
                {
                    Flush();
                    current = new BugItem { Status = "Odprt", Prioriteta = "Srednja" };
                    inOpis  = false;
                    opisBuf.Clear();
                    continue;
                }
                if (current == null) continue;

                bool isKeyLine = false;
                string trimmed = line.TrimStart();

                if (trimmed.StartsWith("[ Opombe")         ||
                    trimmed.StartsWith("[Opombe")           ||
                    trimmed.All(c => c == '_' || c == ' ')  ||
                    trimmed.StartsWith("────") || trimmed.StartsWith("----") ||
                    trimmed.StartsWith("===="))
                {
                    inOpis = false;
                    continue;
                }


                int colon = trimmed.IndexOf(':');
                if (colon > 0)
                {
                    string key = trimmed.Substring(0, colon).Trim();
                    string val = trimmed.Substring(colon + 1).Trim();

                    switch (key.ToLower())
                    {
                        case "naslov":
                            inOpis = false;
                            current.Naslov = val;
                            isKeyLine = true; break;

                        case "status":
                            inOpis = false;
                            current.Status = Normalize(val,
                                new[] { "Odprt","V delu","Rešen","Zaprt","Ponovno odprt" }, "Odprt");
                            isKeyLine = true; break;

                        case "prioriteta":
                            inOpis = false;
                            current.Prioriteta = Normalize(val,
                                new[] { "Nizka","Srednja","Visoka","Kritična" }, "Srednja");
                            isKeyLine = true; break;

                        case "kategorija":
                            inOpis = false;
                            current.Kategorija = val == "–" ? null : val;
                            isKeyLine = true; break;

                        case "dodeljen":
                            inOpis = false;
                            if (val != "–")
                            {
                                int lb = val.LastIndexOf('(');
                                int rb = val.LastIndexOf(')');
                                current.Dodeljen = (lb >= 0 && rb > lb)
                                    ? val.Substring(lb + 1, rb - lb - 1).Trim()
                                    : val.Trim();
                            }
                            isKeyLine = true; break;

                        case "ustvaril":
                        case "ustvarjeno":
                        case "spremenjeno":

                            inOpis = false;
                            isKeyLine = true; break;

                        case "opis":

                            opisBuf.Clear();
                            if (!string.IsNullOrEmpty(val)) opisBuf.Add(val);
                            inOpis = true;
                            isKeyLine = true; break;
                    }
                }

                if (!isKeyLine && inOpis)
                {

                    if (string.IsNullOrWhiteSpace(line))
                        inOpis = false;
                    else
                        opisBuf.Add(trimmed);
                }
            }

            Flush();
            return (bugs, opozorila);
        }

        private static string Normalize(string value, string[] allowed, string fallback)
        {
            if (string.IsNullOrWhiteSpace(value)) return fallback;
            foreach (var a in allowed)
                if (string.Equals(a, value.Trim(), StringComparison.OrdinalIgnoreCase))
                    return a;
            return fallback;
        }


        public static string SampleImportContent()
        {
            var sample = new List<BugItem>
            {
                new BugItem
                {
                    IdNapake=1, Naslov="Napaka pri nalaganju slike",
                    Status="Odprt", Prioriteta="Visoka",
                    Kategorija="Backend", Dodeljen="jnovak",
                    Ustvaritelj="admin",
                    DatumUstvarjeno=DateTime.Today, DatumSpremenjeno=DateTime.Today,
                    Opis="Ob nalaganju slike večje od 5 MB se pojavi napaka 500."
                },
                new BugItem
                {
                    IdNapake=2, Naslov="Gumb za brisanje ne deluje na mobilnih napravah",
                    Status="Odprt", Prioriteta="Srednja",
                    Kategorija="UI", Dodeljen="mkovac",
                    Ustvaritelj="admin",
                    DatumUstvarjeno=DateTime.Today, DatumSpremenjeno=DateTime.Today,
                    Opis="Na iOS 17 se gumb za brisanje ne odziva na dotik."
                },
                new BugItem
                {
                    IdNapake=3, Naslov="Napačen datum v potrditvenem e-mailu",
                    Status="V delu", Prioriteta="Nizka",
                    Kategorija="Backend", Dodeljen="jnovak",
                    Ustvaritelj="admin",
                    DatumUstvarjeno=DateTime.Today, DatumSpremenjeno=DateTime.Today,
                    Opis="Potrditveni email prikazuje napačno časovno cono."
                }
            };

            string tmp = Path.Combine(Path.GetTempPath(), $"bt_vzorec_{DateTime.Now:HHmmss}.txt");
            Export(sample, tmp);
            string content = File.ReadAllText(tmp, Encoding.UTF8);
            try { File.Delete(tmp); } catch { }
            return content;
        }
    }
}
