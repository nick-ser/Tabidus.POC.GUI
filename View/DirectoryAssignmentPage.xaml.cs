using System.Windows.Controls;
using Tabidus.POC.GUI.ViewModel.DirectoryAssignment;
using Tabidus.POC.GUI.ViewModel.Endpoint;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for DirectoryAssignmentPage.xaml
    /// </summary>
    public partial class DirectoryAssignmentPage : Page
    {
        public DirectoryAssignmentPage()
        {
            InitializeComponent();
            Model = new AssignmentViewModel(this);
            GroupHeaderElement.UpdateHeader(7, ApplicationContext.FolderPathName, ApplicationContext.TotalEndpoint);
        }

        public AssignmentViewModel Model
        {
            set { DataContext = value; }
        }

        public GroupHeaderViewModel HeaderViewModel
        {
            get { return GroupHeaderElement.Model; }
        }
    }
}