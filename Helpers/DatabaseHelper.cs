using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using BugTracker.Models;

namespace BugTracker.Helpers
{
    public class DatabaseHelper
    {
        // ── Prilagodite geslo za MySQL ──────────────────────────────────
        private const string ConnStr =
            "Server=localhost;Port=3306;Uid=root;Pwd=root;Database=BugTracker";

        private MySqlConnection Conn() => new MySqlConnection(ConnStr);

        // ══════════════════════════════════════════════════════════════
        //  AVTENTIKACIJA
        // ══════════════════════════════════════════════════════════════

        public bool Login(string username, string password)
        {
            const string sql = @"
                SELECT id_uporabnika, uporabnisko_ime, geslo, vloga
                FROM   UPORABNIKI
                WHERE  uporabnisko_ime = @u AND aktiven = 1";

            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@u", username);
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return false;
            if (!BCrypt.Net.BCrypt.Verify(password, r.GetString("geslo"))) return false;

            SessionManager.UserId   = r.GetInt32("id_uporabnika");
            SessionManager.Username = r.GetString("uporabnisko_ime");
            SessionManager.Vloga    = r.GetString("vloga");
            return true;
        }

        public bool Register(string username, string password,
                             string ime, string priimek, string email)
        {
            if (UserExists(username)) return false;

            string hash = BCrypt.Net.BCrypt.HashPassword(password);
            const string sql = @"
                INSERT INTO UPORABNIKI
                    (uporabnisko_ime, geslo, ime, priimek, email, vloga, aktiven)
                VALUES (@u, @h, @ime, @pri, @email, 'Developer', 1)";

            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@u",     username);
            cmd.Parameters.AddWithValue("@h",     hash);
            cmd.Parameters.AddWithValue("@ime",   ime);
            cmd.Parameters.AddWithValue("@pri",   priimek);
            cmd.Parameters.AddWithValue("@email", email);
            return cmd.ExecuteNonQuery() > 0;
        }

        private bool UserExists(string username)
        {
            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(
                "SELECT COUNT(*) FROM UPORABNIKI WHERE uporabnisko_ime=@u", c);
            cmd.Parameters.AddWithValue("@u", username);
            return (long)cmd.ExecuteScalar() > 0;
        }

        // ══════════════════════════════════════════════════════════════
        //  NAPAKE
        // ══════════════════════════════════════════════════════════════

        public List<BugItem> GetAllBugs(string search = "",
                                         string status = "",
                                         string prioriteta = "")
        {
            var list = new List<BugItem>();
            string sql = @"
                SELECT n.id_napake, n.naslov, n.opis, n.status, n.prioriteta,
                       n.id_kategorije, k.naziv AS kategorija,
                       n.id_ustvaritelja, CONCAT(u1.ime,' ',u1.priimek) AS ustvaritelj,
                       n.id_dodeljenega,  CONCAT(u2.ime,' ',u2.priimek) AS dodeljen,
                       n.datum_ustvarjeno, n.datum_spremenjeno
                FROM   NAPAKE n
                LEFT JOIN KATEGORIJE k  ON n.id_kategorije  = k.id_kategorije
                LEFT JOIN UPORABNIKI u1 ON n.id_ustvaritelja = u1.id_uporabnika
                LEFT JOIN UPORABNIKI u2 ON n.id_dodeljenega  = u2.id_uporabnika
                WHERE 1=1";

            if (!string.IsNullOrEmpty(search))     sql += " AND n.naslov LIKE @s";
            if (!string.IsNullOrEmpty(status))     sql += " AND n.status = @st";
            if (!string.IsNullOrEmpty(prioriteta)) sql += " AND n.prioriteta = @p";
            sql += " ORDER BY n.datum_ustvarjeno DESC";

            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(sql, c);
            if (!string.IsNullOrEmpty(search))     cmd.Parameters.AddWithValue("@s",  $"%{search}%");
            if (!string.IsNullOrEmpty(status))     cmd.Parameters.AddWithValue("@st", status);
            if (!string.IsNullOrEmpty(prioriteta)) cmd.Parameters.AddWithValue("@p",  prioriteta);

            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(MapBug(r));
            return list;
        }

        public BugItem GetBugById(int id)
        {
            const string sql = @"
                SELECT n.id_napake, n.naslov, n.opis, n.status, n.prioriteta,
                       n.id_kategorije, k.naziv AS kategorija,
                       n.id_ustvaritelja, CONCAT(u1.ime,' ',u1.priimek) AS ustvaritelj,
                       n.id_dodeljenega,  CONCAT(u2.ime,' ',u2.priimek) AS dodeljen,
                       n.datum_ustvarjeno, n.datum_spremenjeno
                FROM   NAPAKE n
                LEFT JOIN KATEGORIJE k  ON n.id_kategorije  = k.id_kategorije
                LEFT JOIN UPORABNIKI u1 ON n.id_ustvaritelja = u1.id_uporabnika
                LEFT JOIN UPORABNIKI u2 ON n.id_dodeljenega  = u2.id_uporabnika
                WHERE  n.id_napake = @id";

            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", id);
            using var r = cmd.ExecuteReader();
            return r.Read() ? MapBug(r) : null;
        }

        public int InsertBug(BugItem b)
        {
            const string sql = @"
                INSERT INTO NAPAKE
                    (naslov, opis, status, prioriteta,
                     id_kategorije, id_ustvaritelja, id_dodeljenega)
                VALUES (@n,@o,@st,@pr,@kat,@usr,@del);
                SELECT LAST_INSERT_ID();";

            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@n",   b.Naslov);
            cmd.Parameters.AddWithValue("@o",   b.Opis ?? "");
            cmd.Parameters.AddWithValue("@st",  b.Status);
            cmd.Parameters.AddWithValue("@pr",  b.Prioriteta);
            cmd.Parameters.AddWithValue("@kat", b.IdKategorije.HasValue ? (object)b.IdKategorije.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@usr", b.IdUstvaritelja);
            cmd.Parameters.AddWithValue("@del", b.IdDodeljenega.HasValue  ? (object)b.IdDodeljenega.Value  : DBNull.Value);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool UpdateBug(BugItem nova, BugItem stara)
        {
            const string sql = @"
                UPDATE NAPAKE SET
                    naslov=@n, opis=@o, status=@st, prioriteta=@pr,
                    id_kategorije=@kat, id_dodeljenega=@del
                WHERE id_napake=@id";

            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@n",   nova.Naslov);
            cmd.Parameters.AddWithValue("@o",   nova.Opis ?? "");
            cmd.Parameters.AddWithValue("@st",  nova.Status);
            cmd.Parameters.AddWithValue("@pr",  nova.Prioriteta);
            cmd.Parameters.AddWithValue("@kat", nova.IdKategorije.HasValue ? (object)nova.IdKategorije.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@del", nova.IdDodeljenega.HasValue  ? (object)nova.IdDodeljenega.Value  : DBNull.Value);
            cmd.Parameters.AddWithValue("@id",  nova.IdNapake);
            cmd.ExecuteNonQuery();

            LogChanges(nova, stara);
            return true;
        }

        public bool DeleteBug(int id)
        {
            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(
                "DELETE FROM NAPAKE WHERE id_napake=@id", c);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        private void LogChanges(BugItem nova, BugItem stara)
        {
            if (nova.Naslov     != stara.Naslov)
                LogHistory(nova.IdNapake, SessionManager.UserId, "naslov",     stara.Naslov,     nova.Naslov);
            if (nova.Status     != stara.Status)
                LogHistory(nova.IdNapake, SessionManager.UserId, "status",     stara.Status,     nova.Status);
            if (nova.Prioriteta != stara.Prioriteta)
                LogHistory(nova.IdNapake, SessionManager.UserId, "prioriteta", stara.Prioriteta, nova.Prioriteta);
            if (nova.Kategorija != stara.Kategorija)
                LogHistory(nova.IdNapake, SessionManager.UserId, "kategorija", stara.Kategorija ?? "–", nova.Kategorija ?? "–");
            if (nova.Dodeljen   != stara.Dodeljen)
                LogHistory(nova.IdNapake, SessionManager.UserId, "dodeljen",   stara.Dodeljen   ?? "nihče", nova.Dodeljen ?? "nihče");
        }

        public void LogHistory(int idNapake, int idUporabnika,
                               string polje, string staraVrednost, string novaVrednost)
        {
            const string sql = @"
                INSERT INTO ZGODOVINA
                    (id_napake, id_uporabnika, polje, stara_vrednost, nova_vrednost)
                VALUES (@n,@u,@p,@sv,@nv)";

            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@n",  idNapake);
            cmd.Parameters.AddWithValue("@u",  idUporabnika);
            cmd.Parameters.AddWithValue("@p",  polje);
            cmd.Parameters.AddWithValue("@sv", staraVrednost ?? "");
            cmd.Parameters.AddWithValue("@nv", novaVrednost  ?? "");
            cmd.ExecuteNonQuery();
        }

        public List<HistoryItem> GetBugHistory(int idNapake)
        {
            var list = new List<HistoryItem>();
            const string sql = @"
                SELECT z.id_zgodovine, u.uporabnisko_ime,
                       z.polje, z.stara_vrednost, z.nova_vrednost,
                       DATE_FORMAT(z.datum_cas,'%d.%m.%Y %H:%i') AS datum_cas
                FROM   ZGODOVINA z
                JOIN   UPORABNIKI u ON z.id_uporabnika = u.id_uporabnika
                WHERE  z.id_napake = @id
                ORDER BY z.datum_cas DESC";

            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@id", idNapake);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new HistoryItem
                {
                    IdZgodovine    = r.GetInt32("id_zgodovine"),
                    UporabniskoIme = r.GetString("uporabnisko_ime"),
                    Polje          = r.GetString("polje"),
                    StaraVrednost  = r.IsDBNull(r.GetOrdinal("stara_vrednost")) ? "" : r.GetString("stara_vrednost"),
                    NovaVrednost   = r.IsDBNull(r.GetOrdinal("nova_vrednost"))  ? "" : r.GetString("nova_vrednost"),
                    DatumCas       = r.GetString("datum_cas")
                });
            return list;
        }

        // ══════════════════════════════════════════════════════════════
        //  UPORABNIKI
        // ══════════════════════════════════════════════════════════════

        public List<UserItem> GetAllUsers()
        {
            var list = new List<UserItem>();
            const string sql = @"
                SELECT id_uporabnika, uporabnisko_ime, ime, priimek, email, vloga, aktiven
                FROM   UPORABNIKI ORDER BY ime, priimek";

            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(sql, c);
            using var r   = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new UserItem
                {
                    IdUporabnika   = r.GetInt32("id_uporabnika"),
                    UporabniskoIme = r.GetString("uporabnisko_ime"),
                    Ime            = r.GetString("ime"),
                    Priimek        = r.GetString("priimek"),
                    Email          = r.GetString("email"),
                    Vloga          = r.GetString("vloga"),
                    Aktiven        = r.GetBoolean("aktiven")
                });
            return list;
        }

        public bool InsertUser(UserItem u, string geslo)
            => Register(u.UporabniskoIme, geslo, u.Ime, u.Priimek, u.Email);

        public bool UpdateUser(UserItem u)
        {
            const string sql = @"
                UPDATE UPORABNIKI
                SET ime=@ime, priimek=@pri, email=@email, vloga=@vloga
                WHERE id_uporabnika=@id";

            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(sql, c);
            cmd.Parameters.AddWithValue("@ime",   u.Ime);
            cmd.Parameters.AddWithValue("@pri",   u.Priimek);
            cmd.Parameters.AddWithValue("@email", u.Email);
            cmd.Parameters.AddWithValue("@vloga", u.Vloga);
            cmd.Parameters.AddWithValue("@id",    u.IdUporabnika);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool ToggleUserActive(int id)
        {
            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(
                "UPDATE UPORABNIKI SET aktiven = NOT aktiven WHERE id_uporabnika=@id", c);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool ResetPassword(int id, string novoGeslo)
        {
            string hash = BCrypt.Net.BCrypt.HashPassword(novoGeslo);
            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(
                "UPDATE UPORABNIKI SET geslo=@h WHERE id_uporabnika=@id", c);
            cmd.Parameters.AddWithValue("@h",  hash);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        // ══════════════════════════════════════════════════════════════
        //  KATEGORIJE
        // ══════════════════════════════════════════════════════════════

        public List<CategoryItem> GetAllCategories()
        {
            var list = new List<CategoryItem>();
            const string sql = @"
                SELECT k.id_kategorije, k.naziv, k.opis,
                       COUNT(n.id_napake) AS st_napak
                FROM   KATEGORIJE k
                LEFT JOIN NAPAKE n ON k.id_kategorije = n.id_kategorije
                GROUP BY k.id_kategorije ORDER BY k.naziv";

            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(sql, c);
            using var r   = cmd.ExecuteReader();
            while (r.Read())
                list.Add(new CategoryItem
                {
                    IdKategorije = r.GetInt32("id_kategorije"),
                    Naziv        = r.GetString("naziv"),
                    Opis         = r.IsDBNull(r.GetOrdinal("opis")) ? "" : r.GetString("opis"),
                    StNapak      = r.GetInt32("st_napak")
                });
            return list;
        }

        public bool InsertCategory(CategoryItem k)
        {
            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(
                "INSERT INTO KATEGORIJE (naziv, opis) VALUES (@n, @o)", c);
            cmd.Parameters.AddWithValue("@n", k.Naziv);
            cmd.Parameters.AddWithValue("@o", k.Opis ?? "");
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateCategory(CategoryItem k)
        {
            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(
                "UPDATE KATEGORIJE SET naziv=@n, opis=@o WHERE id_kategorije=@id", c);
            cmd.Parameters.AddWithValue("@n",  k.Naziv);
            cmd.Parameters.AddWithValue("@o",  k.Opis ?? "");
            cmd.Parameters.AddWithValue("@id", k.IdKategorije);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteCategory(int id)
        {
            using var c   = Conn(); c.Open();
            using var cmd = new MySqlCommand(
                "DELETE FROM KATEGORIJE WHERE id_kategorije=@id", c);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        // ══════════════════════════════════════════════════════════════
        //  STATISTIKA
        // ══════════════════════════════════════════════════════════════

        public StatsModel GetStats()
        {
            var s = new StatsModel();
            using var c = Conn(); c.Open();

            using (var cmd = new MySqlCommand(@"
                SELECT COUNT(*) AS skupaj,
                       SUM(status='Odprt')  AS odprtih,
                       SUM(status='V delu') AS v_delu,
                       SUM(status='Rešen')  AS resenih,
                       SUM(status='Zaprt')  AS zaprtih
                FROM NAPAKE", c))
            using (var r = cmd.ExecuteReader())
                if (r.Read())
                {
                    s.SkupajNapak = r.GetInt32("skupaj");
                    s.Odprtih     = r.GetInt32("odprtih");
                    s.VDelu       = r.GetInt32("v_delu");
                    s.Resenih     = r.GetInt32("resenih");
                    s.Zaprtih     = r.GetInt32("zaprtih");
                }

            using (var cmd = new MySqlCommand(
                "SELECT COUNT(*) AS skupaj, SUM(aktiven) AS aktivnih FROM UPORABNIKI", c))
            using (var r = cmd.ExecuteReader())
                if (r.Read())
                {
                    s.SkupajUsers   = r.GetInt32("skupaj");
                    s.AktivnihUsers = r.GetInt32("aktivnih");
                }

            return s;
        }

        // ══════════════════════════════════════════════════════════════
        //  HELPER
        // ══════════════════════════════════════════════════════════════

        private static BugItem MapBug(MySqlDataReader r) => new BugItem
        {
            IdNapake        = r.GetInt32("id_napake"),
            Naslov          = r.GetString("naslov"),
            Opis            = r.IsDBNull(r.GetOrdinal("opis"))            ? "" : r.GetString("opis"),
            Status          = r.GetString("status"),
            Prioriteta      = r.GetString("prioriteta"),
            IdKategorije    = r.IsDBNull(r.GetOrdinal("id_kategorije"))   ? null : r.GetInt32("id_kategorije"),
            Kategorija      = r.IsDBNull(r.GetOrdinal("kategorija"))      ? "–"  : r.GetString("kategorija"),
            IdUstvaritelja  = r.GetInt32("id_ustvaritelja"),
            Ustvaritelj     = r.IsDBNull(r.GetOrdinal("ustvaritelj"))     ? "–"  : r.GetString("ustvaritelj"),
            IdDodeljenega   = r.IsDBNull(r.GetOrdinal("id_dodeljenega"))  ? null : r.GetInt32("id_dodeljenega"),
            Dodeljen        = r.IsDBNull(r.GetOrdinal("dodeljen"))        ? "–"  : r.GetString("dodeljen"),
            DatumUstvarjeno = r.GetDateTime("datum_ustvarjeno"),
            DatumSpremenjeno= r.GetDateTime("datum_spremenjeno")
        };
    }
}
