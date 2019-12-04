using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.ViewModel.DirectoryAssignment;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for MoveLdapDialog.xaml
    /// </summary>
    public partial class ShowLdapDirectoryDialog
    {
        public ShowLdapDirectoryDialog(LDAPAssignmentViewModel ldapAssignmentViewModel)
        {
            InitializeComponent();
            Model = new ShowLDAPDirectoryViewModel(this, ldapAssignmentViewModel);
            ApplicationContext.SelectedTargetNodes = new List<DirectoryNode>();
        }

        public ShowLDAPDirectoryViewModel Model
        {
            get { return DataContext as ShowLDAPDirectoryViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        ///     Closes the move dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            HideWindow();
        }

        private void DataTree_OnSelectedNodesCollectionChanged(object sender, NodeSelectionEventArgs e)
        {
            ApplicationContext.SelectedTargetNodes = new List<DirectoryNode>();
            if (e.CurrentSelectedNodes.Count >= 1)
            {
                foreach (var cnode in e.CurrentSelectedNodes)
                {
                    var node = cnode.Data as DirectoryNode;
                    if (node != null)
                    {
                        ApplicationContext.SelectedTargetNodes.Add(node);
                    }
                }
            }
            else
            {
                ApplicationContext.SelectedTargetNodes=new List<DirectoryNode>();
            }
        }

        
        /// <summary>
        ///     Handles the OnInitializeNode event of the DataTreeImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="InitializeNodeEventArgs" /> instance containing the event data.</param>
        private void DataTree_OnInitializeNode(object sender, InitializeNodeEventArgs e)
        {
            var data = e.Node.Data as DirectoryNode;
            if (data != null &&
                data.GuidString == (DataTree.ItemsSource as ObservableCollection<DirectoryNode>).FirstOrDefault().GuidString)
            {
                e.Node.IsExpanded = true;
            }
            
            if (data != null)
            {
                if (Model.NodeIdExpand.Contains(data.GuidString))
                {
                    e.Node.IsExpanded = true;
                }
                if (Model.NodesSelected.Select(r => r.GuidString).Contains(data.GuidString))
                {
                    e.Node.IsSelected = true;
                }

            }
        }
    }
}