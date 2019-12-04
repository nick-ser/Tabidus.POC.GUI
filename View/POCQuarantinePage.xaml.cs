using System.Windows.Controls;
using Tabidus.POC.GUI.ViewModel.Reporting;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    /// Interaction logic for POCQuarantinePage.xaml
    /// </summary>
    public partial class POCQuarantinePage : Page
    {
        public POCQuarantinePage()
        {
            InitializeComponent();
            Model = new POCQuarantineViewModel(this);
        }

        public POCQuarantineViewModel Model
        {
            get { return DataContext as POCQuarantineViewModel; }
            set { DataContext = value; }
        }
    }
}
