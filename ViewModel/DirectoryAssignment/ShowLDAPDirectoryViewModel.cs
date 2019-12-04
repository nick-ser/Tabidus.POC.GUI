using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.LDAP;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.MainWindowView;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class ShowLDAPDirectoryViewModel : ViewModelBase
    {
        private readonly ShowLdapDirectoryDialog _view;
        private readonly LDAPAssignmentViewModel _ldapAssignmentViewModel;

        private LDAPDirectoriesEndpoints _ldapDirectoriesEndpoints;
        private MainWindowViewModel _mainWindowViewModel;

        private readonly MessageDialog _messageDialog =
            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;

        private ObservableCollection<DirectoryNode> _treeDataSource;

        public ShowLDAPDirectoryViewModel(ShowLdapDirectoryDialog view, LDAPAssignmentViewModel ldapAssignmentViewModel)
        {
            _view = view;
            _ldapAssignmentViewModel = ldapAssignmentViewModel;
            _mainWindowViewModel = PageNavigatorHelper.GetMainModel();
            SelectLdapCommand = new RelayCommand(OnMoveCommand, CanExecuteCommand);
            _view.Loaded += _view_Loaded;
        }

        public ICommand SelectLdapCommand { get; set; }
        public int Type { get; set; }
        public int DomainId { get; set; }
        public string DomainName { get; set; }
        public ObservableCollection<DirectoryNode> TreeDataSource
        {
            get { return _treeDataSource; }
            set
            {
                _treeDataSource = value;
                OnPropertyChanged("TreeDataSource");
            }
        }

        private void _view_Loaded(object sender, RoutedEventArgs e)
        {
            if (Type == 1)
            {
                _view.DataTree.SelectionSettings.NodeSelection = TreeSelectionType.Single;
            }
            else
            {
                _view.DataTree.SelectionSettings.NodeSelection = TreeSelectionType.Multiple;
            }
        }

        private void OnMoveCommand(object args)
        {
            var selectNode = FindChildSelectedNode();
            var dnlist = new List<DirectoryNode>();
            foreach (var sn in ApplicationContext.SelectedTargetNodes)
            {
                dnlist.Add(sn);
                var node = selectNode.Find(e => e.GuidString == sn.GuidString);
                if (node != null)
                {
                    dnlist.Remove(sn);
                }
            }
            var pathNode = string.Empty;
            var nodeIds = new List<string>();
            var ncount = 0;
            foreach (var node in dnlist)
            {
                if (Type != 3 && !node.IsFolder)
                {
                    _messageDialog.ShowMessageDialog("Cannot select a computer, please select a folder", "Message");
                    
                    return;
                }
                if (Type == 3 && node.IsFolder)
                {
                    _messageDialog.ShowMessageDialog("Cannot select a folder, please select a computer", "Message");
                    return;
                }
                var listNodes = new List<string>();
                ncount++;
                if (Type == 3)
                {
                    pathNode += node.Title + (dnlist.Count == ncount ? "" : " ; ");
                }
                else
                {
                    var dir = _ldapDirectoriesEndpoints.Directories.Find(r => r.Id == node.GuidString);
                    GetPathNode(listNodes, dir);
                    listNodes.Reverse();

                    pathNode += string.Join(" | ", listNodes) + (dnlist.Count == ncount ? "" : ";");
                }
                nodeIds.Add(node.GuidString);
            }
            switch (Type)
            {
                case 1:
                    _ldapAssignmentViewModel.TxtLDAPFolder = pathNode;
                    _ldapAssignmentViewModel.LDAPFolderId = dnlist != null && dnlist.Count > 0
                        ? dnlist[0].GuidString
                        : string.Empty;
                    _ldapAssignmentViewModel.ChbLDAPFolderChecked = true;
                    break;
                case 2:
                    _ldapAssignmentViewModel.TxtExcludeFolder = pathNode;
                    _ldapAssignmentViewModel.ExcludeFolderIds = nodeIds;
                    _ldapAssignmentViewModel.ChbExcludeFolderChecked = true;
                    break;
                case 3:
                    _ldapAssignmentViewModel.TxtExcludeComputer = pathNode;
                    _ldapAssignmentViewModel.ExcludeComputerIds = nodeIds;
                    _ldapAssignmentViewModel.ChbExcludeComputerChecked = true;
                    break;
            }
            _ldapAssignmentViewModel.LDAPAssignmentElementChanged.Execute(null);
            _view.Close();
        }

        private void GetPathNode(List<string> listNode, LDAPDirectory dir)
        {
            if (dir == null) return;
            listNode.Add(dir.FolderName);
            foreach (var ep in _ldapDirectoriesEndpoints.Directories)
            {
                if (dir.ParentId == ep.Id)
                {
                    GetPathNode(listNode, ep);
                    break;
                }
            }
        }

        private bool CanExecuteCommand(object args)
        {
            return ApplicationContext.SelectedTargetNodes != null && ApplicationContext.SelectedTargetNodes.Count > 0;
        }

        private LDAPDirectoriesEndpoints GetData()
        {
            var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK",
                StringValue = DomainId.ToString()
            };
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<LDAPDirectoriesEndpoints>(
                sc.GetAllLDAPDirectoryEndpoint,
                requestObj));

            if (resultDeserialize == null)
            {
                var messageDialog =
                    PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                messageDialog.ShowMessageDialog("Data is null", "Message");

                return new LDAPDirectoriesEndpoints();
            }

            return resultDeserialize;
        }

        public void MakeTree(List<DirectoryNode> nodeSelected)
        {
            NodesSelected = nodeSelected;
            var ldapData = GetData();
            _ldapDirectoriesEndpoints = ldapData;
            var listNode = new ObservableCollection<DirectoryNode>();
            if (ldapData != null)
            {
                var directories = ldapData.Directories;
                if (directories != null && directories.Count > 0)
                {
                    var domainDir = directories.Find(r => r.FolderName == DomainName);
                    if (domainDir != null)
                    {
                        var rootId = domainDir.Id;
                        var rootName = DomainName;
                        // create root, build subtree and return it
                        var node = new DirectoryNode
                        {
                            GuidString = rootId,
                            Title = rootName,
                            IsFolder = true,
                            NodeWidth = 350,
                            NodeColor = "#000000"
                        };

                        MakeLDAPSubTree(node, ldapData);

                        listNode.Add(node);
                    }
                }
            }
            foreach (var node in nodeSelected)
            {
                var parentNodes = new List<DirectoryNode>();
                GetParentDirectoryNodes(parentNodes, node);
                parentNodes.RemoveAt(0);
                foreach (var pn in parentNodes)
                {
                    if (!NodeIdExpand.Contains(pn.GuidString))
                    {
                        NodeIdExpand.Add(pn.GuidString);
                    }
                }
            }
            TreeDataSource = listNode;
        }

        private void MakeLDAPSubTree(DirectoryNode parentNode, LDAPDirectoriesEndpoints ldapData)
        {
            // find all children of parent node (they have parentId = id of parent node)
            var nodes = new List<DirectoryNode>();

            if (ldapData != null)
            {
                nodes = ldapData.Directories.Where(e => e.ParentId == parentNode.GuidString)
                    .Select(e => new DirectoryNode
                    {
                        GuidString = e.Id,
                        Title = e.FolderName,
                        IsFolder = true,
                        NodeColor = "#000000",
                        NodeWidth = 350
                    }).OrderBy(o => o.Title).ToList();


                // build subtree for each child and add it in parent's children collection
                foreach (var node in nodes)
                {
                    MakeLDAPSubTree(node, ldapData);
                    parentNode.DirectoryNodes.Add(node);
                }

                // find all children of parent node (they have parentId = id of parent node)
                var nodes2 = ldapData.Endpoints.Where(
                    e => e.LDAPDirectoryId != null && e.LDAPDirectoryId == parentNode.GuidString)
                    .Select(
                        e =>
                            new DirectoryNode
                            {
                                GuidString = e.Id,
                                Title = e.SystemName,
                                IsFolder = false,
                                ComputerType = e.ComputerType,
                                NodeWidth = 350,
                                Managed = e.Managed,
                                NodeColor = "#000000"
                            }).OrderBy(o => o.Title);
                foreach (var node in nodes2)
                {
                    parentNode.DirectoryNodes.Add(node);
                }
            }
        }

        public void RefreshTreeData(List<DirectoryNode> nodeSelected)
        {
            _view.DataTree.SelectedDataItems = null;
            SetNodeExpandedState(_view.DataTree.Nodes, nodeSelected);
        }

        public List<string> NodeIdExpand = new List<string>(); 
        public List<DirectoryNode> NodesSelected = new List<DirectoryNode>(); 
        /// <summary>
        ///     Sets the state of the node expanded.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="expandNode">if set to <c>true</c> [expand node].</param>
        private void SetNodeExpandedState(IEnumerable<XamDataTreeNode> nodes, List<DirectoryNode> nodeSelected)
        {
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null)
                {
                    if (NodeIdExpand.Contains(data.GuidString))
                    {
                        item.IsExpanded = true;
                    }
                    if (nodeSelected.Select(r=>r.GuidString).Contains(data.GuidString))
                    {
                        item.IsSelected = true;
                    }
                    
                }
                
                SetNodeExpandedState(item.Nodes, nodeSelected);
            }
        }

        /// <summary>
        ///     Find all notes that has the parent node in selected nodes
        /// </summary>
        /// <returns></returns>
        private List<DirectoryNode> FindChildSelectedNode()
        {
            var listResult = new List<DirectoryNode>();
            foreach (var node in ApplicationContext.SelectedTargetNodes)
            {
                var parentNodes = new List<DirectoryNode>();
                GetParentDirectoryNodes(parentNodes, node);
                parentNodes.RemoveAt(0);

                listResult.AddRange(from node2 in ApplicationContext.SelectedTargetNodes
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
            var ldapDirs = _ldapDirectoriesEndpoints.Directories;
            var ldapEnds = _ldapDirectoriesEndpoints.Endpoints;
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
                            var dn = new DirectoryNode {IsFolder = true, GuidString = ep.Id};
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
                    if (edir != null)
                    {
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
}