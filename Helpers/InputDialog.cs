using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BugTracker.Helpers
{
    public static class InputDialog
    {
        public static string Show(string prompt, string title = "", string defaultValue = "")
        {
            string result = null;

            var win = new Window
            {
                Title  = title, Width = 420, Height = 160,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = (System.Windows.Media.Brush)
                    Application.Current.Resources["LightBrush"]
            };

            var panel = new StackPanel { Margin = new Thickness(20) };
            panel.Children.Add(new TextBlock
            {
                Text       = prompt, FontSize = 13,
                Foreground = (System.Windows.Media.Brush)
                    Application.Current.Resources["TextBrush"],
                Margin = new Thickness(0, 0, 0, 8)
            });

            var tb = new TextBox
            {
                Text   = defaultValue,
                Style  = (Style)Application.Current.Resources["InputBox"],
                Margin = new Thickness(0, 0, 0, 14)
            };
            panel.Children.Add(tb);

            var row = new Grid();
            row.ColumnDefinitions.Add(new ColumnDefinition());
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10) });
            row.ColumnDefinitions.Add(new ColumnDefinition());

            var ok  = new Button { Content = "V redu",   Style = (Style)Application.Current.Resources["PrimaryButton"],   Padding = new Thickness(0, 8, 0, 8) };
            var can = new Button { Content = "Prekliči", Style = (Style)Application.Current.Resources["SecondaryButton"], Padding = new Thickness(0, 8, 0, 8) };

            ok.Click  += (_, _) => { result = tb.Text; win.Close(); };
            can.Click += (_, _) => win.Close();

            tb.KeyDown += (_, e) =>
            {
                if (e.Key == Key.Enter)  { result = tb.Text; win.Close(); }
                if (e.Key == Key.Escape) win.Close();
            };

            Grid.SetColumn(ok,  0); Grid.SetColumn(can, 2);
            row.Children.Add(ok); row.Children.Add(can);
            panel.Children.Add(row);

            win.Content = panel;
            win.Loaded += (_, _) => { tb.Focus(); tb.SelectAll(); };
            win.ShowDialog();
            return result;
        }
    }
}
