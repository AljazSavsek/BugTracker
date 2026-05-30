using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using BugTracker.Helpers;
using BugTracker.Models;
using MySql.Data.MySqlClient;

namespace BugTracker.Views
{
    public partial class Dashboard : Window
    {
        private readonly DatabaseHelper _db  = new();
        private List<BugItem>           _all = new();

        public Dashboard()
        {
            InitializeComponent();

            // Instead of calling Filter(); directly here, wait for the window to load:
            this.Loaded += (s, e) => Filter();
        }

        private void Init()
        {
            TxtUser.Text  = SessionManager.Username;
            TxtVloga.Text = SessionManager.Vloga;
            MnuAdmin.Visibility = SessionManager.IsAdmin
                ? Visibility.Visible : Visibility.Collapsed;
            Reload();
        }

        // ── Nalaganje ────────────────────────────────────────────────────
        private void Reload()
        {
            try
            {
                _all = _db.GetAllBugs();
                var s = _db.GetStats();
                NumOdprt.Text = s.Odprtih.ToString();
                NumVDelu.Text = s.VDelu.ToString();
                NumResen.Text = s.Resenih.ToString();
                Filter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Napaka pri nalaganju:\n" + ex.Message, "Napaka",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Filter ───────────────────────────────────────────────────────
        private void Filter()
        {
            if (_all is null) return;
            string q  = TxtSearch?.Text?.ToLower() ?? "";
            string st = (CmbStatus?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
            string pr = (CmbPrio?.SelectedItem   as ComboBoxItem)?.Content?.ToString() ?? "";

            var res = _all.Where(b =>
                (q  == "" || b.Naslov.ToLower().Contains(q) || b.Kategorija.ToLower().Contains(q)) &&
                (st == "" || st == "Vsi statusi"    || b.Status    == st) &&
                (pr == "" || pr == "Vse prioritete" || b.Prioriteta == pr)
            ).ToList();
            // 1. Safety Check: If the UI grid isn't loaded yet, stop immediately.
            // 1. Safety Check: If the UI grid isn't loaded yet, stop immediately.
            if (BugGrid == null)
            {
                return;
            }

            // 2. Safety Check: If your data list is null, create a generic empty list.
            if (res == null)
            {
                BugGrid.ItemsSource = new List<object>(); // Safe empty list
                return; // Exit early since there's nothing to filter
            }

            // 3. If it is not null, safely assign it to your Grid
            BugGrid.ItemsSource = null; // Reset the state
            BugGrid.ItemsSource = res;
        }

        // ── Event handleri za filtre (ločena imena – nič overloadov) ─────
        private void OnFilterChanged(object s, TextChangedEventArgs e)      => Filter();
        private void OnComboFilterChanged(object s, SelectionChangedEventArgs e) => Filter();

        // ── Dvojni klik ──────────────────────────────────────────────────
        private void Grid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (BugGrid.SelectedItem is BugItem b)
            {
                var w = new UrediiBugWindow(b.IdNapake) { Owner = this };
                if (w.ShowDialog() == true) Reload();
            }
        }

        // ── Sidebar ──────────────────────────────────────────────────────
        private void MnuVse_Click(object s, MouseButtonEventArgs e)
        {
            SetActive(MnuVse); Reload();
        }

        private void MnuDodaj_Click(object s, MouseButtonEventArgs e)
        {
            var w = new DodajBugWindow { Owner = this };
            if (w.ShowDialog() == true) { SetActive(MnuVse); Reload(); }
        }

        private void MnuAdmin_Click(object s, MouseButtonEventArgs e)
        {
            new AdminPanelWindow { Owner = this }.ShowDialog();
            Reload();
        }

        private void SetActive(Border active)
        {
            foreach (var m in new[] { MnuVse, MnuDodaj, MnuAdmin })
                m.Background = m == active
                    ? (System.Windows.Media.Brush)Application.Current.Resources["AccentBrush"]
                    : System.Windows.Media.Brushes.Transparent;
        }

        // ── Gumbi ────────────────────────────────────────────────────────
        private void BtnNova_Click(object s, RoutedEventArgs e)
            => MnuDodaj_Click(s, null);

        private void BtnRefresh_Click(object s, RoutedEventArgs e) => Reload();

        private void BtnExcel_Click(object s, RoutedEventArgs e)
        {
            try
            {
                var list = (BugGrid.ItemsSource as IEnumerable<BugItem>)?.ToList()
                           ?? new List<BugItem>();
                string path = ExcelHelper.Export(list);
                var r = MessageBox.Show(
                    $"Shranjeno: {System.IO.Path.GetFileName(path)}\n\nOdpri datoteko?",
                    "Izvoz uspešen", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (r == MessageBoxResult.Yes)
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Napaka pri izvozu:\n" + ex.Message,
                    "Napaka", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Izvozi TXT poročilo ──────────────────────────────────────────
        private void BtnExportTxt_Click(object s, RoutedEventArgs e)
        {
            try
            {
                var list = (BugGrid.ItemsSource as IEnumerable<BugItem>)?.ToList()
                           ?? new List<BugItem>();

                if (list.Count == 0)
                {
                    MessageBox.Show("Ni napak za izvoz. Preverite filtre.",
                        "Opozorilo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Odpri SaveFileDialog za izbiro lokacije
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title            = "Shrani poročilo o napakah",
                    Filter           = "Besedilna datoteka (*.txt)|*.txt",
                    DefaultExt       = ".txt",
                    FileName         = $"BugTracker_Porocilo_{DateTime.Now:yyyyMMdd_HHmm}.txt",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (dlg.ShowDialog() != true) return;

                string path = TxtHelper.Export(list, dlg.FileName);

                var r = MessageBox.Show(
                    $"Poročilo shranjeno:\n{System.IO.Path.GetFileName(path)}\n\n" +
                    $"Datoteka vsebuje {list.Count} napak z opombami za ročno dopolnitev.\n\n" +
                    "Odpri datoteko?",
                    "Izvoz uspešen", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (r == MessageBoxResult.Yes)
                    Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Napaka pri izvozu:\n" + ex.Message,
                    "Napaka", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Uvozi TXT ────────────────────────────────────────────────────
        private void BtnImportTxt_Click(object s, RoutedEventArgs e)
        {
            // ── Opcija: ustvari vzorec ────────────────────────────────
            var choice = MessageBox.Show(
                "Kako želite nadaljevati?\n\n" +
                "DA  → Ustvari vzorec TXT datoteke (jo odpre za urejanje)\n" +
                "NE  → Uvozi obstoječo TXT datoteko",
                "Uvozi TXT",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (choice == MessageBoxResult.Cancel) return;

            if (choice == MessageBoxResult.Yes)
            {
                // Ustvari vzorec
                var saveDlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title      = "Shrani vzorec za uvoz",
                    Filter     = "Besedilna datoteka (*.txt)|*.txt",
                    FileName   = "BugTracker_Uvoz_Vzorec.txt",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };
                if (saveDlg.ShowDialog() != true) return;

                System.IO.File.WriteAllText(
                    saveDlg.FileName,
                    TxtHelper.SampleImportContent(),
                    System.Text.Encoding.UTF8);

                MessageBox.Show(
                    $"Vzorec shranjen:\n{saveDlg.FileName}\n\n" +
                    "Uredite datoteko, nato znova pritisnite 'Uvozi TXT' in izberite NE.",
                    "Vzorec ustvarjen", MessageBoxButton.OK, MessageBoxImage.Information);

                Process.Start(new ProcessStartInfo(saveDlg.FileName) { UseShellExecute = true });
                return;
            }

            // ── Uvozi obstoječo datoteko ──────────────────────────────
            var openDlg = new Microsoft.Win32.OpenFileDialog
            {
                Title            = "Izberi TXT datoteko za uvoz",
                Filter           = "Besedilna datoteka (*.txt)|*.txt",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (openDlg.ShowDialog() != true) return;

            try
            {
                var (bugs, napake) = TxtHelper.Import(openDlg.FileName);

                if (bugs.Count == 0)
                {
                    string errMsg = napake.Count > 0
                        ? "Napake pri razčlenjevanju:\n" + string.Join("\n", napake)
                        : "V datoteki ni bilo najdenih napak.\nPreveri format (NASLOV: ..., ---)";
                    MessageBox.Show(errMsg, "Uvoz ni uspel",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Potrditev
                var confirm = MessageBox.Show(
                    $"Najdenih {bugs.Count} napak za uvoz.\n\n" +
                    (napake.Count > 0 ? $"Opozorila ({napake.Count}):\n{string.Join("\n", napake)}\n\n" : "") +
                    "Uvozi v bazo?",
                    "Potrditev uvoza",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);
                if (confirm != MessageBoxResult.Yes) return;

                // Shrani v bazo
                int uspeh = 0, neuspeh = 0;
                foreach (var bug in bugs)
                {
                    // Nastavi ustvaritelja na trenutnega uporabnika
                    bug.IdUstvaritelja = SessionManager.UserId;

                    // Poišči kategorijo po imenu
                    if (!string.IsNullOrEmpty(bug.Kategorija))
                    {
                        var cats = _db.GetAllCategories();
                        var kat  = cats.Find(k => string.Equals(
                            k.Naziv, bug.Kategorija, StringComparison.OrdinalIgnoreCase));
                        bug.IdKategorije = kat?.IdKategorije;
                    }

                    // Poišči dodeljenega uporabnika po username
                    if (!string.IsNullOrEmpty(bug.Dodeljen))
                    {
                        var users = _db.GetAllUsers();
                        var user  = users.Find(u => string.Equals(
                            u.UporabniskoIme, bug.Dodeljen,
                            StringComparison.OrdinalIgnoreCase));
                        bug.IdDodeljenega = user?.IdUporabnika;
                    }

                    try
                    {
                        int id = _db.InsertBug(bug);
                        if (id > 0)
                        {
                            _db.LogHistory(id, SessionManager.UserId,
                                "uvožena", "", $"Uvoz iz TXT: {bug.Status}");
                            uspeh++;
                        }
                        else neuspeh++;
                    }
                    catch { neuspeh++; }
                }

                MessageBox.Show(
                    $"Uvoz zaključen:\n\n" +
                    $"✅ Uvoženih:    {uspeh}\n" +
                    $"❌ Neuspešnih:  {neuspeh}",
                    "Uvoz končan", MessageBoxButton.OK,
                    uspeh > 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);

                Reload();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Napaka pri uvozu:\n" + ex.Message,
                    "Napaka", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ── Odjava ───────────────────────────────────────────────────────
        private void Logout_Click(object s, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Ali se res želite odjaviti?", "Odjava",
                    MessageBoxButton.YesNo, MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                SessionManager.Clear();
                new LoginWindow().Show();
                Close();
            }
        }
    }
}
