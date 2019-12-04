using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Infragistics.Controls.Menus;
using Newtonsoft.Json;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Discovery;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Discovery
{
    public class NeighborhoodWatchMoveViewModel : ViewModelBase
    {
        private readonly NeighborhoodWatchMoveTargetDialog _view;
        private ObservableCollection<DirectoryNode> _treeDataSource;
        private List<NeighborhoodWatch> _selectedData; 

        public NeighborhoodWatchMoveViewModel(NeighborhoodWatchMoveTargetDialog view)
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

        public void SetData(List<NeighborhoodWatch> selectedData)
        {
            _selectedData = selectedData;
        }

        public ICommand MoveCommand { get; set; }

        private void OnMoveCommand(object args)
        {
            if (_selectedData.Any(neighborhood => ApplicationContext.EndPointListAll.Find(r => r.MACAddress == neighborhood.MAC)!=null))
            {
                _view.HideWindow();
                var messageDialog = PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                messageDialog.ShowMessageDialog(
                    "Cannot move, have some Endpoints are existed",
                    "Message");
                return;
            }
            var moveDiscoveryBk = new BackgroundWorker();
            moveDiscoveryBk.DoWork += MoveDiscoveryBk_DoWork;
            moveDiscoveryBk.RunWorkerCompleted += MoveDiscoveryBk_RunWorkerCompleted;
            moveDiscoveryBk.RunWorkerAsync();
            foreach (var neighborhood in _selectedData)
            {
                ApplicationContext.AllNeighborhoodWatch.Find(r => r.Id == neighborhood.Id).Managed = true;
            }
            var neighborhoodVm = PageNavigatorHelper.GetMainContentViewModel<NeighborhoodWatchViewModel>();
            if (neighborhoodVm != null)
            {
                neighborhoodVm.OnTabSelected();
            }
            _view.HideWindow();
        }

        private void MoveDiscoveryBk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ApplicationContext.IsReloadForRefresh = true;
            MakeTree(ApplicationContext.NodeTargetId);
            foreach (var nh in ApplicationContext.AllNeighborhoodWatch)
            {
                nh.IsSelected = false;
            }
        }

        private void MoveDiscoveryBk_DoWork(object sender, DoWorkEventArgs e)
        {
            var strAuth = new StringAuthenticateObject();
            var xmlDataBuilder = new StringBuilder();
            xmlDataBuilder.Append("<DataSet>");
            var indexName = GetFolderIndexAdd();
            foreach (var neighborhood in _selectedData)
            {
                var nname = neighborhood.Computer;
                if (string.IsNullOrWhiteSpace(neighborhood.Computer))
                {
                    nname = "New Computer";
                    if (indexName >= 2)
                    {
                        nname += " (" + indexName + ")";
                        indexName++;
                    }
                }
                xmlDataBuilder.Append("<Endpont>");
                xmlDataBuilder.Append("<SystemName>" + nname + "</SystemName>");
                xmlDataBuilder.Append("<FolderId>" + ApplicationContext.NodeTargetId + "</FolderId>");
                xmlDataBuilder.Append("<MacAddress>" + neighborhood.MAC + "</MacAddress>");
                xmlDataBuilder.Append("</Endpont>");
            }
            xmlDataBuilder.Append("</DataSet>");
            strAuth.StringAuth = "OK";
            strAuth.StringValue = xmlDataBuilder.ToString();
            using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            {
                var data = JsonConvert.SerializeObject(strAuth);
                var result = sc.AddNewEndpointsFromDiscovery(EncryptionHelper.EncryptString(data, KeyEncryption));
                var rsDeser = JsonConvert.DeserializeObject<List<EndPointData>>(EncryptionHelper.DecryptRijndael(result, KeyEncryption));
                var listData = new List<EndPoint>();
                foreach (var ed in rsDeser)
                {
                    var enp = new EndPoint(ed);
                    listData.Add(enp);
                }
                var ec = ApplicationContext.EndPointListAll.Count;
                ApplicationContext.EndPointListTree.RemoveAll(r => r.FolderId == ApplicationContext.NodeTargetId);
                ApplicationContext.EndPointListTree.AddRange(listData);
                if (ec == ApplicationContext.EndPointListAll.Count)
                {
                    ApplicationContext.EndPointListAll.RemoveAll(r => r.FolderId == ApplicationContext.NodeTargetId);
                    ApplicationContext.EndPointListAll.AddRange(listData);
                }
                
            }
        }

        private int GetFolderIndexAdd()
        {
            var noteSelected = ApplicationContext.NodeTargetId;
            var subNodes = ApplicationContext.EndPointListAll.Where(e => e.FolderId == noteSelected).Select(e => e);
            
            var numberCounts = new List<int>();
            foreach (var lbel in subNodes)
            {
                var header = lbel.SystemName;
                if (header.IndexOf("New computer", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var headerUp = header.ToLower();
                    string[] separatingChars = { "new computer" };
                    var nameSplit = headerUp.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);
                    if (nameSplit.Length >= 1)
                    {
                        var numberCountLocal = 0;
                        try
                        {
                            numberCountLocal = Int32.Parse(nameSplit[0].Replace("(", "").Replace(")", "").Trim());
                        }
                        catch
                        {
                            numberCountLocal = 0;
                        }
                        if (numberCountLocal > 1)
                        {
                            numberCounts.Add(numberCountLocal);
                        }
                    }
                    else
                    {
                        numberCounts.Add(1);
                    }
                }
            }
            var maxCount = 0;
            if (numberCounts.Count > 0)
            {
                numberCounts.Sort();
                foreach (var n in numberCounts)
                {
                    if (!numberCounts.Contains(n + 1))
                    {
                        maxCount = n + 1;
                        break;
                    }
                }
            }
            return maxCount;
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