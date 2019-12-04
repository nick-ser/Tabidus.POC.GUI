using System.Windows.Controls;
using Tabidus.POC.GUI.UserControls.Endpoint;
using Tabidus.POC.GUI.ViewModel.Endpoint;
using Tabidus.POC.GUI.ViewModel.Policy;

namespace Tabidus.POC.GUI.View
{
    public partial class PolicyEnhancementPage : Page
    {
        public PolicyEnhancementPage()
        {
            InitializeComponent();
            Model = new PolicyEnhancementPageViewModel(this);
            PolicyEnhancementHeaderElement.UpdateHeader(3, ApplicationContext.FolderPathName, ApplicationContext.TotalEndpoint);
        }

        public PolicyEnhancementPageViewModel Model
        {
            get { return DataContext as PolicyEnhancementPageViewModel; }
            set { DataContext = value; }
        }

        public GroupHeaderViewModel HeaderViewModel
        {
            get { return PolicyEnhancementHeaderElement.Model; }
        }
        public EndpointViewHeaderViewModel EndpointHeaderViewModel
        {
            get { return EndpointPolicyEnhancementHeaderElement.Model; }
        }
    }
}