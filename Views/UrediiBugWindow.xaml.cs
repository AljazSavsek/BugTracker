using System;
using System.Windows;
using System.Windows.Controls;
using BugTracker.Helpers;
using BugTracker.Models;

namespace BugTracker.Views
{
    public partial class UrediiBugWindow : Window
    {
        private readonly DatabaseHelper _db;
        private readonly int            _id;
        private BugItem                 _orig;

        public UrediiBugWindow(int id)
        {
            InitializeComponent();
            _db = new DatabaseHelper();
            _id = id;
            Loaded += (_, _) => Load();
        }

        private void Load()
        {
            try
            {
                _orig = _db.GetBugById(_id);
                if (_orig == null) { MessageBox.Show("Napaka ni bila najdena."); Close(); return; }

                TxtTitle.Text  = $"Uredi napako  #{_id}";
                TxtNaslov.Text = _orig.Naslov;
                TxtOpis.Text   = _orig.Opis;

                // Kategorije
                CmbKat.Items.Clear();
                CmbKat.Items.Add(new ComboBoxItem { Content = "– brez –", Tag = null });
                foreach (var k in _db.GetAllCategories())
                {
                    var ci = new ComboBoxItem { Content = k.Naziv, Tag = k.IdKategorije };
                    CmbKat.Items.Add(ci);
                    if (k.IdKategorije == _orig.IdKategorije) ci.IsSelected = true;
                }
                if (CmbKat.SelectedIndex < 0) CmbKat.SelectedIndex = 0;

                // Uporabniki
                CmbDodeli.Items.Clear();
                CmbDodeli.Items.Add(new ComboBoxItem { Content = "– nihče –", Tag = null });
                foreach (var u in _db.GetAllUsers())
                {
                    if (!u.Aktiven) continue;
                    var ci = new ComboBoxItem
                        { Content = $"{u.ImeInPriimek} ({u.UporabniskoIme})", Tag = u.IdUporabnika };
                    CmbDodeli.Items.Add(ci);
                    if (u.IdUporabnika == _orig.IdDodeljenega) ci.IsSelected = true;
                }
                if (CmbDodeli.SelectedIndex < 0) CmbDodeli.SelectedIndex = 0;

                SetCombo(CmbStatus, _orig.Status);
                SetCombo(CmbPrio,   _orig.Prioriteta);

                ListHistory.Items.Clear();
                foreach (var h in _db.GetBugHistory(_id))
                    ListHistory.Items.Add(h.Opis);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Napaka pri nalaganju:\n" + ex.Message);
            }
        }

        private void BtnShrani_Click(object sender, RoutedEventArgs e)
        {
            string naslov = TxtNaslov.Text.Trim();
            if (string.IsNullOrEmpty(naslov))
            {
                MessageBox.Show("Naslov je obvezen.", "Validacija",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var nova = new BugItem
            {
                IdNapake       = _id,
                Naslov         = naslov,
                Opis           = TxtOpis.Text.Trim(),
                Status         = Val(CmbStatus) ?? _orig.Status,
                Prioriteta     = Val(CmbPrio)   ?? _orig.Prioriteta,
                IdKategorije   = (CmbKat.SelectedItem    as ComboBoxItem)?.Tag as int?,
                Kategorija     = (CmbKat.SelectedItem    as ComboBoxItem)?.Content?.ToString(),
                IdDodeljenega  = (CmbDodeli.SelectedItem as ComboBoxItem)?.Tag as int?,
                Dodeljen       = (CmbDodeli.SelectedItem as ComboBoxItem)?.Content?.ToString(),
                IdUstvaritelja = _orig.IdUstvaritelja
            };

            try   { _db.UpdateBug(nova, _orig); DialogResult = true; Close(); }
            catch (Exception ex)
            { MessageBox.Show("Napaka:\n" + ex.Message, "Napaka",
                MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void BtnIzbrisi_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Izbriši napako #{_id}? Tega ni mogoče razveljaviti.",
                    "Brisanje", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                == MessageBoxResult.Yes)
            { _db.DeleteBug(_id); DialogResult = true; Close(); }
        }

        private void BtnPreklici_Click(object sender, RoutedEventArgs e)
        { DialogResult = false; Close(); }

        private static string Val(ComboBox c)
            => (c.SelectedItem as ComboBoxItem)?.Content?.ToString();

        private static void SetCombo(ComboBox c, string value)
        {
            foreach (ComboBoxItem ci in c.Items)
                if (ci.Content?.ToString() == value) { ci.IsSelected = true; return; }
            if (c.Items.Count > 0) c.SelectedIndex = 0;
        }
    }
}
