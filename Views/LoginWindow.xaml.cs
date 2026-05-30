using System.Windows;
using System.Windows.Input;
using BugTracker.Helpers;

namespace BugTracker.Views
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseHelper _db = new();

        public LoginWindow()
        {
            InitializeComponent();
            TxtPass.KeyDown += (_, e) => { if (e.Key == Key.Enter) DoLogin(); };
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e) => DoLogin();

        private void DoLogin()
        {
            ErrBorder.Visibility = Visibility.Collapsed;
            string u = TxtUser.Text.Trim();
            string p = TxtPass.Password;

            if (string.IsNullOrEmpty(u) || string.IsNullOrEmpty(p))
            { ShowErr("Izpolnite vsa polja."); return; }

            if (_db.Login(u, p))
            { new Dashboard().Show(); Close(); }
            else
            { ShowErr("Napačno uporabniško ime ali geslo."); TxtPass.Clear(); }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
            => new RegisterWindow { Owner = this }.ShowDialog();

        private void ShowErr(string msg)
        { ErrText.Text = msg; ErrBorder.Visibility = Visibility.Visible; }
    }
}
