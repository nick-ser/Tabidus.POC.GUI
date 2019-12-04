using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Infragistics.Controls.Menus;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.DataPresenter.Events;
using Newtonsoft.Json;
using Tabidus.POC.Common.Model.Discovery;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.ViewModel.Discovery;
using Tabidus.POC.GUI.ViewModel.Endpoint;
using Infragistics.DragDrop;
using Infragistics.Windows;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Editors;
using Tabidus.POC.Common.Constants;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for EndPointListPage.xaml
    /// </summary>
    public partial class NeighborhoodWatchPage : Page
    {
        // The CheckBoxes in the XamDataGrid execute this command when clicked.
        public static readonly RoutedCommand CheckboxCommand = new RoutedCommand();
        public static readonly string KeyEncryption = Functions.GetConfig("MESSAGE_KEY", "");

        public NeighborhoodWatchPage()
        {
            InitializeComponent();
            NeighborhoodWatchDataGrid.FieldLayouts.DataPresenter.GroupByAreaLocation = GroupByAreaLocation.None;
            Model = new NeighborhoodWatchViewModel(this);
        }

        public NeighborhoodWatchViewModel Model
        {
            get { return DataContext as NeighborhoodWatchViewModel; }
            set { DataContext = value; }
        }

        private void Field_Resized(object sender, SizeChangedEventArgs e)
        {
            var field = (sender as LabelPresenter).Field;
            if (field.Name == "Vendor")
            {
                if ((int)field.LabelWidthResolved != 200)
                    SetColumnWidth(field.Name, (int)field.LabelWidthResolved);
                else
                {
                    field.Width = new FieldLength(GetColumnWidth(field.Name));
                }
            }
            if (field.Name == "OperatingSystem")
            {
                if ((int)field.LabelWidthResolved != 250)
                    SetColumnWidth(field.Name, (int)field.LabelWidthResolved);
                else
                {
                    field.Width = new FieldLength(GetColumnWidth(field.Name));
                }
            }
            else if (field.Name == "LastDetected")
            {
                if ((int)field.LabelWidthResolved != 120)
                    SetColumnWidth(field.Name, (int)field.LabelWidthResolved);
                else
                {
                    field.Width = new FieldLength(GetColumnWidth(field.Name));
                }
            }
            else if (field.Name == "DetectedBy")
            {
                if ((int)field.LabelWidthResolved != 150)
                    SetColumnWidth(field.Name, (int)field.LabelWidthResolved);
                else
                {
                    field.Width = new FieldLength(GetColumnWidth(field.Name));
                }
            }
            else
            {
                if (field.Width != null)
                {
                    SetColumnWidth(field.Name, (int)field.LabelWidthResolved);
                }
            }
            
        }

        private void Field_Loaded(object sender, RoutedEventArgs e)
        {
            var field = (sender as LabelPresenter).Field;
            field.Width = new FieldLength(GetColumnWidth(field.Name));
        }

        private int GetColumnWidth(string id)
        {
            switch (id)
            {
                case "DetectedBy":
                    return ApplicationContext.NeDetectedByWidth;
                case "LastDetected":
                    return ApplicationContext.NeLastSeenWidth;
                case "OperatingSystem":
                    return ApplicationContext.NeOSNameWidth;
                case "IPv4":
                    return ApplicationContext.NeIPv4Width;
                case "IPv6":
                    return ApplicationContext.NeIPv6Width;
                case "Domain":
                    return ApplicationContext.NeDomainWidth;
                case "Vendor":
                    return ApplicationContext.NeVendorWidth;
                case "Computer":
                    return ApplicationContext.NeSystemNameWidth;
                case "MAC":
                    return ApplicationContext.NeMACWidth;
                case "Confirmed":
                    return ApplicationContext.NeConfirmedWidth;
                default:
                    return 100;
            }
        }

        private void SetColumnWidth(string id, int width)
        {
            switch (id)
            {
                case "DetectedBy":
                    ApplicationContext.NeDetectedByWidth = width;
                    break;
                case "LastDetected":
                    ApplicationContext.NeLastSeenWidth = width;
                    break;
                case "OperatingSystem":
                    ApplicationContext.NeOSNameWidth = width;
                    break;
                case "IPv4":
                    ApplicationContext.NeIPv4Width = width;
                    break;
                case "IPv6":
                    ApplicationContext.NeIPv6Width = width;
                    break;
                case "Domain":
                    ApplicationContext.NeDomainWidth = width;
                    break;
                case "Vendor":
                    ApplicationContext.NeVendorWidth = width;
                    break;
                case "Computer":
                    ApplicationContext.NeSystemNameWidth = width;
                    break;
                case "MAC":
                    ApplicationContext.NeMACWidth = width;
                    break;
                case "Confirmed":
                    ApplicationContext.NeConfirmedWidth = width;
                    break;
                default:
                    break;
            }
            var widthConfigText =
                string.Format(
                    "Confirmed:{0};SystemName:{1};Domain:{2};IPv4:{3};IPv6:{4};MAC:{5};Vendor:{6};OSName:{7};LastSeen:{8};DetectedBy:{9}",
                    ApplicationContext.NeConfirmedWidth, ApplicationContext.NeSystemNameWidth,
                    ApplicationContext.NeDomainWidth, ApplicationContext.NeIPv4Width, ApplicationContext.NeIPv6Width,
                    ApplicationContext.NeMACWidth, ApplicationContext.NeVendorWidth, ApplicationContext.NeOSNameWidth,
                    ApplicationContext.NeLastSeenWidth, ApplicationContext.NeDetectedByWidth);
            Functions.WriteToConfig("NEIGHBORHOOD_VIEW_WIDTH_KEY", widthConfigText);
        }

        private void CheckboxCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Parameter is NeighborhoodWatch;
        }

        private void CheckboxCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var neighbor = e.Parameter as NeighborhoodWatch;
            
            if (neighbor != null)
            {
                var confirmBkg = new BackgroundWorker();
                confirmBkg.DoWork += ConfirmBkg_DoWork;
                confirmBkg.RunWorkerAsync(neighbor);
                ApplicationContext.AllNeighborhoodWatch.Find(r => r.Id == neighbor.Id).Confirmed = neighbor.Confirmed;
                ApplicationContext.IsReload = true;
                Model.OnTabSelected();
                if (neighbor.Confirmed)
                {
                    ApplicationContext.EndPointListAll.RemoveAll(r => r.MACAddress == neighbor.MAC);
                    ApplicationContext.EndPointListTree.RemoveAll(r => r.MACAddress == neighbor.MAC);
                    ApplicationContext.IsReloadForRefresh = true;
                    Model.MakeTree(0);
                }
            }
        }

        private void ConfirmBkg_DoWork(object sender, DoWorkEventArgs e)
        {
            var neighbor = e.Argument as NeighborhoodWatch;
            AddOrRemoveConfirmed(neighbor);
        }

        private void AddOrRemoveConfirmed(NeighborhoodWatch cust)
        {
            using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            {
                var data = JsonConvert.SerializeObject(cust);
                sc.SaveNeighborhoodWatchConfirmed(EncryptionHelper.EncryptString(data, KeyEncryption));
            }
        }

        private void NeighborhoodWatchDataGrid_ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!ApplicationContext.IsReload)
            {
                ApplicationContext.NeighborhoodWatchViewVerticalOffset = NeighborhoodWatchDataGrid.ScrollInfo.VerticalOffset;
            }
            else
            {
                NeighborhoodWatchDataGrid.ScrollInfo.SetVerticalOffset(ApplicationContext.NeighborhoodWatchViewVerticalOffset);
                ApplicationContext.IsReload = false;
            }
        }
        
        private void DragSource_DragEnter(object sender, Infragistics.DragDrop.DragDropCancelEventArgs e)
        {
            //set drag data
            var xdt = e.DragSource as CardPanel;
            var cpr = xdt.TryFindParent<DataRecordPresenter>();
            var ep = cpr.DataRecord.DataItem as NeighborhoodWatch;

            if (ep == null)
            {
                e.Cancel = true;
            }
            else e.Data = ep;

            //change allow or not
            e.OperationType = OperationType.DropNotAllowed;
            if (e.DropTarget is XamDataTreeNodeControl)
            {
                XamDataTreeNodeControl drca = e.DropTarget as XamDataTreeNodeControl;
                var dtnode = drca.Node;
                if (dtnode != null)
                {
                    DirectoryNode dr = dtnode.Data as DirectoryNode;
                    if (dr.IsFolder)
                    {
                        e.OperationType = OperationType.Move;
                    }
                }
            }
        }

        private void EventSetter_OnHandler(object sender, MouseEventArgs e)
        {
            var xdt = sender as CellValuePresenter;
            var cpr = xdt.TryFindParent<DataRecordPresenter>();
            var ep = cpr.DataRecord.DataItem as NeighborhoodWatch;
            if (ep != null && ep.DetectedById > 0)
            {
                var leftModel = PageNavigatorHelper.GetLeftElementViewModel();
                var rightEle = PageNavigatorHelper._MainWindow.RightTreeElement;
                
                if (leftModel != null)
                {
                    if (!rightEle.Model.DirectoryPushed)
                    {
                        rightEle.OnDirectoryClick();
                    }
                    leftModel.ChangeEndpointNavigationState();
                    var enp = ApplicationContext.EndPointListAll.Find(r => r.EndpointId == ep.DetectedById);
                    var dir = ApplicationContext.FolderListAll.Find(r => r.FolderId == enp.FolderId);
                    var listNodes = new List<int>();
                    GetParentListId(listNodes, dir);
                    PageNavigatorHelper.GetRightElementViewModel().SelectTreeNode(ep.DetectedById, listNodes, TreeViewSelectMode.Endpoint);
                }
            }
        }

        private void GetParentListId(List<int> listNode, Directory dir)
        {
            if (dir == null) return;
            listNode.Add(dir.FolderId);
            foreach (var ep in ApplicationContext.FolderListAll)
            {
                if (dir.ParentId == ep.FolderId)
                {
                    GetParentListId(listNode, ep);
                    break;
                }
            }
        }

        private void DragSource_OnDragLeave(object sender, DragDropEventArgs e)
        {
            e.OperationType = OperationType.DropNotAllowed;
        }
        
        private void DragSource_Drop(object sender, Infragistics.DragDrop.DropEventArgs e)
        {
            var data = e.Data as NeighborhoodWatch;
            var drca = e.DropTarget as XamDataTreeNodeControl;
            if (drca != null)
            {
                var node = drca.Node;
                if (node != null)
                {
                    if (ApplicationContext.EndPointListAll.Select(r => r.MACAddress).Contains(data.MAC))
                    {
                        var messageDialog = PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                        messageDialog.ShowMessageDialog(
                            "Cannot move, this Endpoint is existed",
                            "Message");
                        return;
                    }
                    var ndata = node.Data as DirectoryNode;
                    
                    ApplicationContext.AllNeighborhoodWatch.Find(r => r.Id == data.Id).Managed = true;
                    
                    Model.OnTabSelected();
                    
                    var mainVM = PageNavigatorHelper.GetMainModel();
                    mainVM.AddComputer(ndata, data.Computer, data.MAC);
                }
            }
        }

        DataRecordPresenter dp;
        int currentIndex;
        private void DragSource_DragStart(object sender, Infragistics.DragDrop.DragDropStartEventArgs e)
        {
            dp = Utilities.GetAncestorFromType(e.DragSource as DependencyObject, typeof(DataRecordPresenter), false) as DataRecordPresenter;
            currentIndex = dp.Record.Index;
            var xdt = e.DragSource as CardPanel;
            var cpr = xdt.TryFindParent<DataRecordPresenter>();
            var ep = cpr.DataRecord.DataItem as NeighborhoodWatch;
            var dragSource = sender as DragSource;
            DataTemplate cardLayout = new DataTemplate();
            cardLayout.DataType = typeof(StackPanel);
            
            FrameworkElementFactory cardHolder = new FrameworkElementFactory(typeof(TextBlock));
            cardHolder.SetValue(TextBlock.TextProperty, string.IsNullOrWhiteSpace(ep.Computer) ? "New Computer" : ep.Computer);
            cardLayout.VisualTree = cardHolder;
            dragSource.DragTemplate = cardLayout;
        }

        private void CardPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        
        private void DragSource_DragOver(object sender, Infragistics.DragDrop.DragDropMoveEventArgs e)
        {
            if (e.DropTarget is XamDataTreeNodeControl)
            {
                XamDataTreeNodeControl drca = e.DropTarget as XamDataTreeNodeControl;
                var dtnode = drca.Node;
                if (dtnode != null)
                {
                    DirectoryNode dr = dtnode.Data as DirectoryNode;
                    if (!dr.IsFolder)
                    {
                        e.OperationType = OperationType.DropNotAllowed;
                    }
                }
                else
                {
                    e.OperationType = OperationType.DropNotAllowed;
                }
            }
        }
        
    }
}