using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.LDAP;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.MainWindowView;

namespace Tabidus.POC.GUI.ViewModel.Discovery
{
    public class MoveLDAPViewModel : ViewModelBase
    {
        private readonly MoveLdapDialog _view;
        private ObservableCollection<DirectoryNode> _treeDataSource;
        private MainWindowViewModel _mainWindowViewModel;

        public MoveLDAPViewModel(MoveLdapDialog view)
        {
            _view = view;
            _mainWindowViewModel = PageNavigatorHelper.GetMainModel();
            MoveLdapCommand = new RelayCommand(OnMoveCommand, CanExecuteCommand);
        }

        public ICommand MoveLdapCommand { get; set; }

        public ObservableCollection<DirectoryNode> TreeDataSource
        {
            get { return _treeDataSource; }
            set
            {
                _treeDataSource = value;
                OnPropertyChanged("TreeDataSource");
            }
        }

        private void OnMoveCommand(object args)
        {
            _mainWindowViewModel.ShowMessage("Moving...");
            var selectNode = FindChildSelectedNode();
            var dnlist = new List<DirectoryNode>();
            foreach (var sn in ApplicationContext.LDAPNodesSelected)
            {
                dnlist.Add(sn);
                var node = selectNode.Find(e => e.GuidString == sn.GuidString);
                if (node != null)
                {
                    dnlist.Remove(sn);
                }
            }
            
            var moveBackground = new BackgroundWorker();
            moveBackground.DoWork += MoveBackground_DoWork;
            moveBackground.RunWorkerAsync(dnlist);
            
            _view.HideWindow();

        }

        private void MoveBackground_DoWork(object sender, DoWorkEventArgs e)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(() =>
            {
                var dnlist = e.Argument as List<DirectoryNode>;
                var ldapView = PageNavigatorHelper.GetMainContent<LDAPPage>();
                if (ldapView != null)
                {
                    var listMove = new ObservableCollection<DirectoryNode>();
                    foreach (var dn in dnlist)
                    {
                        listMove.Add(dn);
                    }
                    ldapView.MoveNotes(listMove, new Guid().ToString(), ApplicationContext.NodeTargetId,true);
                }
            }));
            
        }

        private bool CanExecuteCommand(object args)
        {
            return ApplicationContext.NodeTargetId >= 1;
        }

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
            foreach (var node in ApplicationContext.LDAPNodesSelected)
            {
                var parentNodes = new List<DirectoryNode>();
                GetParentDirectoryNodes(parentNodes, node);
                parentNodes.RemoveAt(0);

                listResult.AddRange(from node2 in ApplicationContext.LDAPNodesSelected
                    where node2.IsFolder && parentNodes.Select(e => e.GuidString).Contains(node2.GuidString)
                    select node);
            }
            return listResult;
        }

        private void FindAllChild(List<DirectoryNode> listDir, DirectoryNode parentNode)
        {
            listDir.Add(parentNode);
            var ldapDirs =
                    ApplicationContext.LdapDirectoriesEndpointsDictionary[ApplicationContext.LDAPActived.Id].Directories;
            
            var nodes = ldapDirs.Where(e => e.ParentId == parentNode.GuidString)
                .Select(e => new DirectoryNode
                {
                    GuidString = e.Id,
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
            var isExisted = listNode.Any(ln => ln.IsFolder == dir.IsFolder && ln.GuidString == dir.GuidString);
            var ldapDirs =
                    ApplicationContext.LdapDirectoriesEndpointsDictionary[ApplicationContext.LDAPActived.Id].Directories;
            var ldapEnds =
                   ApplicationContext.LdapDirectoriesEndpointsDictionary[ApplicationContext.LDAPActived.Id].Endpoints;
            if (!isExisted)
            {
                listNode.Add(dir);
            }
            if (dir.IsFolder)
            {
                
                if (ldapDirs != null)
                {
                    var nodeDir = ldapDirs.Find(e => e.Id == dir.GuidString);
                    foreach (var ep in ldapDirs)
                    {
                        if (nodeDir.ParentId == ep.Id && nodeDir.Id != ep.ParentId) //prevent  circle recursive
                        {
                            var dn = new DirectoryNode{ IsFolder = true, GuidString = ep.Id };
                            GetParentDirectoryNodes(listNode, dn);
                            break;
                        }
                    }
                }
                
            }
            else
            {
                if (ldapEnds != null)
                {
                    var edir = ldapEnds.Find(e => e.Id == dir.GuidString);
                    foreach (var ep in ldapDirs)
                    {
                        if (edir.LDAPDirectoryId == ep.Id)
                        {
                            var dn = new DirectoryNode { IsFolder = true, GuidString = ep.Id };
                            GetParentDirectoryNodes(listNode, dn);
                            break;
                        }
                    }
                }
                
            }
        }
    }
}