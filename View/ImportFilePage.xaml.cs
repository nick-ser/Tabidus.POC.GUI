using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Endpoint;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    /// Interaction logic for ImportFilePage.xaml
    /// </summary>
    public partial class ImportFilePage : Page
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportFilePage"/> class.
        /// </summary>
        /// <param name="folderId">The folder identifier.</param>
        public ImportFilePage(int folderId)
        {
            InitializeComponent();
            Model = new ImportFilePageViewModel(this, folderId);
        }

        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <value>The model.</value>
        public ImportFilePageViewModel Model
        {
            set
            {
                DataContext = value;
            }
        }

        /// <summary>
        /// Expands all tree data.
        /// </summary>
        public void ExpandAllTreeData()
        {
            SetNodeExpandedState(DataTreeImport.Nodes, true);
        }
        /// <summary>
        /// Sets the state of the node expanded.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="expandNode">if set to <c>true</c> [expand node].</param>
        private void SetNodeExpandedState(IEnumerable<XamDataTreeNode> nodes, bool expandNode)
        {
            foreach (XamDataTreeNode item in nodes)
            {
                item.IsExpanded = expandNode;
                this.SetNodeExpandedState(item.Nodes, expandNode);
            }
        }

        /// <summary>
        /// Handles the OnInitializeNode event of the DataTreeImport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="InitializeNodeEventArgs"/> instance containing the event data.</param>
        private void DataTreeImport_OnInitializeNode(object sender, InitializeNodeEventArgs e)
        {
            var data = e.Node.Data as DirectoryNode;
            if (data != null && data.NodeId==(DataTreeImport.ItemsSource as ObservableCollection<DirectoryNode>).FirstOrDefault().NodeId)
            {
                e.Node.IsExpanded = true;
            }
            
        }
    }
}
