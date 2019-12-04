using System.Windows.Controls;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Endpoint;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for EndPointListPage.xaml
    /// </summary>
    public partial class EndPointViewPage : Page
    {
        public EndPointViewPage()
        {
            InitializeComponent();
            var endpointViewModel = new EndpointViewModel(this);
            DataContext = endpointViewModel;
        }
    }
}