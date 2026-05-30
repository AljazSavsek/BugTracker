using System.Windows;
using System.Windows.Controls;
using BugTracker.Helpers;
using BugTracker.Models;

namespace BugTracker.Views
{
    // ══════════════════════════════════════════════════════════════════
    //  Dodaj novega uporabnika
    // ══════════════════════════════════════════════════════════════════
    public class DodajUserDialog : Window
    {
        private readonly DatabaseHelper _db = new();
        private TextBox     _ime, _priimek, _email, _user;
        private PasswordBox _pass, _conf;
        private ComboBox    _vloga;

        public DodajUserDialog()
        {
            Title  = "Dodaj uporabnika";
            Width  = 460; Height = 460;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Background = (System.Windows.Media.Brush)
                Application.Current.Resources["LightBrush"];

            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            var panel  = new StackPanel { Margin = new Thickness(28) };
            scroll.Content = panel;
            Content = scroll;

            // ── Ime + Priimek ─────────────────────────────────────────
            var row1 = new Grid(); 
            row1.ColumnDefinitions.Add(new ColumnDefinition());
            row1.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            row1.ColumnDefinitions.Add(new ColumnDefinition());
            row1.Margin = new Thickness(0, 0, 0, 12);

            var spIme     = new StackPanel();
            var spPriimek = new StackPanel();
            _ime     = AddField(spIme,     "Ime *",     "");
            _priimek = AddField(spPriimek, "Priimek *", "");
            Grid.SetColumn(spIme,     0);
            Grid.SetColumn(spPriimek, 2);
            row1.Children.Add(spIme);
            row1.Children.Add(spPriimek);
            panel.Children.Add(row1);

            // ── E-mail ────────────────────────────────────────────────
            _email = AddField(panel, "E-mail *", "");

            // Assigns the margin if it's a TextBox, otherwise assigns an empty Thickness(0)
            _ = panel.Children[panel.Children.Count - 1] is TextBox emTb
                ? (emTb.Margin = new Thickness(0, 0, 0, 12))
                : new Thickness(0);


            // ── Username ──────────────────────────────────────────────
            _user = AddField(panel, "Uporabniško ime *", "");

            // ── Geslo + potrditev ─────────────────────────────────────
            var row2 = new Grid();
            row2.ColumnDefinitions.Add(new ColumnDefinition());
            row2.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            row2.ColumnDefinitions.Add(new ColumnDefinition());
            row2.Margin = new Thickness(0, 0, 0, 12);

            var spPass = new StackPanel();
            var spConf = new StackPanel();
            Label(spPass, "Geslo *");
            Label(spConf, "Potrdi *");
            _pass = new PasswordBox { Style = (Style)Application.Current.Resources["InputPassword"] };
            _conf = new PasswordBox { Style = (Style)Application.Current.Resources["InputPassword"] };
            spPass.Children.Add(_pass);
            spConf.Children.Add(_conf);
            Grid.SetColumn(spPass, 0);
            Grid.SetColumn(spConf, 2);
            row2.Children.Add(spPass);
            row2.Children.Add(spConf);
            panel.Children.Add(row2);

            // ── Vloga ─────────────────────────────────────────────────
            Label(panel, "Vloga");
            _vloga = new ComboBox
            {
                Style  = (Style)Application.Current.Resources["InputCombo"],
                Margin = new Thickness(0, 0, 0, 20)
            };
            foreach (var v in new[] { "Developer", "Tester", "Admin" })
                _vloga.Items.Add(new ComboBoxItem { Content = v });
            _vloga.SelectedIndex = 0;
            panel.Children.Add(_vloga);

            // ── Gumbi ─────────────────────────────────────────────────
            var btnRow = new Grid();
            btnRow.ColumnDefinitions.Add(new ColumnDefinition());
            btnRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            btnRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

            var btnC = new Button
            {
                Content = "Prekliči",
                Style   = (Style)Application.Current.Resources["SecondaryButton"],
                Padding = new Thickness(0, 10, 0, 10)
            };
            var btnS = new Button
            {
                Content = "💾 Dodaj",
                Style   = (Style)Application.Current.Resources["PrimaryButton"],
                Padding = new Thickness(0, 10, 0, 10)
            };
            btnC.Click += (_, _) => { DialogResult = false; Close(); };
            btnS.Click += Save;
            Grid.SetColumn(btnC, 0); Grid.SetColumn(btnS, 2);
            btnRow.Children.Add(btnC);
            btnRow.Children.Add(btnS);
            panel.Children.Add(btnRow);
        }

        private void Save(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_ime.Text)    ||
                string.IsNullOrWhiteSpace(_priimek.Text)||
                string.IsNullOrWhiteSpace(_email.Text)  ||
                string.IsNullOrWhiteSpace(_user.Text)   ||
                string.IsNullOrEmpty(_pass.Password))
            { MessageBox.Show("Izpolnite vsa obvezna polja."); return; }

            if (_pass.Password != _conf.Password)
            { MessageBox.Show("Gesli se ne ujemata."); return; }

            if (_pass.Password.Length < 6)
            { MessageBox.Show("Geslo mora imeti vsaj 6 znakov."); return; }

            var u = new UserItem
            {
                UporabniskoIme = _user.Text.Trim(),
                Ime            = _ime.Text.Trim(),
                Priimek        = _priimek.Text.Trim(),
                Email          = _email.Text.Trim(),
                Vloga          = (_vloga.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Developer"
            };

            if (_db.InsertUser(u, _pass.Password))
            { DialogResult = true; Close(); }
            else
                MessageBox.Show("Uporabniško ime ali e-mail je že zaseden.");
        }

        // ── Pomožni metodi ────────────────────────────────────────────────
        private TextBox AddField(Panel parent, string lbl, string val)
        {
            Label(parent, lbl);
            var tb = new TextBox
            {
                Text   = val,
                Style  = (Style)Application.Current.Resources["InputBox"],
                Margin = new Thickness(0, 0, 0, 12)
            };
            parent.Children.Add(tb);
            return tb;
        }

        private static void Label(Panel p, string text)
            => p.Children.Add(new TextBlock
            {
                Text  = text,
                Style = (Style)Application.Current.Resources["FieldLabel"]
            });
    }

    // ══════════════════════════════════════════════════════════════════
    //  Uredi obstoječega uporabnika
    // ══════════════════════════════════════════════════════════════════
    public class UrediUserDialog : Window
    {
        private readonly DatabaseHelper _db = new();
        private readonly UserItem       _user;
        private TextBox  _ime, _priimek, _email;
        private ComboBox _vloga;

        public UrediUserDialog(UserItem user)
        {
            _user  = user;
            Title  = $"Uredi – {user.UporabniskoIme}";
            Width  = 420; Height = 340;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Background = (System.Windows.Media.Brush)
                Application.Current.Resources["LightBrush"];

            var panel = new StackPanel { Margin = new Thickness(28) };
            Content = panel;

            _ime     = Field(panel, "Ime *",     user.Ime);
            _priimek = Field(panel, "Priimek *", user.Priimek);
            _email   = Field(panel, "E-mail *",  user.Email);

            Lbl(panel, "Vloga");
            _vloga = new ComboBox
            {
                Style  = (Style)Application.Current.Resources["InputCombo"],
                Margin = new Thickness(0, 0, 0, 20)
            };
            foreach (var v in new[] { "Developer", "Tester", "Admin" })
            {
                var ci = new ComboBoxItem { Content = v };
                if (v == user.Vloga) ci.IsSelected = true;
                _vloga.Items.Add(ci);
            }
            panel.Children.Add(_vloga);

            var btnRow = new Grid();
            btnRow.ColumnDefinitions.Add(new ColumnDefinition());
            btnRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(12) });
            btnRow.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });

            var btnC = new Button
            {
                Content = "Prekliči",
                Style   = (Style)Application.Current.Resources["SecondaryButton"],
                Padding = new Thickness(0, 10, 0, 10)
            };
            var btnS = new Button
            {
                Content = "💾 Shrani",
                Style   = (Style)Application.Current.Resources["PrimaryButton"],
                Padding = new Thickness(0, 10, 0, 10)
            };
            btnC.Click += (_, _) => { DialogResult = false; Close(); };
            btnS.Click += Save;
            Grid.SetColumn(btnC, 0); Grid.SetColumn(btnS, 2);
            btnRow.Children.Add(btnC);
            btnRow.Children.Add(btnS);
            panel.Children.Add(btnRow);
        }

        private void Save(object s, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_ime.Text)    ||
                string.IsNullOrWhiteSpace(_priimek.Text)||
                string.IsNullOrWhiteSpace(_email.Text))
            { MessageBox.Show("Izpolnite vsa polja."); return; }

            _user.Ime     = _ime.Text.Trim();
            _user.Priimek = _priimek.Text.Trim();
            _user.Email   = _email.Text.Trim();
            _user.Vloga   = (_vloga.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? _user.Vloga;

            _db.UpdateUser(_user);
            DialogResult = true;
            Close();
        }

        private TextBox Field(Panel p, string lbl, string val)
        {
            Lbl(p, lbl);
            var tb = new TextBox
            {
                Text   = val,
                Style  = (Style)Application.Current.Resources["InputBox"],
                Margin = new Thickness(0, 0, 0, 14)
            };
            p.Children.Add(tb);
            return tb;
        }

        private static void Lbl(Panel p, string text)
            => p.Children.Add(new TextBlock
            {
                Text  = text,
                Style = (Style)Application.Current.Resources["FieldLabel"]
            });
    }
}
