using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.ViewModel.DirectoryAssignment;
using Tabidus.POC.GUI.ViewModel.Policy;
using System;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Infragistics.Windows;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for MoveLdapDialog.xaml
    /// </summary>
    public partial class EndpointDirectoryDialog
    {
        public EndpointDirectoryDialog()
        {
            InitializeComponent();
            Model = new EndpointDirectoryDialogViewModel(this);
            ApplicationContext.SelectedTargetNodes = new List<DirectoryNode>();
        }

        public EndpointDirectoryDialogViewModel Model
        {
            get { return DataContext as EndpointDirectoryDialogViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        ///     Closes the move dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        public bool? DialogResult { get; set; }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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
                data.NodeId == (DataTree.ItemsSource as ObservableCollection<DirectoryNode>).FirstOrDefault().NodeId)
            {
                e.Node.IsExpanded = true;
            }
            
        }

        private void EndpointDirectoryDialog_OnLoaded(object sender, RoutedEventArgs e)
        {

            Border header = Utilities.GetDescendantFromName(sender as DependencyObject, "HeaderArea") as Border;
            header.Visibility = System.Windows.Visibility.Collapsed;
            Grid borderGrid = Utilities.GetDescendantFromName(sender as DependencyObject, "BorderGrid") as Grid;
            borderGrid.Visibility = Visibility.Collapsed;
        }
    }
}