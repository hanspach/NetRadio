using System;
using System.Windows.Controls;

namespace NetRadio
{
    public partial class BrowserView : UserControl
    {
        public BrowserView()
        {
            InitializeComponent();
            txtUrl.KeyDown += delegate(object sender, System.Windows.Input.KeyEventArgs e)
            {
                try
                {
                    if (e.Key == System.Windows.Input.Key.Return)
                    {
                        string s = txtUrl.Text;
                        if (!s.StartsWith("http://") && !s.StartsWith("https://"))
                            s = "https://" + s;
                        if (!s.Contains("www."))
                        {
                            int idx = s.IndexOf("//");
                            s.Insert(idx + 2, "www.");
                        }
                        browser.Navigate(s);
                    }
                }
                catch(System.Security.SecurityException se)
                {
                    Console.WriteLine(se.Message);
                }
            };
        }

        private void RedoClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if(browser.CanGoBack)
                browser.GoBack();
        }

        private void RefreshClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            browser.Refresh();
        }
    }
}
