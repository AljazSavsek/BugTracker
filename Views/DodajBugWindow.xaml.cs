using System;
using System.Windows;
using System.Windows.Controls;
using BugTracker.Helpers;
using BugTracker.Models;

namespace BugTracker.Views
{
    public partial class DodajBugWindow : Window
    {
        private readonly DatabaseHelper _db = new();

        public DodajBugWindow() { InitializeComponent(); Loaded += (_, _) => LoadLists(); }

        private void LoadLists()
        {
            CmbKat.Items.Clear();
            CmbKat.Items.Add(new ComboBoxItem { Content = "– brez –", Tag = null });
            foreach (var k in _db.GetAllCategories())
                CmbKat.Items.Add(new ComboBoxItem { Content = k.Naziv, Tag = k.IdKategorije });
            CmbKat.SelectedIndex = 0;

            CmbDodeli.Items.Clear();
            CmbDodeli.Items.Add(new ComboBoxItem { Content = "– nihče –", Tag = null });
            foreach (var u in _db.GetAllUsers())
                if (u.Aktiven)
                    CmbDodeli.Items.Add(new ComboBoxItem
                        { Content = $"{u.ImeInPriimek} ({u.UporabniskoIme})", Tag = u.IdUporabnika });
            CmbDodeli.SelectedIndex = 0;
        }

        private void BtnShrani_Click(object sender, RoutedEventArgs e)
        {
            string naslov = TxtNaslov.Text.Trim();
            if (string.IsNullOrEmpty(naslov))
            {
                MessageBox.Show("Naslov napake je obvezen.", "Validacija",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var bug = new BugItem
            {
                Naslov         = naslov,
                Opis           = TxtOpis.Text.Trim(),
                Status         = (CmbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Odprt",
                Prioriteta     = (CmbPrio.SelectedItem   as ComboBoxItem)?.Content?.ToString() ?? "Srednja",
                IdKategorije   = (CmbKat.SelectedItem    as ComboBoxItem)?.Tag as int?,
                IdUstvaritelja = SessionManager.UserId,
                IdDodeljenega  = (CmbDodeli.SelectedItem as ComboBoxItem)?.Tag as int?
            };

            try
            {
                int id = _db.InsertBug(bug);
                if (id > 0)
                {
                    _db.LogHistory(id, SessionManager.UserId, "ustvarjena", "", bug.Status);
                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Napaka:\n" + ex.Message, "Napaka",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPreklici_Click(object sender, RoutedEventArgs e)
        { DialogResult = false; Close(); }
    }
}
