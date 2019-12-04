using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Controls.Menus;
using Infragistics.DragDrop;
using Newtonsoft.Json;
using Tabidus.POC.Common;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.DirectoryAssignment;
using Tabidus.POC.GUI.ViewModel.Discovery;
using Tabidus.POC.GUI.ViewModel.Endpoint;
using Tabidus.POC.GUI.ViewModel.Label;
using Tabidus.POC.GUI.ViewModel.Policy;
using Tabidus.POC.GUI.ViewModel.Reporting;
using Tabidus.POC.GUI.ViewModel.Software;
using Tabidus.POC.GUI.ViewModel.Task;

namespace Tabidus.POC.GUI.UserControls
{
    using System;
    using System.ComponentModel;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Resources;
    using System.Windows.Threading;

    /// <summary>
    ///     Interaction logic for RightTreeElement.xaml
    /// </summary>
    public partial class RightTreeElement : UserControl
    {
        private string _originalTitle;
        private string _originalSourceName;

        public RightTreeElement()
        {
            InitializeComponent();
            btnDirectory.BorderThickness = new Thickness(0, 0, 0, 2);
            btnDirectory.BorderBrush = (Brush)(new BrushConverter().ConvertFrom("#1dabed"));
            btnDirectory.Foreground = Brushes.White;
            Model = new RightTreeViewModel(this);
        }

        public RightTreeViewModel Model
        {
            get { return DataContext as RightTreeViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        ///     Handle when user clicks on Directory button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDirectory_Click(object sender, RoutedEventArgs e)
        {
            OnDirectoryClick();
          //btnDirectory.BorderThickness = new Thickness(0, 0, 4, 0);
          //  btnDirectory.BorderBrush = (Brush)(new BrushConverter().ConvertFrom("#1dabed"));
        }

        public void OnDirectoryClick()
        {
            Model.SetInitState();
            BackButton.Visibility = Visibility.Collapsed;
            //btnDirectorySetFocus();
            btnLabel.BorderThickness = new Thickness(0);
            Model.DirectoryPushed = true;
            btnDirectory.BorderThickness = new Thickness(0, 0, 0, 2);
            btnDirectory.BorderBrush = (Brush)(new BrushConverter().ConvertFrom("#1dabed"));
            btnDirectory.Foreground = Brushes.White;
            var bc = new BrushConverter();
            btnLabel.Foreground = (Brush)bc.ConvertFrom("#8e8f98");

            btnLabel.Style = FindResource("PushedTabPageButton") as Style;
            btnDirectory.Style = FindResource("PushedTabPageButton") as Style;
            LabelScrollViewer.Visibility = Visibility.Collapsed;
            DirectoryScrollViewer.Visibility = Visibility.Visible;
            //reload tree when click on the directory
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (Model.SearchCommand.CanExecute(null))
            {
                Model.SearchText = "";
                ApplicationContext.SearchText = "";
                Model.DirectoryLabelClicked(true);

                mainViewModel.AddDeleteButtonVisible = true;
            }
            ApplicationContext.DirSearched = false;
        }

        /// <summary>
        /// BTNs the directory set focus.
        /// </summary>
        public void btnDirectorySetFocus()
        {
            btnDirectory.Style = FindResource("PushedTabPageButton") as Style;

            Uri resourceUri = new Uri("../Images/directory_button_down.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            if (streamInfo != null)
            {
                BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                var brush = new ImageBrush();
                brush.ImageSource = temp;

                btnDirectory.Background = brush;
            }
        }

        /// <summary>
        ///     Handle when user clicks on Labels button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLabel_Click(object sender, RoutedEventArgs e)
        {
            Model.SetInitState();
            BackButton.Visibility = Visibility.Collapsed;
            //BtnDirectoryLostFocus();
            Model.DirectoryPushed = false;
            btnLabel.Style = FindResource("PushedTabPageButton") as Style;
            btnLabel.Foreground = Brushes.White;
            btnDirectory.Style = FindResource("PushedTabPageButton") as Style;
            var bc = new BrushConverter();
            btnDirectory.Foreground = (Brush)bc.ConvertFrom("#8e8f98");
            btnDirectory.BorderThickness = new Thickness(0);
            btnLabel.BorderThickness = new Thickness(0, 0, 0, 2);
            btnLabel.BorderBrush = (Brush)(new BrushConverter().ConvertFrom("#1dabed"));
            LabelScrollViewer.Visibility = Visibility.Visible;
            DirectoryScrollViewer.Visibility = Visibility.Collapsed;
            Model.SearchText = "";
            ApplicationContext.SearchText = "";
            Model.DirectoryLabelClicked(false);
            ApplicationContext.LabelSearched = false;
        }

        public void BtnDirectoryLostFocus()
        {
            btnDirectory.Style = FindResource("TabPageButton") as Style;

            Uri resourceUri = new Uri("../Images/directory_button_up.png", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            if (streamInfo != null)
            {
                BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
                var brush = new ImageBrush();
                brush.ImageSource = temp;

                btnDirectory.Background = brush;
            }
        }

        /// <summary>
        ///     event selected collection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTree_SelectedNodesCollectionChanged(object sender, NodeSelectionEventArgs e)
        {
            if (e.CurrentSelectedNodes.Count < 1)
            {
                if (DataTree.Equals(sender))
                {
                    ApplicationContext.IsError = true;
                    ApplicationContext.NodesSelected = new List<DirectoryNode>();
                }
                else
                {
                    ApplicationContext.LabelNodesSelected = new List<DirectoryNode>();
                }
            }
            else
            {
                if (SoftwareDataTree.Equals(sender))
                {
                    ApplicationContext.SoftwareSelectedNodeId = (e.CurrentSelectedNodes[0].Data as DirectoryNode).NodeId;
                    if (PageNavigatorHelper.IsCurrent<SoftwarePage>())
                    {
                        var viewModel =
                        PageNavigatorHelper.GetMainContentViewModel<SoftwareViewModel>();
                        viewModel.SourceName = (e.CurrentSelectedNodes[0].Data as DirectoryNode).Title.ToUpper();
                        viewModel.BuidPage();
                    }
                    return;
                }
                if (CheckScreenWithoutRefresh()) return;
                if (ApplicationContext.IsRebuildTree)
                {
                    if (!LabelDataTree.Equals(sender) &&
                        e.CurrentSelectedNodes.Count == ApplicationContext.NodesSelected.Count)
                    {
                        ApplicationContext.IsRebuildTree = false;
                    }
                    if (LabelDataTree.Equals(sender) &&
                        e.CurrentSelectedNodes.Count == ApplicationContext.LabelNodesSelected.Count)
                    {
                        ApplicationContext.IsRebuildTree = false;
                    }
                    return;
                }
                var ln = new List<DirectoryNode>();
                var fids = new List<DirectoryNode>();
                var eids = new List<int>();
                foreach (var cn in e.CurrentSelectedNodes)
                {
                    var cnode = cn.Data as DirectoryNode;
                    if (cnode != null)
                    {
                        ln.Add(cnode);
                        if (cnode.IsFolder)
                        {
                            fids.Add(cnode);
                        }
                        else
                        {
                            eids.Add(cnode.NodeId);
                        }
                    }
                }
                var node = e.CurrentSelectedNodes[0].Data as DirectoryNode;
                if (node != null)
                {
                    if (LabelDataTree.Equals(sender))
                    {
                        if (Model.DirectoryPushed)
                        {
                            return;
                        }
                        ApplicationContext.LabelNodesSelected = ln;
                        ApplicationContext.IsFromLabel = true;
                        if (CheckEndpointNavigationClicked())
                        {
                            if (e.CurrentSelectedNodes.Count == 1)
                            {
                                if (node.IsFolder)
                                {
                                    var listResult = new List<ListLableEndpointResponse>();
                                    foreach (var fid in fids)
                                    {
                                        var result = Model.GetEndpointByLabel(fid.NodeId, fid.Title);
                                        listResult.Add(result);
                                    }
                                    if (!PageNavigatorHelper.IsCurrent<EndPointListPage>())
                                        PageNavigatorHelper.Switch(new EndPointListPage());
                                    var viewModel =
                                        PageNavigatorHelper.GetMainContentViewModel<GroupViewModel>();

                                    viewModel.SetParamsValueForLabel(listResult, eids);
                                    viewModel.GetLabelData();
                                }
                                else
                                {
                                    //dislay EndpointView page
                                    if (!PageNavigatorHelper.IsCurrent<EndPointViewPage>())
                                        PageNavigatorHelper.Switch(new EndPointViewPage());
                                    var viewModel =
                                        PageNavigatorHelper.GetMainContentViewModel<EndpointViewModel>();
                                    viewModel.SetParams(node.NodeId);
                                    viewModel.ReloadData();
                                }
                            }
                            else
                            {
                                var listResult = new List<ListLableEndpointResponse>();
                                foreach (var fid in fids)
                                {
                                    var result = Model.GetEndpointByLabel(fid.NodeId, fid.Title);
                                    listResult.Add(result);
                                }
                                if (!PageNavigatorHelper.IsCurrent<EndPointListPage>())
                                    PageNavigatorHelper.Switch(new EndPointListPage());
                                var viewModel =
                                    PageNavigatorHelper.GetMainContentViewModel<GroupViewModel>();

                                viewModel.SetParamsValueForLabel(listResult, eids);
                                viewModel.GetLabelData();
                            }
                        }
                    }
                    else
                    {
                        if (!Model.DirectoryPushed)
                        {
                            return;
                        }
                        ApplicationContext.IsFromLabel = false;
                        ApplicationContext.IsError = !node.IsFolder;
                        ApplicationContext.NodeId = node.NodeId;
                        //after add a note, select one node only
                        if (ApplicationContext.IsAddNode)
                        {
                            var ns = new List<DirectoryNode>();
                            ns.Add(ln[0]);
                            ln = ns;
                            ApplicationContext.IsAddNode = false;
                            if (CheckEndpointNavigationClicked())
                            {
                                if (node.IsFolder)
                                {
                                    //dislay GroupView page
                                    PageNavigatorHelper.Switch(new EndPointListPage());
                                    var viewModel =
                                        PageNavigatorHelper.GetMainContentViewModel<GroupViewModel>();
                                    var selectedFolderIds = new List<int>();
                                    var selectedEndpointIds = new List<int>();
                                    selectedFolderIds.Add(node.NodeId);
                                    viewModel.SetParamsValueForDirectory(selectedFolderIds, selectedEndpointIds,
                                        ApplicationContext.SearchText, true, node.Guid, node.Title);
                                    viewModel.EditOrAdd();
                                }
                                else
                                {
                                    //dislay EndpointView page
                                    PageNavigatorHelper.Switch(new EndPointViewPage());
                                    var viewModel =
                                        PageNavigatorHelper.GetMainContentViewModel<EndpointViewModel>();
                                    viewModel.SetParams(node.NodeId);
                                    viewModel.EditOrAdd(node.Title, true);
                                }
                            }
                        }
                        else if (ApplicationContext.IsEditNode)
                        {
                            var ns = new List<DirectoryNode>();
                            ns.Add(ln[0]);
                            ln = ns;
                            ApplicationContext.IsEditNode = false;
                            if (CheckEndpointNavigationClicked())
                            {
                                if (node.IsFolder)
                                {
                                    //dislay GroupView page
                                    if (!PageNavigatorHelper.IsCurrent<EndPointListPage>())
                                        PageNavigatorHelper.Switch(new EndPointListPage());
                                    var viewModel =
                                        PageNavigatorHelper.GetMainContentViewModel<GroupViewModel>();
                                    var selectedFolderIds = new List<int>();
                                    var selectedEndpointIds = new List<int>();
                                    selectedFolderIds.Add(node.NodeId);
                                    viewModel.SetParamsValueForDirectory(selectedFolderIds, selectedEndpointIds,
                                        ApplicationContext.SearchText, false, Guid.NewGuid(), "");
                                    viewModel.EditOrAdd();
                                }
                                else
                                {
                                    //dislay EndpointView page
                                    if (!PageNavigatorHelper.IsCurrent<EndPointViewPage>())
                                        PageNavigatorHelper.Switch(new EndPointViewPage());
                                    var viewModel =
                                        PageNavigatorHelper.GetMainContentViewModel<EndpointViewModel>();
                                    viewModel.SetParams(node.NodeId);
                                    viewModel.EditOrAdd(node.Title);
                                }
                            }
                        }
                        else
                        {
                            if (ApplicationContext.IsDeleteNode)
                            {
                                ApplicationContext.IsDeleteNode = false;
                            }
                            if (CheckEndpointNavigationClicked())
                            {
                                if (e.CurrentSelectedNodes.Count == 1)
                                {
                                    if (e.OriginalSelectedNodes.Count == 0)
                                    {
                                        if (node != null)
                                        {
                                            if (node.IsFolder)
                                            {
                                                ApplicationContext.NodesSelected = ln;
                                                var pageViewModel =
                                                    PageNavigatorHelper.GetMainContentViewModel<PageViewModelBase>();
                                                //if (pageViewModel != null &&
                                                //    pageViewModel.GetType() == typeof (ColorCodePageViewModel))
                                                //{
                                                //    Model.GetBelowNode(node);
                                                //    return;
                                                //}
                                                //if (pageViewModel != null &&
                                                //    pageViewModel.GetType() == typeof (AssignmentViewModel))
                                                //{
                                                //    var assignmentVM = pageViewModel as AssignmentViewModel;
                                                //    if (assignmentVM != null)
                                                //        assignmentVM.GetRuleByFolder();
                                                //    Model.GetBelowNode(node);
                                                //    return;
                                                //}
                                                if (pageViewModel != null &&
                                                    pageViewModel.GetType() == typeof(PolicyEnhancementPageViewModel))
                                                {
                                                    var assignmentVM = pageViewModel as PolicyEnhancementPageViewModel;
                                                    if (assignmentVM != null)
                                                        assignmentVM.BuidPage();
                                                    Model.GetBelowNode(node);
                                                    return;
                                                }
												if (pageViewModel != null &&
													pageViewModel.GetType() == typeof(TaskListViewModel))
												{
													return;
												}
												//dislay GroupView page
												if (!PageNavigatorHelper.IsCurrent<EndPointListPage>())
                                                    PageNavigatorHelper.Switch(new EndPointListPage());
                                                var viewModel =
                                                    PageNavigatorHelper.GetMainContentViewModel<GroupViewModel>();
                                                var selectedFolderIds = new List<int>();
                                                var selectedEndpointIds = new List<int>();
                                                selectedFolderIds.Add(node.NodeId);
                                                viewModel.SetParamsValueForDirectory(selectedFolderIds,
                                                    selectedEndpointIds,
                                                    ApplicationContext.SearchText, false, Guid.NewGuid(), "");
                                                viewModel.GetData();
                                            }
                                            else
                                            {
                                                if (PageNavigatorHelper.IsCurrent<PolicyEnhancementPage>())
                                                {
                                                    ApplicationContext.NodesSelected = ln;
                                                    var pageVM = PageNavigatorHelper.GetMainContentViewModel<PolicyEnhancementPageViewModel>();
                                                    if (pageVM != null)
                                                        pageVM.BuidPage();
                                                    Model.GetBelowNode(node);
                                                    return;
                                                }
												else if (PageNavigatorHelper.IsCurrent<TaskListPage>())
                                                {
                                                    ApplicationContext.NodesSelected = ln;
                                                    var pageVM = PageNavigatorHelper.GetMainContentViewModel<TaskListViewModel>();
                                                    if (pageVM != null)
                                                        pageVM.Refresh();
                                                    return;
                                                }
                                                //dislay EndpointView page
                                                if (!PageNavigatorHelper.IsCurrent<EndPointViewPage>())
                                                    PageNavigatorHelper.Switch(new EndPointViewPage());
                                                var viewModel =
                                                    PageNavigatorHelper.GetMainContentViewModel<EndpointViewModel>();
                                                viewModel.SetParams(node.NodeId);
                                                viewModel.ReloadData();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var oriNote = e.OriginalSelectedNodes[0].Data as DirectoryNode;
                                        if (e.OriginalSelectedNodes.Count == 1 && oriNote.IsFolder == node.IsFolder &&
                                            oriNote.NodeId == node.NodeId)
                                        {
                                            //doing nothing because re-selected itself
                                        }
                                        else
                                        {
                                            if (node != null)
                                            {
                                                if (node.IsFolder)
                                                {
                                                    if (PageNavigatorHelper.IsCurrent<ColorCodePage>())
                                                    {
                                                        var viewModel =
                                                            PageNavigatorHelper
                                                                .GetMainContentViewModel<ColorCodePageViewModel>();
                                                        viewModel.SetParams(node);
                                                        viewModel.GetData();
                                                    }
                                                    else if (PageNavigatorHelper.IsCurrent<DirectoryAssignmentPage>())
                                                    {
                                                        ApplicationContext.NodesSelected = ln;
                                                        var viewModel =
                                                            PageNavigatorHelper
                                                                .GetMainContentViewModel<AssignmentViewModel>();
                                                        viewModel.GetRuleByFolder();
                                                    }
                                                    else if (PageNavigatorHelper.IsCurrent<PolicyEnhancementPage>())
                                                    {
                                                        ApplicationContext.NodesSelected = ln;
                                                        var viewModel = PageNavigatorHelper.GetMainContentViewModel<PolicyEnhancementPageViewModel>();
                                                        if (viewModel != null)
                                                            viewModel.BuidPage();
                                                    }
													else if (PageNavigatorHelper.IsCurrent<TaskListPage>())
                                                    {
                                                        ApplicationContext.NodesSelected = ln;
                                                        var viewModel = PageNavigatorHelper.GetMainContentViewModel<TaskListViewModel>();
                                                        if (viewModel != null)
                                                            viewModel.Refresh();
                                                    }
                                                    //dislay GroupView page
                                                    else
                                                    {
                                                        if (!PageNavigatorHelper.IsCurrent<EndPointListPage>())
                                                            PageNavigatorHelper.Switch(new EndPointListPage());
                                                        var viewModel =
                                                            PageNavigatorHelper.GetMainContentViewModel<GroupViewModel>();
                                                        var selectedFolderIds = new List<int>();
                                                        var selectedEndpointIds = new List<int>();
                                                        selectedFolderIds.Add(node.NodeId);
                                                        viewModel.SetParamsValueForDirectory(selectedFolderIds,
                                                            selectedEndpointIds,
                                                            ApplicationContext.SearchText, false, Guid.NewGuid(), "");
                                                        viewModel.GetData();
                                                    }
                                                }
                                                else
                                                {
                                                    ApplicationContext.NodesSelected = ln;
                                                    if (PageNavigatorHelper.IsCurrent<PolicyEnhancementPage>())
                                                    {
                                                        var pageViewModel = PageNavigatorHelper.GetMainContentViewModel<PolicyEnhancementPageViewModel>();
                                                        if (pageViewModel != null)
                                                            pageViewModel.BuidPage();
                                                    }
													else if (PageNavigatorHelper.IsCurrent<TaskListPage>())
	                                                {
														var taskListViewModel = PageNavigatorHelper.GetMainContentViewModel<PageViewModelBase>();
														taskListViewModel.Refresh();
													}
                                                    else
                                                    {
                                                        //dislay EndpointView page
                                                        if (!PageNavigatorHelper.IsCurrent<EndPointViewPage>())
                                                            PageNavigatorHelper.Switch(new EndPointViewPage());
                                                        var viewModel =
                                                            PageNavigatorHelper.GetMainContentViewModel<EndpointViewModel>();
                                                        viewModel.SetParams(node.NodeId);
                                                        viewModel.ReloadData();
                                                    }
                                                    
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
	                                if (PageNavigatorHelper.IsCurrent<PolicyEnhancementPage>())
	                                {
		                                ApplicationContext.NodesSelected = ln;
										var pageViewModel = PageNavigatorHelper.GetMainContentViewModel<PolicyEnhancementPageViewModel>();
										if (pageViewModel != null)
											pageViewModel.BuidPage();
										return;
	                                }
                                    //dislay GroupView page
                                    if (!PageNavigatorHelper.IsCurrent<EndPointListPage>())
                                        PageNavigatorHelper.Switch(new EndPointListPage());
                                    var viewModel =
                                        PageNavigatorHelper.GetMainContentViewModel<GroupViewModel>();

                                    viewModel.SetParamsValueForDirectory(fids.Select(r => r.NodeId).ToList(), eids,
                                        ApplicationContext.SearchText, false, Guid.NewGuid(), "");
                                    viewModel.GetData();
                                }
                            }
                            else if (PageNavigatorHelper.IsCurrent<POCQuarantinePage>())
                            {
                                var viewModel = PageNavigatorHelper.GetMainContentViewModel<POCQuarantineViewModel>();
                                if (e.CurrentSelectedNodes.Count == 1)
                                {
                                    if (!node.IsFolder)
                                    {
                                        viewModel.RefreshQuarantineByComputerIds(new List<int> { node.NodeId });
                                    }
                                    else
                                    {
                                        viewModel.RefreshQuarantineByFolder(node.NodeId);
                                    }
                                }
                                else
                                {
                                    viewModel.RefreshByGroupEntities(fids.Select(r => r.NodeId).ToList(), eids);
                                }
                            }
                        }


                        ApplicationContext.NodesSelected = ln;
                        Model.GetBelowNode(node);
                    }
                }
            }
        }

        private bool CheckScreenWithoutRefresh()
        {
            var childViewModel = PageNavigatorHelper.GetMainContentViewModel<ViewModelBase>();
            if (childViewModel != null && ApplicationContext.IsReloadForRefresh)
            {
                #region Screen without refresh function

                if (childViewModel.GetType() == typeof (LabelViewModel) ||
                    (childViewModel.GetType() == typeof (ImportFilePageViewModel)) ||
                    (childViewModel.GetType() == typeof (NeighborhoodWatchViewModel)))
                {
                    //don't refresh labels screen
                    ApplicationContext.IsReloadForRefresh = false;
                    return true;
                }

                #endregion
            }
            ApplicationContext.IsReloadForRefresh = false;
            return false;
        }

        private bool CheckEndpointNavigationClicked()
        {
            var mainVm = PageNavigatorHelper.GetMainModel();
            if (mainVm != null &&
                (mainVm.NavigationIndex == (int) NavigationIndexes.Endpoint ||
                 mainVm.NavigationIndex == (int) NavigationIndexes.Label))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Save the node name edited
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTree_NodeExitedEditMode(object sender, NodeEventArgs e)
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            var nodedata = e.Node.Data as DirectoryNode;
            if (DataTree.Equals(sender))
            {
                if (nodedata != null && !string.IsNullOrWhiteSpace(nodedata.Title.Trim()))
                {
                    if (nodedata != null && nodedata.IsFolder)
                    {
                        if (nodedata.Title.Length > 200)
                        {
                            var mdialog = PageNavigatorHelper._MainWindow.MessageDialogView;
                            mdialog.ShowMessageDialog("Name's length must be less than 200 letters", "Message");

                            nodedata.Title = _originalTitle;
                        }
                        else
                        {
                            DataTree.SelectionSettings.NodeSelection = TreeSelectionType.None;
                            var ed = new DirectoryEndpoint
                            {
                                Name = nodedata.Title,
                                FolderId = nodedata.NodeId
                            };

                            if (mainViewModel != null)
                            {
                                mainViewModel.EditDirectoryAction(ed);
                            }
                        }
                    }
                    if (nodedata != null && !nodedata.IsFolder)
                    {
                        if (nodedata.Title.Length > 250)
                        {
                            var mdialog = PageNavigatorHelper._MainWindow.MessageDialogView;
                            mdialog.ShowMessageDialog("Name's length must be less than 250 letters", "Message");

                            nodedata.Title = _originalTitle;
                        }
                        else
                        {
                            DataTree.SelectionSettings.NodeSelection = TreeSelectionType.None;
                            var ed = new DirectoryEndpoint
                            {
                                Name = nodedata.Title,
                                FolderId = nodedata.NodeId
                            };

                            if (mainViewModel != null)
                            {
                                mainViewModel.EditEndpointAction(ed);
                            }
                        }
                    }
                }
                else
                {
                    if (nodedata != null)
                    {
                        nodedata.Title = _originalTitle;
                    }
                }
            }
            else if (SoftwareDataTree.Equals(sender))
            {
                if (nodedata != null && !string.IsNullOrWhiteSpace(nodedata.SourceName.Trim()))
                {
                    if (nodedata.SourceName.Length > 200)
                    {
                        var mdialog = PageNavigatorHelper._MainWindow.MessageDialogView;
                        mdialog.ShowMessageDialog("the length of SourceName must be less than 200 letters", "Message");

                        nodedata.Title = _originalTitle;
                        nodedata.SourceName = _originalSourceName;
                        return;
                    }
                    var updSource = ApplicationContext.UpdateSourceList.Find(r => r.Id == nodedata.NodeId);
                    if (updSource != null)
                    {
                        updSource.SourceName = nodedata.Title;
                    }
                    Model.BuilSoftwareTree();
                    var editSoftwareBg = new BackgroundWorkerHelper();
                    editSoftwareBg.AddDoWork(SaveBackgroundWorker_DoWork).DoWork(updSource);
                }
                else
                {
                    if (nodedata != null)
                    {
                        nodedata.Title = _originalTitle;
                        nodedata.SourceName = _originalSourceName;
                    }
                }
            }
            ApplicationContext.IsBusy = false;
        }

        private void SaveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                var keyEncryption = Functions.GetConfig("MESSAGE_KEY", "");
                var updSource = e.Argument as UpdateSource;
                if (updSource != null)
                {
                    using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                    {
                        var requestData = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(updSource),
                            keyEncryption);
                        sc.EditUpdateSource(requestData);
                    }
                }
            }));
        }

        /// <summary>
        ///     Event of dropping a note
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTree_OnNodeDragEnd(object sender, DragDropEventArgs e)
        {
            if (e.OperationType == OperationType.DropNotAllowed)
            {
                return;
            }
            var data = new MoveFoldersAndEndpointsInputArgs();
            var dragNode = e.Data as XamDataTreeNode;
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (dragNode != null)
            {
                var dragData = dragNode.Data as DirectoryNode;
                if (dragData != null)
                {
                    var dropTarget = e.DropTarget as XamDataTreeNodeControl;
                    if (dropTarget != null)
                    {
                        var dropData = dropTarget.Node.Data as DirectoryNode;
                        if (dropData != null)
                        {
                            if (DataTree.Equals(sender))
                            {
                                ApplicationContext.NodeId = dragData.NodeId;
                                if (!(dropData.NodeId == dragData.NodeId && dropData.IsFolder == dragData.IsFolder))
                                {
                                    data.TargerFolderId = dropData.NodeId;
                                    var lstfi = new List<int>();
                                    var lstei = new List<int>();
                                    if (dragData.IsFolder)
                                    {
                                        lstfi.Add(dragData.NodeId);
                                    }
                                    else
                                    {
                                        lstei.Add(dragData.NodeId);
                                    }
                                    data.FolderIds = lstfi;
                                    data.EndpointIds = lstei;

                                    if (mainViewModel != null)
                                    {
                                        mainViewModel.MoveDirectoriesAndEndpointsAction(data);
                                    }
                                }
                            }
                            else if (SoftwareDataTree.Equals(sender))
                            {
                                var updSource = ApplicationContext.UpdateSourceList.Find(r => r.Id == dragData.NodeId);
                                if (updSource != null)
                                {
                                    updSource.ParentId = dropData.NodeId;
                                    var editUpdSourceBg = new BackgroundWorkerHelper();
                                    editUpdSourceBg.AddDoWork(SaveBackgroundWorker_DoWork).DoWork(updSource);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DataTree_OnNodeExpansionChanged(object sender, NodeExpansionChangedEventArgs e)
        {
            var nodedata = e.Node.Data as DirectoryNode;
            var id = nodedata != null ? nodedata.NodeId : 1;
            if (DataTree.Equals(sender))
            {
                if (e.Node.IsExpanded)
                {
                    if (!ApplicationContext.ExpandedIds.Contains(id))
                    {
                        ApplicationContext.ExpandedIds.Add(id);
                    }
                }
                else
                {
                    if (ApplicationContext.ExpandedIds.Contains(id))
                    {
                        ApplicationContext.ExpandedIds.Remove(id);
                    }
                }
            }
            else
            {
                if (e.Node.IsExpanded)
                {
                    if (nodedata != null && nodedata.IsFolder && !ApplicationContext.LabelExpandedIds.Contains(id))
                    {
                        ApplicationContext.LabelExpandedIds.Add(id);
                    }
                }
                else
                {
                    if (ApplicationContext.LabelExpandedIds.Contains(id))
                    {
                        ApplicationContext.LabelExpandedIds.Remove(id);
                    }
                }
            }
        }

        private void SearchTextbox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var viewModel = (RightTreeViewModel) DataContext;
                if (viewModel.SearchCommand.CanExecute(null))
                    viewModel.SearchCommand.Execute(null);
            }
        }

        private void DataTree_OnNodeEnteredEditMode(object sender, TreeEditingNodeEventArgs e)
        {
            ApplicationContext.IsBusy = true;

            var nodedata = e.Node.Data as DirectoryNode;
            if (nodedata != null)
            {
                _originalTitle = nodedata.Title;
                if (SoftwareDataTree.Equals(sender))
                {
                    _originalSourceName = nodedata.SourceName;
                }
            }
        }

        private void DataTree_OnInitializeNode(object sender, InitializeNodeEventArgs e)
        {
            e.Node.IsExpanded = true;
        }
    }
}