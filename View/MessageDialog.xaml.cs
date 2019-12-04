using System.Windows;
using Infragistics.Controls.Interactions;
using Tabidus.POC.GUI.Common;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for ImportFolderComputerDialog.xaml
    /// </summary>
    public partial class MessageDialog : XamDialogWindow
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MessageDialog" /> class.
        /// </summary>
        public MessageDialog()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void ShowMessageDialog(string message, string caption, DialogState state = DialogState.Alert)
        {
            Header = caption;
            TxtMessageText.Text = message;
            Show();
            Visibility = Visibility.Visible;
            BtnOk.Focus();
        }
    }
}