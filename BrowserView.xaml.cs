using System;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NetRadio
{
    public partial class BrowserView : UserControl
    {
        public BrowserView()
        {
            InitializeComponent();
        }

        private void RedoClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            browser.GoBack();
        }

        private void RefreshClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            browser.Refresh();
        }
    }
}
