using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BugTracker.Helpers;
using BugTracker.Models;

namespace BugTracker.Views
{
    public partial class AdminPanelWindow : Window
    {
        private readonly DatabaseHelper _db = new();

        public AdminPanelWindow()
        {
            InitializeComponent();
            Loaded += (_, _) => ShowTab("U");
        }

        // ── Tab ──────────────────────────────────────────────────────────
        private void Tab_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border b) ShowTab(b.Tag?.ToString());
        }

        private void ShowTab(string t)
        {
            PanelU.Visibility = t == "U" ? Visibility.Visible : Visibility.Collapsed;
            PanelK.Visibility = t == "K" ? Visibility.Visible : Visibility.Collapsed;
            PanelS.Visibility = t == "S" ? Visibility.Visible : Visibility.Collapsed;
            PanelN.Visibility = t == "N" ? Visibility.Visible : Visibility.Collapsed;

            foreach (var tab in new[] { TabU, TabK, TabS, TabN })
            {
                bool active = tab.Tag?.ToString() == t;
                tab.Background = active
                    ? (Brush)Application.Current.Resources["AccentBrush"]
                    : Brushes.Transparent;
                if (tab.Child is TextBlock tb)
                {
                    tb.Foreground = active ? Brushes.White
                        : (Brush)Application.Current.Resources["MutedBrush"];
                    tb.FontWeight = active ? FontWeights.SemiBold : FontWeights.Normal;
                }
            }

            switch (t)
            {
                case "U": LoadUsers(); break;
                case "K": LoadKat();   break;
                case "S": LoadStats(); break;
            }
        }

        // ── Uporabniki ───────────────────────────────────────────────────
        private void LoadUsers()
        {
            GridUsers.ItemsSource = null;
            GridUsers.ItemsSource = _db.GetAllUsers();
        }

        private void BtnDodajUser_Click(object s, RoutedEventArgs e)
        {
            var d = new DodajUserDialog { Owner = this };
            if (d.ShowDialog() == true) LoadUsers();
        }

        private void BtnUrediUser_Click(object s, RoutedEventArgs e)
        {
            if (!(GridUsers.SelectedItem is UserItem u))
            { MessageBox.Show("Izberite uporabnika."); return; }
            var d = new UrediUserDialog(u) { Owner = this };
            if (d.ShowDialog() == true) LoadUsers();
        }

        private void BtnResetPass_Click(object s, RoutedEventArgs e)
        {
            if (!(GridUsers.SelectedItem is UserItem u))
            { MessageBox.Show("Izberite uporabnika."); return; }
            string novo = InputDialog.Show($"Novo geslo za '{u.UporabniskoIme}':", "Reset gesla");
            if (string.IsNullOrWhiteSpace(novo)) return;
            if (novo.Length < 6) { MessageBox.Show("Geslo mora imeti vsaj 6 znakov."); return; }
            _db.ResetPassword(u.IdUporabnika, novo);
            MessageBox.Show("Geslo nastavljeno.", "Uspeh",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnToggle_Click(object s, RoutedEventArgs e)
        {
            if (!(GridUsers.SelectedItem is UserItem u))
            { MessageBox.Show("Izberite uporabnika."); return; }
            if (u.UporabniskoIme == SessionManager.Username)
            { MessageBox.Show("Ne morete spremeniti lastnega statusa."); return; }
            string dej = u.Aktiven ? "deaktivirate" : "aktivirate";
            if (MessageBox.Show($"Ali res želite {dej} '{u.UporabniskoIme}'?",
                    "Potrditev", MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            { _db.ToggleUserActive(u.IdUporabnika); LoadUsers(); }
        }

        // ── Kategorije ───────────────────────────────────────────────────
        private void LoadKat()
        {
            GridKat.ItemsSource = null;
            GridKat.ItemsSource = _db.GetAllCategories();
        }

        private void BtnDodajKat_Click(object s, RoutedEventArgs e)
        {
            string naziv = InputDialog.Show("Naziv kategorije:", "Dodaj kategorijo");
            if (string.IsNullOrWhiteSpace(naziv)) return;
            string opis = InputDialog.Show("Opis (neobvezno):", "Opis", "");
            _db.InsertCategory(new CategoryItem { Naziv = naziv.Trim(), Opis = opis?.Trim() ?? "" });
            LoadKat();
        }

        private void BtnUrediKat_Click(object s, RoutedEventArgs e)
        {
            if (!(GridKat.SelectedItem is CategoryItem k))
            { MessageBox.Show("Izberite kategorijo."); return; }
            string naziv = InputDialog.Show("Nov naziv:", "Uredi", k.Naziv);
            if (string.IsNullOrWhiteSpace(naziv)) return;
            string opis = InputDialog.Show("Nov opis:", "Opis", k.Opis ?? "");
            k.Naziv = naziv.Trim(); k.Opis = opis?.Trim() ?? "";
            _db.UpdateCategory(k); LoadKat();
        }

        private void BtnIzbrisiKat_Click(object s, RoutedEventArgs e)
        {
            if (!(GridKat.SelectedItem is CategoryItem k))
            { MessageBox.Show("Izberite kategorijo."); return; }
            if (k.StNapak > 0)
            { MessageBox.Show($"Kategorija ima {k.StNapak} napak – ni mogoče izbrisati."); return; }
            if (MessageBox.Show($"Izbriši '{k.Naziv}'?", "Potrditev",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            { _db.DeleteCategory(k.IdKategorije); LoadKat(); }
        }

        // ── Statistika ───────────────────────────────────────────────────
        private void LoadStats()
        {
            try
            {
                var s = _db.GetStats();
                SC_Tot.Text = s.SkupajNapak.ToString();
                SC_Odp.Text = s.Odprtih.ToString();
                SC_Del.Text = s.VDelu.ToString();
                SC_Res.Text = s.Resenih.ToString();
                SC_Zap.Text = s.Zaprtih.ToString();
                SC_Usr.Text = s.SkupajUsers.ToString();
                SC_Akt.Text = s.AktivnihUsers.ToString();
            }
            catch { }
        }

        // ── Nastavitve ───────────────────────────────────────────────────
        private void BtnSetPass_Click(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtNew.Password)) return;
            if (TxtNew.Password != TxtConf.Password)
            { MessageBox.Show("Gesli se ne ujemata."); return; }
            if (TxtNew.Password.Length < 6)
            { MessageBox.Show("Vsaj 6 znakov."); return; }
            _db.ResetPassword(SessionManager.UserId, TxtNew.Password);
            TxtNew.Clear(); TxtConf.Clear();
            MessageBox.Show("Geslo nastavljeno.", "Uspeh",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
