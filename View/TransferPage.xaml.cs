using System.Windows.Controls;
using Tabidus.POC.GUI.ViewModel.Software;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for SoftwarePage.xaml
    /// </summary>
    public partial class TransferPage : Page
    {
        public TransferPage()
        {
            InitializeComponent();

            Model = new TransferViewModel(this);
        }

        public TransferViewModel Model
        {
            get { return DataContext as TransferViewModel; }
            set { DataContext = value; }
        }
    }
}