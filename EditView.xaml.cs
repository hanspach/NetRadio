using System.ComponentModel;
using System.Windows.Controls;

namespace NetRadio
{
    public partial class EditView : UserControl
    {
        private BackgroundWorker worker = new BackgroundWorker();

        public EditView()
        {
            InitializeComponent();
        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var binding = ((TextBox)sender).GetBindingExpression(TextBox.TextProperty);
            binding.UpdateSource();
        }
    }
}
