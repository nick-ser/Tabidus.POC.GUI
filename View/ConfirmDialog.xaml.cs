using System.Windows;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    /// confirm dialog window that uses if we want to show a confirm dialog
    /// when use this, let change the confirm text
    /// </summary>
    public partial class ConfirmDialog : Window
    {
        public ConfirmDialog()
        {
            InitializeComponent();
        }

        public ConfirmDialog(string message, string caption) : this()
        {
            titlewindow.Text = caption;
            ConfirmText.Text = message;
        }
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.MouseLeftButtonDown += delegate { DragMove(); };
        }
    }
}