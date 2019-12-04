using System.Windows;
using Infragistics.Controls.Interactions;
using System.Windows.Input;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Endpoint;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    /// Interaction logic for ImportFolderComputerDialog.xaml
    /// </summary>
    public partial class ImportFolderComputerDialog : XamDialogWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportFolderComputerDialog"/> class.
        /// </summary>
        public ImportFolderComputerDialog()
        {
            InitializeComponent();

            Model = new ImportFolderComputerViewModel(this);
        }
        private ImportFolderComputerViewModel _model;
        public ImportFolderComputerViewModel Model
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
                DataContext = _model;
            }
        }
        /// <summary>
        /// Handles the KeyUp event of the FilePath control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void FilePath_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btnBrowse.Command.Execute(null);
            }
        }

        /// <summary>
        /// Shows the window.
        /// </summary>
        public void ShowWindow()
        {
            Visibility = Visibility.Visible;
            this.Show();
        }
    }
}
