using System.Windows;
using System.Windows.Media;
using BugTracker.Helpers;

namespace BugTracker.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly DatabaseHelper _db = new();
        public RegisterWindow() => InitializeComponent();

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            MsgBorder.Visibility = Visibility.Collapsed;
            string ime  = TxtIme.Text.Trim(),    pri  = TxtPriimek.Text.Trim();
            string mail = TxtEmail.Text.Trim(),   usr  = TxtUser.Text.Trim();
            string pass = TxtPass.Password,        conf = TxtConf.Password;

            if (string.IsNullOrEmpty(ime) || string.IsNullOrEmpty(pri) ||
                string.IsNullOrEmpty(mail)|| string.IsNullOrEmpty(usr) ||
                string.IsNullOrEmpty(pass))
            { ShowMsg("Izpolnite vsa obvezna polja.", true); return; }

            if (pass != conf)
            { ShowMsg("Gesli se ne ujemata.", true); TxtPass.Clear(); TxtConf.Clear(); return; }

            if (pass.Length < 6)
            { ShowMsg("Geslo mora imeti vsaj 6 znakov.", true); return; }

            if (_db.Register(usr, pass, ime, pri, mail))
            {
                ShowMsg($"Račun '{usr}' ustvarjen! Sedaj se prijavite.", false);
                var t = new System.Windows.Threading.DispatcherTimer
                    { Interval = System.TimeSpan.FromSeconds(1.5) };
                t.Tick += (_, _) => { t.Stop(); Close(); };
                t.Start();
            }
            else ShowMsg("Uporabniško ime ali e-mail je že zaseden.", true);
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e) => Close();

        private void ShowMsg(string msg, bool err)
        {
            MsgText.Text = msg;
            MsgBorder.Background = new SolidColorBrush(err
                ? (Color)ColorConverter.ConvertFromString("#FDECEA")
                : (Color)ColorConverter.ConvertFromString("#EAFAF1"));
            MsgText.Foreground = err
                ? (Brush)Application.Current.Resources["DangerBrush"]
                : (Brush)Application.Current.Resources["SuccessBrush"];
            MsgBorder.Visibility = Visibility.Visible;
        }
    }
}
