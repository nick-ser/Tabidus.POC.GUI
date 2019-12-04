using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Endpoint
{
    public class MoveTreeNodeViewModel : ViewModelBase
    {
        private readonly MoveTargetDialog _view;
        private ObservableCollection<DirectoryNode> _treeDataSource;

        public MoveTreeNodeViewModel(MoveTargetDialog view)
        {
            _view = view;
            MoveCommand = new RelayCommand(OnMoveCommand, CanExecuteCommand);
        }

        public ObservableCollection<DirectoryNode> TreeDataSource
        {
            get { return _treeDataSource; }
            set
            {
                _treeDataSource = value;
                OnPropertyChanged("TreeDataSource");
            }
        }

        public ICommand MoveCommand { get; set; }

        private void OnMoveCommand(object args)
        {
            var data = new MoveFoldersAndEndpointsInputArgs();
            data.TargerFolderId = ApplicationContext.NodeTargetId;

            var selectNode = FindChildSelectedNode();
            var dnlist = new List<DirectoryNode>();
            var allChildNodes = new List<DirectoryNode>();
            foreach (var sn in ApplicationContext.NodesSelected)
            {
                if (sn.IsFolder)
                {
                    var childNodes = new List<DirectoryNode>();
                    FindAllChild(childNodes, sn);
                    allChildNodes.AddRange(childNodes);
                }
                dnlist.Add(sn);
                var node = selectNode.Find(e => e.NodeId == sn.NodeId);
                if (node != null)
                {
                    dnlist.Remove(sn);
                }
            }

            if (allChildNodes.Select(e => e.NodeId).ToList().Contains(data.TargerFolderId))
            {
                var messageDialog = PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                messageDialog.ShowMessageDialog("Target folder is invalid", "Message");
                return;
            }

            data.FolderIds = dnlist.Where(e => e.IsFolder).Select(e => e.NodeId).ToList();
            data.EndpointIds = dnlist.Where(e => !e.IsFolder).Select(e => e.NodeId).ToList();
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.MoveDirectoriesAndEndpointsAction(data);
            }
            _view.HideWindow();
            MakeTree(data.TargerFolderId);
        }

        private bool CanExecuteCommand(object args)
        {
            return ApplicationContext.NodeTargetId >= 1;
        }

        /// <summary>
        ///     Make tree data
        /// </summary>
        /// <returns></returns>
        public void MakeTree()
        {
            var rootId = ApplicationContext.FolderListAll == null ||
                         (ApplicationContext.FolderListAll != null && ApplicationContext.FolderListAll.Count == 0)
                ? 1
                : ApplicationContext.FolderListAll.Min(r => r.FolderId);
            var rootName = ApplicationContext.FolderListAll == null ||
                           (ApplicationContext.FolderListAll != null && ApplicationContext.FolderListAll.Count == 0)
                ? "Company"
                : ApplicationContext.FolderListAll.Find(r => r.FolderId == rootId).FolderName;
            var dir = ApplicationContext.NodesSelected.Find(r => r.NodeId == rootId && r.IsFolder);
            if (dir==null)
            {
                // create root, build subtree and return it
                var node = new DirectoryNode
                {
                    NodeId = rootId,
                    Title = rootName,
                    IsFolder = true,
                    NodeColor = "#000000"
                };
                MakeSubTree(node);
                var listNode = new ObservableCollection<DirectoryNode>();
                listNode.Add(node);
                TreeDataSource = listNode;
            }
            else
            {
                var listNode = new ObservableCollection<DirectoryNode>();
                TreeDataSource = listNode;
            }
            
        }

        /// <summary>
        ///     Make sub tree using recursive
        /// </summary>
        /// <param name="parentNode"></param>
        private void MakeSubTree(DirectoryNode parentNode)
        {
            // find all children of parent node (they have parentId = id of parent node)
            // && !ApplicationContext.NodesSelected.Select(n=>n.NodeId).ToList().Contains(e.FolderId)
            var nodes = ApplicationContext.FolderListAll.Where(e => e.ParentId == parentNode.NodeId)
                .Select(e => new DirectoryNode
                {
                    NodeId = e.FolderId,
                    Title = e.FolderName,
                    IsFolder = true,
                    NodeColor = "#000000"
                }).OrderBy(o => o.Title);

            // build subtree for each child and add it in parent's children collection
            foreach (var node in nodes)
            {
                MakeSubTree(node);
                parentNode.DirectoryNodes.Add(node);
            }
        }

        public void RefreshTreeData()
        {
            _view.DataTree.SelectedDataItems = null;
            SetNodeExpandedState(_view.DataTree.Nodes, true);
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

        /// <summary>
        ///     Find all notes that has the parent node in selected nodes
        /// </summary>
        /// <returns></returns>
        private List<DirectoryNode> FindChildSelectedNode()
        {
            var listResult = new List<DirectoryNode>();
            foreach (var node in ApplicationContext.NodesSelected)
            {
                var parentNodes = new List<DirectoryNode>();
                GetParentDirectoryNodes(parentNodes, node);
                parentNodes.RemoveAt(0);
                
                listResult.AddRange(from node2 in ApplicationContext.NodesSelected
                    where node2.IsFolder && parentNodes.Select(e => e.NodeId).Contains(node2.NodeId)
                    select node);
            }
            return listResult;
        }

        private void FindAllChild(List<DirectoryNode> listDir, DirectoryNode parentNode)
        {
            listDir.Add(parentNode);
            // find all children of parent node (they have parentId = id of parent node)
            // && !ApplicationContext.NodesSelected.Select(n=>n.NodeId).ToList().Contains(e.FolderId)
            var nodes = ApplicationContext.FolderListAll.Where(e => e.ParentId == parentNode.NodeId)
                .Select(e => new DirectoryNode
                {
                    NodeId = e.FolderId,
                    Title = e.FolderName,
                    IsFolder = true,
                    NodeColor = "#000000"
                });
            // build subtree for each child and add it in parent's children collection
            foreach (var node in nodes)
            {
                FindAllChild(listDir, node);
            }
        }

        /// <summary>
        ///     Get all parent notes of a note
        /// </summary>
        /// <param name="listNode"></param>
        /// <param name="dir"></param>
        private void GetParentDirectoryNodes(List<DirectoryNode> listNode, DirectoryNode dir)
        {
            var isExisted = listNode.Any(ln => ln.IsFolder == dir.IsFolder && ln.NodeId == dir.NodeId);

            if (!isExisted)
            {
                listNode.Add(dir);
            }
            if (dir.IsFolder)
            {
                var nodeDir = ApplicationContext.FolderListAll.Find(e => e.FolderId == dir.NodeId);
                foreach (var ep in ApplicationContext.FolderListAll)
                {
                    if (nodeDir.ParentId == ep.FolderId && nodeDir.FolderId != ep.ParentId) //prevent  circle recursive
                    {
                        var dn = new DirectoryNode {IsFolder = true, NodeId = ep.FolderId};
                        GetParentDirectoryNodes(listNode, dn);
                        break;
                    }
                }
            }
            else
            {
                var edir = ApplicationContext.EndPointListAll.Find(e => e.EndpointId == dir.NodeId);
                foreach (var ep in ApplicationContext.FolderListAll)
                {
                    if (edir.FolderId == ep.FolderId)
                    {
                        var dn = new DirectoryNode {IsFolder = true, NodeId = ep.FolderId};
                        GetParentDirectoryNodes(listNode, dn);
                        break;
                    }
                }
            }
        }
    }
}