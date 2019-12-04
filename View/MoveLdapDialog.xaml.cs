using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.ViewModel.Discovery;
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
    public partial class MoveLdapDialog
    {
        public MoveLdapDialog()
        {
            InitializeComponent();
            Model = new MoveLDAPViewModel(this);
        }

        public MoveLDAPViewModel Model
        {
            get { return DataContext as MoveLDAPViewModel; }
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
            if (e.CurrentSelectedNodes.Count == 1)
            {
                var node = e.CurrentSelectedNodes[0].Data as DirectoryNode;
                if (node != null)
                {
                    ApplicationContext.NodeTargetId = node.NodeId;
                }
            }
        }

        private void MoveTargetDialog_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool) e.NewValue)
            {
                DataTree.SelectedDataItems = null;
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

        private void MoveLdapDialog_OnLoaded(object sender, RoutedEventArgs e)
        {
            Border header = Utilities.GetDescendantFromName(sender as DependencyObject, "HeaderArea") as Border;
            header.Visibility = System.Windows.Visibility.Collapsed;
            Grid borderGrid = Utilities.GetDescendantFromName(sender as DependencyObject, "BorderGrid") as Grid;
            borderGrid.Visibility = Visibility.Collapsed;
        }
    }
}