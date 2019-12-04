using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.LDAP;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.Label;

namespace Tabidus.POC.GUI.ViewModel
{
    /// <summary>
    ///     Class RightTreeViewModel.
    /// </summary>
    public class RightTreeViewModel : ViewModelBase
    {
        /// <summary>
        ///     The _view
        /// </summary>
        private readonly RightTreeElement _view;

        /// <summary>
        ///     The _label filter view model
        /// </summary>
        private LabelFilterViewModel _labelFilterViewModel;

        /// <summary>
        ///     The _tree data source
        /// </summary>
        private ObservableCollection<DirectoryNode> _treeDataSource;

        private bool _directoryPushed = true;

        public bool DirectoryPushed
        {
            get { return _directoryPushed; }
            set
            {
                _directoryPushed = value;
                OnPropertyChanged("DirectoryPushed");
            }
        }

        private bool _softwareTreeVisible;

        public bool SoftwareTreeVisible
        {
            get { return _softwareTreeVisible; }
            set
            {
                _softwareTreeVisible = value;
                OnPropertyChanged("SoftwareTreeVisible");
            }
        }

        private bool _directoryTreeVisible = true;

        public bool DirectoryTreeVisible
        {
            get { return _directoryTreeVisible; }
            set
            {
                _directoryTreeVisible = value;
                OnPropertyChanged("DirectoryTreeVisible");
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RightTreeViewModel" /> class.
        /// </summary>
        /// <param name="view">The view.</param>
        public RightTreeViewModel(RightTreeElement view)
        {
            _view = view;
            TreeDataSource = new ObservableCollection<DirectoryNode>();
            SoftwareTreeDataSource = new ObservableCollection<DirectoryNode>();
            SearchCommand = new RelayCommand(SearchAction, CanSearchCommand);
            BackCommand = new RelayCommand(BackCommandAction);
            ApplicationContext.SearchText = string.Empty;
        }

        /// <summary>
        ///     Gets the search command.
        /// </summary>
        /// <value>The search command.</value>
        public ICommand SearchCommand { get; private set; }

        public ICommand BackCommand { get; private set; }

        private bool CanSearchCommand(object arg)
        {
            if (DirectoryPushed)
            {
                return ApplicationContext.FolderListAll != null && ApplicationContext.FolderListAll.Count > 0;
            }
            else
            {
                return ApplicationContext.LableEndpointDatas != null && ApplicationContext.LableEndpointDatas.Count > 0;
            }
        }

        /// <summary>
        ///     Gets the search text.
        /// </summary>
        /// <value>The search text.</value>
        public string SearchText
        {
            get { return ApplicationContext.SearchText; }
            set
            {
                ApplicationContext.SearchText = value;
                OnPropertyChanged("SearchText");
            }
        }

        /// <summary>
        ///     Gets or sets the label filter view model.
        /// </summary>
        /// <value>The label filter view model.</value>
        public LabelFilterViewModel LabelFilterViewModel
        {
            get { return _labelFilterViewModel; }
            set
            {
                _labelFilterViewModel = value;
                OnPropertyChanged("LabelFilterViewModel");
            }
        }

        /// <summary>
        ///     Gets or sets the tree data source.
        /// </summary>
        /// <value>The tree data source.</value>
        public ObservableCollection<DirectoryNode> TreeDataSource
        {
            get { return _treeDataSource; }
            set
            {
                _treeDataSource = value;
                OnPropertyChanged("TreeDataSource");
            }
        }

        private ObservableCollection<DirectoryNode> _softwareTreeDataSource;

        public ObservableCollection<DirectoryNode> SoftwareTreeDataSource
        {
            get { return _softwareTreeDataSource; }
            set
            {
                _softwareTreeDataSource = value;
                OnPropertyChanged("SoftwareTreeDataSource");
            }
        }

        /// <summary>
        ///     Loads the label view.
        /// </summary>
        /// <param name="refresh">if set to <c>true</c> [refresh].</param>
        public void LoadLabelView(bool refresh = false)
        {
            if (refresh)
            {
                var authRequest = new StringAuthenticateObject {StringAuth = "OK", StringValue = "0"};
                var result =
                    ServiceManager.Invoke(
                        sc =>
                            RequestResponseUtils.GetData<ListLableEndpointResponse>(sc.GetEndPointForLabel, authRequest));
                if (result != null)
                    ApplicationContext.LableEndpointDatas = result.Result.OrderBy(s => s.Name).ToList();
                else
                    ApplicationContext.LableEndpointDatas = new List<LabelEndPointsData>();
            }
            BuildLabelTree(ApplicationContext.LableEndpointDatas, ApplicationContext.LabelNodesSelected);
        }

        public ListLableEndpointResponse GetEndpointByLabel(int labelId, string labelName)
        {
            var listEndpoints = ApplicationContext.LableEndpointDatas.Where(r => r.Id == labelId).ToList();
            var result = new ListLableEndpointResponse(true, listEndpoints);
            result.Message = labelName;
            return result;
        }

        /// <summary>
        ///     Updates the tree.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="restText">if set to <c>true</c> [rest text].</param>
        /// <param name="isReload"></param>
        public void UpdateTree(int nodeId, bool restText = false, bool isReload = false)
        {
            if (restText)
            {
                SearchText = string.Empty;
            }
            MakeTree(nodeId, !string.IsNullOrWhiteSpace(SearchText), SearchText, restText, isReload);
            //LoadLabelView(true);
        }


        /// <summary>
        ///     Updates the tree.
        /// </summary>
        /// <param name="nodeId">The node identifier.</param>
        /// <param name="restText">if set to <c>true</c> [rest text].</param>
        public void UpdateTree(DirectoryNode nodeId, bool restText = false, bool isEdited = false, bool isReload = false)
        {
            if (restText)
            {
                SearchText = string.Empty;
            }
            MakeTree(nodeId, !string.IsNullOrWhiteSpace(SearchText), SearchText, restText, isEdited, isReload);
            //LoadLabelView(true);
        }


        public void UpdateTree(List<DirectoryNode> nodes, bool restText = false, bool isReload = false)
        {
            if (restText)
            {
                SearchText = string.Empty;
            }
            MakeTree(nodes, !string.IsNullOrWhiteSpace(SearchText), SearchText, isReload);
        }

        /// <summary>
        ///     Builds the label tree.
        /// </summary>
        /// <param name="datas">The datas.</param>
        public void BuildLabelTree(List<LabelEndPointsData> datas)
        {
            if (datas != null)
            {
                LabelFilterViewModel = new LabelFilterViewModel(_view.LabelDataTree);
                LabelFilterViewModel.LoadData(datas);
            }
        }

        public void BuildLabelTree(List<LabelEndPointsData> datas, List<DirectoryNode> labelNodesSelected)
        {
            if (datas != null)
            {
                LabelFilterViewModel = new LabelFilterViewModel(_view.LabelDataTree);
                LabelFilterViewModel.LoadData(datas, labelNodesSelected);
            }
        }

        private List<UpdateSource> GetAllUpdateSource()
        {
            var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK"
            };
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<UpdateSource>>(
                sc.GetAllUpdateSource,
                requestObj));

            if (resultDeserialize == null)
            {
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action) (() =>
                    {
                        var messageDialog =
                            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                        messageDialog.ShowMessageDialog("Data is null", "Message");
                    }));
                return new List<UpdateSource>();
            }

            return resultDeserialize;
        }

        public void RebuildUpdateSourceCache()
        {
            ApplicationContext.UpdateSourceList = GetAllUpdateSource();
        }

        public void BuilSoftwareTree(bool IsRefresh = false)
        {
            if (ApplicationContext.UpdateSourceList == null)
            {
                ApplicationContext.UpdateSourceList = GetAllUpdateSource();
            }
            if (IsRefresh)
            {
                ApplicationContext.UpdateSourceList = GetAllUpdateSource();
            }
            var rootId = ApplicationContext.UpdateSourceList.Count == 0
                ? 0
                : ApplicationContext.UpdateSourceList.Min(r => r.Id);
            var rootName = ApplicationContext.UpdateSourceList.Count == 0
                ? "Main Source"
                : ApplicationContext.UpdateSourceList.Find(r => r.Id == rootId).SourceName;
            var rootDate = ApplicationContext.UpdateSourceList.Count == 0
                ? DateTime.MinValue
                : ApplicationContext.UpdateSourceList.Find(r => r.Id == rootId).LastUpdated;
            // create root, build subtree and return it
            var node = new DirectoryNode
            {
                NodeId = rootId,
                Title = rootName + " (POCSERVER)",
                IsFolder = true,
                NodeColor = CommonConstants.SOFTWARE_TREE_GREEN_ONLINE_COLOR,
                NodeWidth = ApplicationContext.GridRightOriginalWidth,
                SourceName = rootName
            };
            MakeSubSoftwareNode(node, rootDate);
            var listNode = new ObservableCollection<DirectoryNode>();
            listNode.Add(node);
            SoftwareTreeDataSource = listNode;
            RefreshSoftwareTree(ApplicationContext.SoftwareSelectedNodeId==0?1: ApplicationContext.SoftwareSelectedNodeId);
        }

        private void MakeSubSoftwareNode(DirectoryNode parentNode, DateTime rootDate)
        {
            // find all children of parent node (they have parentId = id of parent node)
            var nodes = ApplicationContext.UpdateSourceList.Where(e => e.ParentId == parentNode.NodeId)
                .Select(e => new DirectoryNode
                {
                    NodeId = e.Id,
                    Title = string.IsNullOrWhiteSpace(e.SourceName)?e.SystemName: (e.SourceName+ " ("+e.SystemName+")"),
                    IsFolder = true,
                    NodeColor = e.LastUpdated>=rootDate? CommonConstants.SOFTWARE_TREE_GREEN_ONLINE_COLOR : CommonConstants.SOFTWARE_TREE_RED_ONLINE_COLOR,
                    NodeWidth = ApplicationContext.GridRightOriginalWidth,
                    SourceName = e.SourceName
                }).OrderBy(o => o.Title);

            // build subtree for each child and add it in parent's children collection
            foreach (var node in nodes)
            {
                MakeSubSoftwareNode(node, rootDate);
                parentNode.DirectoryNodes.Add(node);
            }
        }

        /// <summary>
        ///     refresh the tree data
        /// </summary>
        /// <param name="nodeSelected">The node selected.</param>
        public void RefreshTreeData(DirectoryNode nodeSelected, bool isEdited = false)
        {
            _view.DataTree.SelectedDataItems = null;
            SetNodeExpandedState(_view.DataTree.Nodes, true, nodeSelected, isEdited);
        }

        public void RefreshTreeAfterAdded(DirectoryNode nodeSelected)
        {
            _view.DataTree.SelectedDataItems = null;
            SetNodeStateAfterAdded(_view.DataTree.Nodes, nodeSelected);
        }

        public void SetNodeDropable()
        {
            SetNodeDropable(_view.DataTree.Nodes);
        }

        public void SetNodeDropableOrNot()
        {
            SetNodeDropableOrNot(_view.DataTree.Nodes);
        }

        public void TreeEnterEditMode(IEnumerable<XamDataTreeNode> nodes)
        {
            var editedNode = ApplicationContext.NodeEditting.Data as DirectoryNode;
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null && editedNode!=null)
                {
                    if (data.Guid == editedNode.Guid && data.IsFolder == editedNode.IsFolder)
                    {
                        _view.DataTree.EnterEditMode(item);
                        break;
                    }
                    
                }

                TreeEnterEditMode(item.Nodes);
            }
        }

        public void RefreshTreeData(List<DirectoryNode> nodeSelected)
        {
            if (_view.DataTree.Nodes != null && _view.DataTree.Nodes.Count > 0)
            {
                _view.DataTree.SelectedDataItems = null;
                nodeSelected = nodeSelected ?? new List<DirectoryNode>();
                if (nodeSelected.Count == 0)
                {
                    nodeSelected.Add((DirectoryNode) _view.DataTree.Nodes[0].Data);
                }
                ApplicationContext.NodesSelected = nodeSelected;
                SetNodeExpandedState(_view.DataTree.Nodes, nodeSelected);
            }
        }

        public void RefreshLabelTreeData(List<DirectoryNode> nodeSelected)
        {
            if (_view.LabelDataTree.Nodes != null && _view.LabelDataTree.Nodes.Count > 0)
            {
                _view.LabelDataTree.SelectedDataItems = null;
                nodeSelected = nodeSelected ?? new List<DirectoryNode>();

                if (nodeSelected.Count == 0)
                {
                    nodeSelected.Add((DirectoryNode) _view.LabelDataTree.Nodes[0].Data);
                }
                ApplicationContext.LabelNodesSelected = nodeSelected;
                SetNodeExpandedState(_view.LabelDataTree.Nodes, nodeSelected, true);
            }
        }

        public void SelectLabelNodeFromGrid(DirectoryNode nodeSelected)
        {
            if (_view.LabelDataTree.Nodes != null && _view.LabelDataTree.Nodes.Count > 0)
            {
                _view.LabelDataTree.SelectedDataItems = null;
                IsSelected = false;
                ApplicationContext.LabelNodesSelected = new List<DirectoryNode>
                {
                    nodeSelected
                };
                SelectLabelNode(_view.LabelDataTree.Nodes, nodeSelected);
            }
        }

        /// <summary>
        ///     refresh the tree data
        /// </summary>
        /// <param name="nodeSelected">The node selected.</param>
        public void RefreshTreeData(int nodeSelected)
        {
            _view.DataTree.SelectedDataItems = null;
            var dn = new DirectoryNode
            {
                IsFolder = true,
                NodeId = nodeSelected
            };
            SetNodeExpandedState(_view.DataTree.Nodes, true, dn);
        }

        private void RefreshSoftwareTree(int nodeSelected)
        {
            _view.SoftwareDataTree.SelectedDataItems = null;
            SetNodeExpandedState(_view.SoftwareDataTree.Nodes, nodeSelected);
        }
        public void RefreshTreeData(List<int> nodeSelecteds)
        {
            _view.DataTree.SelectedDataItems = null;
            SetNodeExpandedState(_view.DataTree.Nodes, nodeSelecteds);
        }

        public void SelectTreeNode(int nodeSelected, List<int> parentList,
            TreeViewSelectMode selectMode = TreeViewSelectMode.All)
        {
            _view.DataTree.SelectedDataItems = null;
            SetNodeExpandedState(_view.DataTree.Nodes, nodeSelected, parentList, selectMode);
        }

        /// <summary>
        ///     Searches the action.
        /// </summary>
        /// <param name="pars">The pars.</param>
        private void SearchAction(object pars)
        {
            _view.BackButton.Visibility = System.Windows.Visibility.Visible;
            if (DirectoryPushed)
            {
                if (!ApplicationContext.DirSearched)
                {
                    ApplicationContext.DirExpandedNodesBeforeSearch = new List<int>();
                    foreach (var id in ApplicationContext.ExpandedIds)
                    {
                        ApplicationContext.DirExpandedNodesBeforeSearch.Add(id);
                    }
                    ApplicationContext.DirNodesSelectedBeforeSearch = new List<DirectoryNode>();
                    foreach (var ns in ApplicationContext.NodesSelected)
                    {
                        ApplicationContext.DirNodesSelectedBeforeSearch.Add(ns);
                    }

                    ApplicationContext.DirectoryTreeSourceBeforeSearch = new ObservableCollection<DirectoryNode>();
                    foreach (var ds in _view.DataTree.ItemsSource as ObservableCollection<DirectoryNode>)
                    {
                        ApplicationContext.DirectoryTreeSourceBeforeSearch.Add(ds);
                    }
                }
                //search for directory
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action) (() => { MakeTree(0, true, SearchText); }));

                SetNodeExpandedState(_view.DataTree.Nodes, true);
                ApplicationContext.DirSearched = true;
            }
            else
            {
                if (!ApplicationContext.LabelSearched)
                {
                    ApplicationContext.LabelExpandedNodesBeforeSearch = new List<int>();
                    foreach (var ns in ApplicationContext.LabelExpandedIds)
                    {
                        ApplicationContext.LabelExpandedNodesBeforeSearch.Add(ns);
                    }
                    ApplicationContext.LabelNodesSelectedBeforeSearch = new List<DirectoryNode>();
                    foreach (var ns in ApplicationContext.LabelNodesSelected)
                    {
                        ApplicationContext.LabelNodesSelectedBeforeSearch.Add(ns);
                    }
                    ApplicationContext.LabelTreeSourceBeforeSearch = new List<DirectoryNode>();
                    foreach (var ns in _view.LabelDataTree.ItemsSource as List<DirectoryNode>)
                    {
                        ApplicationContext.LabelTreeSourceBeforeSearch.Add(ns);
                    }
                    ApplicationContext.LableEndpointDatasBeforeSearch = new List<LabelEndPointsData>();
                    foreach (var ns in ApplicationContext.LableEndpointDatas ?? new List<LabelEndPointsData>())
                    {
                        ApplicationContext.LableEndpointDatasBeforeSearch.Add(ns);
                    }
                }
                //search for label
                var authRequest = new StringAuthenticateObject
                {
                    StringAuth = "OK",
                    StringValue = SearchText
                };

                var result =
                    ServiceManager.Invoke(
                        sc =>
                            RequestResponseUtils.GetData<ListLableEndpointResponse>(sc.SearchEndPointForLabel,
                                authRequest));
                result.Result = result.Result.OrderBy(s => s.Name).ToList();
                ApplicationContext.LableEndpointDatas = result.Result;
                ApplicationContext.LabelExpandedIds = (result
                    .Result.Select(le => le.Id)).ToList();
                BuildLabelTree(result.Result);
                ApplicationContext.LabelSearched = true;
            }
        }

        private void BackCommandAction(object arg)
        {
            _view.BackButton.Visibility = System.Windows.Visibility.Collapsed;
            SearchText = string.Empty;
            ApplicationContext.SearchText = string.Empty;
            if (DirectoryPushed)
            {
                ApplicationContext.ExpandedIds = ApplicationContext.DirExpandedNodesBeforeSearch;
                ApplicationContext.NodesSelected = ApplicationContext.DirNodesSelectedBeforeSearch;
                ApplicationContext.FolderListTree = ApplicationContext.FolderListAll;
                ApplicationContext.EndPointListTree = ApplicationContext.EndPointListAll;
                _view.DataTree.SelectedDataItems = null;
                ReBuildTree(ApplicationContext.NodesSelected);

                ApplicationContext.DirSearched = false;
            }
            else
            {
                ApplicationContext.LableEndpointDatas = ApplicationContext.LableEndpointDatasBeforeSearch;
                ApplicationContext.LabelExpandedIds = ApplicationContext.LabelExpandedNodesBeforeSearch;
                ApplicationContext.LabelNodesSelected = ApplicationContext.LabelNodesSelectedBeforeSearch;
                LabelResetAction();
                ApplicationContext.LabelSearched = false;
            }
        }

        public void SetInitState()
        {
            if (_view.BackButton.Visibility == System.Windows.Visibility.Visible)
            {
                if (DirectoryPushed)
                {
                    ApplicationContext.ExpandedIds = ApplicationContext.DirExpandedNodesBeforeSearch;
                    ApplicationContext.NodesSelected = ApplicationContext.DirNodesSelectedBeforeSearch;
                    ApplicationContext.FolderListTree = ApplicationContext.FolderListAll;
                    ApplicationContext.EndPointListTree = ApplicationContext.EndPointListAll;
                }
                else
                {
                    ApplicationContext.LableEndpointDatas = ApplicationContext.LableEndpointDatasBeforeSearch;
                    ApplicationContext.LabelExpandedIds = ApplicationContext.LabelExpandedNodesBeforeSearch;
                    ApplicationContext.LabelNodesSelected = ApplicationContext.LabelNodesSelectedBeforeSearch;
                }
            }
        }

        private void LabelResetAction()
        {
            _view.LabelDataTree.ItemsSource = ApplicationContext.LabelTreeSourceBeforeSearch;
            _view.LabelDataTree.SelectedDataItems = null;
            SetNodeExpandedState(_view.LabelDataTree.Nodes, ApplicationContext.LabelNodesSelected, true);
        }

        public void DirectoryLabelClicked(bool isDirectory)
        {
            ApplicationContext.FolderListTree = ApplicationContext.FolderListAll;
            ApplicationContext.EndPointListTree = ApplicationContext.EndPointListAll;
            if (isDirectory)
            {
                _view.DataTree.SelectedDataItems = null;
                ReBuildTree(ApplicationContext.NodesSelected);
            }
            else
            {
                LoadLabelView();
            }
        }

        /// <summary>
        ///     Sets the state of the node expanded.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="expandNode">if set to <c>true</c> [expand node].</param>
        private void SetNodeExpandedState(IEnumerable<XamDataTreeNode> nodes, bool expandNode)
        {
            foreach (var item in nodes)
            {
                item.IsExpanded = expandNode;
                SetNodeExpandedState(item.Nodes, expandNode);
            }
        }

        private void SetNodeExpandedState(IEnumerable<XamDataTreeNode> nodes, int nodeSelected, List<int> listParent,
            TreeViewSelectMode selectMode = TreeViewSelectMode.All)
        {
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null)
                {
                    ApplicationContext.NodeId = nodeSelected;
                    if (listParent.Contains(data.NodeId))
                    {
                        item.IsExpanded = true;
                    }
                    if (ApplicationContext.ExpandedIds != null && ApplicationContext.ExpandedIds.Contains(data.NodeId))
                    {
                        item.IsExpanded = true;
                    }
                    if (nodeSelected != 0)
                    {
                        if (selectMode == TreeViewSelectMode.All)
                        {
                            if (data.NodeId == nodeSelected)
                            {
                                item.IsSelected = true;
                            }
                        }
                        else if (selectMode == TreeViewSelectMode.Endpoint)
                        {
                            if (data.NodeId == nodeSelected && !data.IsFolder)
                            {
                                item.IsSelected = true;
                            }
                        }
                        else if (selectMode == TreeViewSelectMode.Folder)
                        {
                            if (data.NodeId == nodeSelected && data.IsFolder)
                            {
                                item.IsSelected = true;
                            }
                        }
                    }
                    item.IsDropTarget = data.IsFolder;
                }

                SetNodeExpandedState(item.Nodes, nodeSelected, listParent, selectMode);
            }
        }

        /// <summary>
        ///     Sets the state of the node expanded.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="expandNode">if set to <c>true</c> [expand node].</param>
        /// <param name="nodeSelected">The node selected.</param>
        /// <param name="isEdited">isEdited</param>
        private void SetNodeExpandedState(IEnumerable<XamDataTreeNode> nodes, bool expandNode,
            DirectoryNode nodeSelected, bool isEdited = false)
        {
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null)
                {
                    ApplicationContext.NodeId = nodeSelected.NodeId == 0
                        ? ApplicationContext.FolderListAll.Min(r => r.FolderId)
                        : nodeSelected.NodeId;
                    if (ApplicationContext.ExpandedIds != null && ApplicationContext.ExpandedIds.Contains(data.NodeId))
                    {
                        item.IsExpanded = true;
                    }
                    if (ApplicationContext.NodeId != 0)
                    {
                        if (data.NodeId == ApplicationContext.NodeId && data.IsFolder == nodeSelected.IsFolder)
                        {
                            item.IsSelected = true;
                            if (isEdited)
                            {
                                ApplicationContext.NodeEditting = item;
//                                _view.DataTree.EnterEditMode(item);
                            }
                        }
                    }

                    item.IsDropTarget = data.IsFolder;
                }

                SetNodeExpandedState(item.Nodes, expandNode, nodeSelected, isEdited);
            }
        }

        private void SetNodeStateAfterAdded(IEnumerable<XamDataTreeNode> nodes, DirectoryNode nodeSelected)
        {
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null)
                {
                    if (ApplicationContext.ExpandedIds != null && ApplicationContext.ExpandedIds.Contains(data.NodeId))
                    {
                        item.IsExpanded = true;
                    }
                    if (data.Guid == nodeSelected.Guid && data.IsFolder == nodeSelected.IsFolder)
                    {
                        item.IsSelected = true;
                        ApplicationContext.NodeEditting = item;
                    }


                    item.IsDropTarget = data.IsFolder;
                }

                SetNodeStateAfterAdded(item.Nodes, nodeSelected);
            }
        }
        private void SetNodeDropable(IEnumerable<XamDataTreeNode> nodes)
        {
            foreach (var item in nodes)
            {
                item.IsDropTarget = true;
                
                SetNodeDropable(item.Nodes);
            }
        }

        private void SetNodeDropableOrNot(IEnumerable<XamDataTreeNode> nodes)
        {
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null)
                {
                    item.IsDropTarget = data.IsFolder;
                }
                SetNodeDropableOrNot(item.Nodes);
            }
        }

        public void EndEditingTree()
        {
            _view.DataTree.ExitEditMode(true);
        }
        private void SetNodeExpandedState(IEnumerable<XamDataTreeNode> nodes, List<DirectoryNode> nodeSelected,
            bool isLabelTree = false)
        {
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null)
                {
                    ApplicationContext.NodeId = nodeSelected.Count > 0
                        ? nodeSelected[0].NodeId
                        : ApplicationContext.FolderListAll.Min(r => r.FolderId);
                    if (!isLabelTree)
                    {
                        if (ApplicationContext.ExpandedIds != null &&
                            ApplicationContext.ExpandedIds.Contains(data.NodeId))
                        {
                            item.IsExpanded = true;
                        }
                    }
                    else
                    {
                        if (ApplicationContext.LabelExpandedIds != null &&
                            ApplicationContext.LabelExpandedIds.Contains(data.NodeId))
                        {
                            item.IsExpanded = true;
                        }
                    }

                    foreach (var n in nodeSelected)
                    {
                        if (n.IsFolder)
                        {
                            if (data.NodeId == n.NodeId && data.IsFolder == n.IsFolder)
                            {
                                item.IsSelected = true;
                            }
                        }
                        else
                        {
                            if (isLabelTree)
                            {
                                if (data.Guid == n.Guid)
                                {
                                    item.IsSelected = true;
                                }
                            }
                            else
                            {
                                if (data.NodeId == n.NodeId && data.IsFolder == n.IsFolder)
                                {
                                    item.IsSelected = true;
                                }
                            }
                        }
                    }

                    item.IsDropTarget = data.IsFolder;
                }

                SetNodeExpandedState(item.Nodes, nodeSelected);
            }
        }

        public bool IsSelected { get; set; }

        private void SelectLabelNode(IEnumerable<XamDataTreeNode> nodes, DirectoryNode nodeSelected)
        {
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null)
                {
                    if (ApplicationContext.LabelExpandedIds != null &&
                        ApplicationContext.LabelExpandedIds.Contains(data.NodeId))
                    {
                        item.IsExpanded = true;
                    }

                    if (data.NodeId == nodeSelected.NodeId && data.IsFolder == nodeSelected.IsFolder && !IsSelected)
                    {
                        item.IsSelected = true;
                        IsSelected = true;
                    }
                }

                SelectLabelNode(item.Nodes, nodeSelected);
            }
        }

        private void SetNodeExpandedState(IEnumerable<XamDataTreeNode> nodes, List<int> nodeSelecteds)
        {
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null)
                {
                    if (ApplicationContext.ExpandedIds != null && ApplicationContext.ExpandedIds.Contains(data.NodeId))
                    {
                        item.IsExpanded = true;
                    }
                    if (nodeSelecteds != null)
                    {
                        if (nodeSelecteds.Contains(data.NodeId))
                        {
                            item.IsSelected = true;
                        }
                    }
                    item.IsDropTarget = data.IsFolder;
                }

                SetNodeExpandedState(item.Nodes, nodeSelecteds);
            }
        }

        private void SetNodeExpandedState(IEnumerable<XamDataTreeNode> nodes, int nodeSelected)
        {
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null)
                {
                    item.IsExpanded = true;

                    if (nodeSelected == data.NodeId)
                    {
                        item.IsSelected = true;
                    }

                    item.IsDropTarget = data.IsFolder;
                }

                SetNodeExpandedState(item.Nodes, nodeSelected);
            }
        }

        public void SetNodeWidth(IEnumerable<XamDataTreeNode> nodes, double width)
        {
            ApplicationContext.GridRightOriginalWidth = width;
            ApplicationContext.IsRebuildTree = true;
            if (DirectoryTreeVisible)
            {
                if (DirectoryPushed)
                    ReBuildTree(ApplicationContext.NodesSelected);
                else
                    BuildLabelTree(ApplicationContext.LableEndpointDatas, ApplicationContext.LabelNodesSelected);
            }
            else if (SoftwareTreeVisible)
            {
                BuilSoftwareTree();
            }
        }
    }
}