using System.Windows.Controls;

namespace NetRadio
{
    public partial class EditView : UserControl
    {
        public EditView()
        {
            InitializeComponent();
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ViewModels.Settings.IsDirty = true;
        }

        private void cbImages_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Console.WriteLine("MoseDown"); ViewModels.Settings.IsDirty = true;
        }
    }
}
