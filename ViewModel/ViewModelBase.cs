using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using log4net;
using Newtonsoft.Json;
using Tabidus.POC.Common;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.POCAgent;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public static readonly ILog Logger = LogManager.GetLogger(typeof(ViewModelBase));
        public static readonly string KeyEncryption = Functions.GetConfig("MESSAGE_KEY", "");

        /// <summary>
        /// The variable to store the last reresh time
        /// </summary>
        public virtual void Refresh()
        {

        }
        protected List<LastUpdated> LastRefresh = new List<LastUpdated>();
            
        /// <summary>
        ///     Make tree data
        /// </summary>
        /// <returns></returns>
        public void MakeTree(int nodeSelected, bool isSearch = false, string searchKey = "", bool isExpandAll=false, bool isReload=false)
        {            
            if (isSearch)
            {
                GetSearchedData(searchKey);
                
            }
            else if(isReload)
            {
                ApplicationContext.FolderListAll = GetAllFolders();
                ApplicationContext.EndPointListAll = GetDirectoryEndpoints(1);
                ApplicationContext.FolderListTree = ApplicationContext.FolderListAll;
                ApplicationContext.EndPointListTree = ApplicationContext.EndPointListAll;
            }
            var rootId = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r => r.FolderId == ApplicationContext.FolderListAll.Min(m => m.FolderId)).FolderId : ApplicationContext.FolderListTree.Min(r => r.FolderId);
            var rootName = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r => r.FolderId == ApplicationContext.FolderListAll.Min(m => m.FolderId)).FolderName : ApplicationContext.FolderListTree.Find(r => r.FolderId == rootId).FolderName;
            var guid = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r => r.FolderId == ApplicationContext.FolderListAll.Min(m => m.FolderId)).Guid : ApplicationContext.FolderListTree.Find(r => r.FolderId == rootId).Guid;
            var mainVm = PageNavigatorHelper.GetMainModel();
            var isPocAgent = mainVm.NavigationIndex == (int)NavigationIndexes.POCAgent;
            var rootPolicy = isPocAgent ? ApplicationContext.FolderPolicyList.Find(r => r.ObjectId == rootId) : null;
            // create root, build subtree and return it
            var node = new DirectoryNode
            {
                NodeId = rootId,
                Title = rootName,
                IsFolder = true,
                Guid = guid,
                NodeWidth = ApplicationContext.GridRightOriginalWidth,
                NodeHoverWidth = isPocAgent ? 0 : ApplicationContext.GridRightOriginalWidth,
                NodeSelectedWidth = isPocAgent ? ApplicationContext.GridRightOriginalWidth : 0,
                NodePolicyColor = isPocAgent ? (rootPolicy != null ? rootPolicy.Color : UIConstant.PolicyDefaultColor) : "",
                ChildrenPolicyColor = isPocAgent ? GetChildPolicyAssign(new Directory { FolderId = rootId }) : null,
                NodeColor = "#FFF"
            };
            MakeSubTree(node, isPocAgent);
            var listNode = new ObservableCollection<DirectoryNode>();
            listNode.Add(node);
            var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
            if (rightViewModel != null)
            {
                rightViewModel.EndEditingTree();
                rightViewModel.TreeDataSource = listNode;
                if (isExpandAll)
                {
                    ApplicationContext.ExpandedIds = new List<int>();
                    ApplicationContext.ExpandedIds.Add(ApplicationContext.FolderListAll.Select(c => c.FolderId).ToList().Min());
                }
                rightViewModel.RefreshTreeData(nodeSelected);
            }

        }
        public void MakeTree(DirectoryNode nodeSelected, bool isSearch = false, string searchKey = "", bool isExpandAll = false, bool isEdited=false, bool isReload=false)
        {
            if (isSearch)
            {
                GetSearchedData(searchKey);
            }
            else if(isReload)
            {
                ApplicationContext.FolderListAll = GetAllFolders();
                ApplicationContext.EndPointListAll = GetDirectoryEndpoints(1);
                ApplicationContext.FolderListTree = ApplicationContext.FolderListAll;
                ApplicationContext.EndPointListTree = ApplicationContext.EndPointListAll;
            }
            var rootId = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r => r.FolderId == ApplicationContext.FolderListAll.Min(m => m.FolderId)).FolderId : ApplicationContext.FolderListTree.Min(r => r.FolderId);
            var rootName = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r => r.FolderId == ApplicationContext.FolderListAll.Min(m => m.FolderId)).FolderName : ApplicationContext.FolderListTree.Find(r => r.FolderId == rootId).FolderName;
            var guid = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r => r.FolderId == ApplicationContext.FolderListAll.Min(m => m.FolderId)).Guid : ApplicationContext.FolderListTree.Find(r => r.FolderId == rootId).Guid;
            var mainVm = PageNavigatorHelper.GetMainModel();
            var isPocAgent = mainVm.NavigationIndex == (int)NavigationIndexes.POCAgent;
            var rootPolicy = isPocAgent ? ApplicationContext.FolderPolicyList.Find(r => r.ObjectId == rootId) : null;
            // create root, build subtree and return it
            var node = new DirectoryNode
            {
                NodeId = rootId,
                Title = rootName,
                IsFolder = true,
                NodeWidth = ApplicationContext.GridRightOriginalWidth,
                Guid = guid,
                NodeHoverWidth = isPocAgent ? 0 : ApplicationContext.GridRightOriginalWidth,
                NodeSelectedWidth = isPocAgent ? ApplicationContext.GridRightOriginalWidth : 0,
                NodePolicyColor = isPocAgent ? (rootPolicy != null ? rootPolicy.Color : UIConstant.PolicyDefaultColor) : "",
                ChildrenPolicyColor = isPocAgent ? GetChildPolicyAssign(new Directory { FolderId = rootId }) : null,
                NodeColor = "#FFF"
            };
            MakeSubTree(node, isPocAgent);
            var listNode = new ObservableCollection<DirectoryNode>();
            listNode.Add(node);
            var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
            if (rightViewModel != null)
            {
                rightViewModel.EndEditingTree();
                rightViewModel.TreeDataSource = listNode;
                if (isExpandAll)
                {
                    ApplicationContext.ExpandedIds = ApplicationContext.FolderListAll.Select(c => c.FolderId).ToList();
                }
                rightViewModel.RefreshTreeData(nodeSelected, isEdited);
            }

        }

        public void MakeTree(List<DirectoryNode> nodeSelected, bool isSearch = false, string searchKey = "", bool isReload=false)
        {
            if (isSearch)
            {
                GetSearchedData(searchKey);
                
            }
            else if(isReload)
            {
                ApplicationContext.FolderListAll = GetAllFolders();
                ApplicationContext.EndPointListAll = GetDirectoryEndpoints(1);
                ApplicationContext.FolderListTree = ApplicationContext.FolderListAll;
                ApplicationContext.EndPointListTree = ApplicationContext.EndPointListAll;
            }
            var rootId = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r => r.FolderId == ApplicationContext.FolderListAll.Min(m => m.FolderId)).FolderId : ApplicationContext.FolderListTree.Min(r => r.FolderId);
            var rootName = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r => r.FolderId == ApplicationContext.FolderListAll.Min(m => m.FolderId)).FolderName : ApplicationContext.FolderListTree.Find(r => r.FolderId == rootId).FolderName;
            var guid = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r => r.FolderId == ApplicationContext.FolderListAll.Min(m => m.FolderId)).Guid : ApplicationContext.FolderListTree.Find(r => r.FolderId == rootId).Guid;
            var mainVm = PageNavigatorHelper.GetMainModel();
            var isPocAgent = mainVm.NavigationIndex == (int)NavigationIndexes.POCAgent;
            var rootPolicy = isPocAgent ? ApplicationContext.FolderPolicyList.Find(r => r.ObjectId == rootId) : null;
            // create root, build subtree and return it
            var node = new DirectoryNode
            {
                NodeId = rootId,
                Title = rootName,
                IsFolder = true,
                NodeWidth = ApplicationContext.GridRightOriginalWidth,
                Guid = guid,
                NodeHoverWidth = isPocAgent ? 0 : ApplicationContext.GridRightOriginalWidth,
                NodeSelectedWidth = isPocAgent ? ApplicationContext.GridRightOriginalWidth : 0,
                NodePolicyColor = isPocAgent ? (rootPolicy != null ? rootPolicy.Color : UIConstant.PolicyDefaultColor) : "",
                ChildrenPolicyColor = isPocAgent ? GetChildPolicyAssign(new Directory { FolderId = rootId }) : null
            };
            MakeSubTree(node,isPocAgent);
            var listNode = new ObservableCollection<DirectoryNode>();
            listNode.Add(node);
            var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
            if (rightViewModel != null)
            {
                rightViewModel.EndEditingTree();
                rightViewModel.TreeDataSource = listNode;
                
                rightViewModel.RefreshTreeData(nodeSelected);
            }

        }
        

        public void ReBuildTree(List<DirectoryNode> nodeSelected)
        {
            var mainVm = PageNavigatorHelper.GetMainModel();
            var isPocAgent = mainVm.NavigationIndex == (int) NavigationIndexes.POCAgent;
            var rootId = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r => r.FolderId == ApplicationContext.FolderListAll.Min(m => m.FolderId)).FolderId : ApplicationContext.FolderListTree.Min(r => r.FolderId);
            var rootName = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r=>r.FolderId== ApplicationContext.FolderListAll.Min(m=>m.FolderId)).FolderName : ApplicationContext.FolderListTree.Find(r => r.FolderId == rootId).FolderName;
            var guid = ApplicationContext.FolderListTree == null || (ApplicationContext.FolderListTree != null && ApplicationContext.FolderListTree.Count == 0) ? ApplicationContext.FolderListAll.Find(r => r.FolderId == ApplicationContext.FolderListAll.Min(m => m.FolderId)).Guid : ApplicationContext.FolderListTree.Find(r => r.FolderId == rootId).Guid;
            var rootPolicy = isPocAgent ? ApplicationContext.FolderPolicyList.Find(r => r.ObjectId == rootId) : null;
            // create root, build subtree and return it
            var node = new DirectoryNode
            {
                NodeId = rootId,
                Title = rootName,
                IsFolder = true,
                NodeWidth = ApplicationContext.GridRightOriginalWidth,
                Guid = guid,
                NodeHoverWidth = isPocAgent?0:ApplicationContext.GridRightOriginalWidth,
                NodeSelectedWidth = isPocAgent? ApplicationContext.GridRightOriginalWidth : 0,
                NodePolicyColor = isPocAgent? (rootPolicy!=null?rootPolicy.Color: UIConstant.PolicyDefaultColor) : "",
                ChildrenPolicyColor = isPocAgent? GetChildPolicyAssign(new Directory { FolderId = rootId }): null
            };
            MakeSubTree(node, isPocAgent);
            var listNode = new ObservableCollection<DirectoryNode>();
            listNode.Add(node);
            var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
            if (rightViewModel != null)
            {
                rightViewModel.EndEditingTree();
                rightViewModel.TreeDataSource = listNode;
                rightViewModel.RefreshTreeData(nodeSelected);
            }

        }

        /// <summary>
        /// Add Folder or Computer to current Tree Object
        /// </summary>
        /// <param name="newNode"></param>
        /// <param name="isEdited"></param>
        public void AddNodeToCurrentTree(DirectoryNode newNode, bool isEdited=false, DirectoryNode dir =null)
        {
            var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
            if (dir == null)
            {
                rightViewModel.EndEditingTree();
                var currentSelectedNode = ApplicationContext.NodesSelected.First();
                var listNode = new List<DirectoryNode> { newNode };
                ApplicationContext.NodesSelected = listNode;
                currentSelectedNode.DirectoryNodes.Add(newNode);
                rightViewModel.RefreshTreeAfterAdded(newNode);
            }
            else
            {
                dir.DirectoryNodes.Add(newNode);
            }
            
            
        }

        /// <summary>
        /// Select current tree node to refresh data for groupview screen
        /// </summary>
        public void SelectCurrentTreeNode(DirectoryNode curnode)
        {
            var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
            rightViewModel.EndEditingTree();
            rightViewModel.RefreshTreeData(curnode);
           // ResortTreeNode(curnode);
        }

        public object ResortTreeNode(DirectoryNode node)
        {
            var cu = ApplicationContext.NodesSelected.FirstOrDefault(); 
            DirectoryNode rootNode = PageNavigatorHelper.GetRightElementViewModel().TreeDataSource[0];
            var parentNode = GetTreeParentNode(rootNode, cu);
            if (parentNode != null)
            {
               var childNodes= parentNode.DirectoryNodes.OrderBy(n => n.Title);
               parentNode.DirectoryNodes.Clear();
                
            }
            return null;
        }

        DirectoryNode GetTreeParentNode(DirectoryNode dataSourceTree,DirectoryNode childNode)
        {
           foreach (var data in dataSourceTree.DirectoryNodes)
           {
               if (data == childNode)
               {
                   return dataSourceTree;              
               }
               DirectoryNode parentNode= GetTreeParentNode(data, childNode);
               if (parentNode != null)
                   return parentNode;
           }
            return null;
        }
        public void GetBelowNode(DirectoryNode currentNode)
       {
            var belowNode=new DirectoryNode();
            var parentNode = GetParentNode(currentNode);
            if (parentNode == null)
            {
                var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
                if (rightViewModel != null)
                {
                    ApplicationContext.BelowNode = rightViewModel.TreeDataSource.FirstOrDefault();
                }
                else ApplicationContext.BelowNode = belowNode;
                return;
            }
            var sameLvlNodes = GetSameLvlNodes(parentNode,currentNode);
            if (sameLvlNodes.Count == 0)
               belowNode = parentNode;
            else
            {
               belowNode = sameLvlNodes[0];
            }
           ApplicationContext.BelowNode = belowNode;

       }

        List<DirectoryNode> GetSameLvlNodes(DirectoryNode parentNode,DirectoryNode curentNode)
        {          
            List<DirectoryNode> sameLvlNodes=new List<DirectoryNode>();
            var folderNodes = (from dir in ApplicationContext.FolderListTree
                where dir.ParentId == parentNode.NodeId
                select dir).ToList();

            var endPointNodes= (from dir in ApplicationContext.EndPointListTree
                where dir.FolderId == parentNode.NodeId
                select dir).ToList();

            foreach (var folderNode in folderNodes)
            {
                if (!IsFolderOnSelectedNodes(folderNode))
                {
                    var nodeConvert = new DirectoryNode
                    {
                        NodeId = folderNode.FolderId,
                        Title = folderNode.FolderName,
                        IsFolder = true,
                        NodeColor = CommonConstants.DEFAULT_TEXT_COLOR
                    };
                    sameLvlNodes.Add(nodeConvert);
                }
                
            }
            foreach (var endPointNode in endPointNodes)
            {

                if (!IsEndpointOnSelectedNodes(endPointNode))
                {
                    var nodeConvert= new DirectoryNode
                        {
                            NodeId = endPointNode.EndpointId,
                            Title = endPointNode.SystemName,
                            IsFolder = false,
                            ComputerType = (endPointNode.ComputerType == "server" ? 0 : endPointNode.ComputerType == "desktop" ? 1 : 2),
                            PowerState = (endPointNode.PowerState == "offline" ? 0 : 1),
                            IsNoAgent = string.IsNullOrEmpty(endPointNode.ID),
                            //NodeColor = (e.PowerState == "offline" ? CommonConstants.GREEN_OFFLINE_COLOR : CommonConstants.GREEN_ONLINE_COLOR)
                            NodeColor = "F00"
                        };
                    sameLvlNodes.Add(nodeConvert);
                }
              
            }    
     
            return sameLvlNodes;
        }

        private static bool IsEndpointOnSelectedNodes(EndPoint endPoint)
        {
            if (ApplicationContext.NodesSelected == null) return true;
            var node = (from selectedNode in ApplicationContext.NodesSelected
                        where selectedNode.NodeId == endPoint.EndpointId && !selectedNode.IsFolder
                select selectedNode).ToList();
            if (node.Count != 0) return true;
            return false;
        }

        private static bool IsFolderOnSelectedNodes(Directory folderNode)
        {
            if (ApplicationContext.NodesSelected == null) return true;
            var node = (from selectedNode in ApplicationContext.NodesSelected
                where selectedNode.NodeId == folderNode.FolderId && selectedNode.IsFolder
                select selectedNode).ToList();
            if (node.Count != 0) return true;
            return false;

        }

        DirectoryNode GetParentNode(DirectoryNode currentNode)
        {
            DirectoryNode parentNode=new DirectoryNode();
            if (ApplicationContext.FolderListTree.Count > 0)
            {
                int parentId = 0;
                if (currentNode.IsFolder)
                {
                    var curData = ApplicationContext.FolderListTree.Find(e => e.Guid == currentNode.Guid);
                    if (curData != null)
                    {
                        parentId = curData.ParentId ??
                               0;
                    }
                    
                }
                else
                {
                    var curData = ApplicationContext.EndPointListTree.Find(e => e.Guid == currentNode.Guid);
                    if (curData != null)
                    {
                        parentId =curData.FolderId ?? 0;
                    }
                    
                }
                if (parentId == 0) return null;
                var node = ApplicationContext.FolderListTree.Find(
                    e => e.FolderId == parentId);
                if (node != null)
                {
                    parentNode = new DirectoryNode
                    {
                        NodeId = node.FolderId,
                        Title = node.FolderName,
                        IsFolder = true,
                        NodeColor = CommonConstants.DEFAULT_TEXT_COLOR
                    };
                    return parentNode;
                }
                return null;
            }
            
            return null;
        }

        private string GetPolicyColorOfDirectory(Directory d)
        {
            var rootPolicy = ApplicationContext.FolderPolicyList.Find(r => r.ObjectId == d.FolderId);
            return rootPolicy == null ? UIConstant.PolicyDefaultColor : rootPolicy.Color;
        }
        private string GetPolicyColorOfEndpoint(EndPoint e)
        {
            var rootPolicy = ApplicationContext.EndpointPolicyList.Find(r => r.ObjectId == e.EndpointId);
            return rootPolicy == null ? UIConstant.PolicyDefaultColor : rootPolicy.Color;
        }

        private List<PolicyAssign> GetChildPolicyAssign(Directory dir)
        {
            var childList = new List<PolicyAssign>();
            var parentPolicyColor = GetPolicyColorOfDirectory(dir);
            FindChildPolicyAssign(dir, childList, parentPolicyColor);
            return childList;
        }

        private void FindChildPolicyAssign(Directory parentNode, List<PolicyAssign> pal, string parentPolicyColor)
        {
            // find all children of parent node (they have parentId = id of parent node)
            var nodes = ApplicationContext.FolderListAll.Where(e => e.ParentId == parentNode.FolderId).ToList();
            // find all children of parent node (they have parentId = id of parent node)
            var nodes2 = ApplicationContext.EndPointListAll.Where(e => e.FolderId != null && e.FolderId == parentNode.FolderId)
                .ToList();
            
            foreach (var node in nodes)
            {
                var pcolor = GetPolicyColorOfDirectory(node);
                if (pcolor != UIConstant.PolicyDefaultColor && pcolor != parentPolicyColor && !pal.Select(r=>r.Color).Contains(pcolor))
                {
                    pal.Add(new PolicyAssign
                    {
                        ObjectId = node.FolderId,
                        Color = pcolor
                    });
                }
                FindChildPolicyAssign(node, pal, parentPolicyColor);
            }
            foreach (var node in nodes2)
            {
                var pcolor = GetPolicyColorOfEndpoint(node);
                if (pcolor != UIConstant.PolicyDefaultColor && pcolor != parentPolicyColor && !pal.Select(r => r.Color).Contains(pcolor))
                {
                    pal.Add(new PolicyAssign
                    {
                        ObjectId = node.EndpointId,
                        Color = pcolor
                    });
                }
            }
        }
         
        /// <summary>
        ///     Make sub tree using recursive
        /// </summary>
        /// <param name="parentNode"></param>
        private void MakeSubTree(DirectoryNode parentNode, bool isPocAgent = false)
        {
            // find all children of parent node (they have parentId = id of parent node)
            var nodes = ApplicationContext.FolderListTree.Where(e => e.ParentId == parentNode.NodeId)
                .Select(e => new DirectoryNode
                {
                    NodeId = e.FolderId, Title = e.FolderName, IsFolder = true,
                    NodeColor = CommonConstants.DEFAULT_TEXT_COLOR,
                    NodeWidth = ApplicationContext.GridRightOriginalWidth,
                    Guid = e.Guid,
                    NodeHoverWidth = isPocAgent ? 0 : ApplicationContext.GridRightOriginalWidth,
                    NodeSelectedWidth = isPocAgent ? ApplicationContext.GridRightOriginalWidth : 0,
                    NodePolicyColor = isPocAgent ? GetPolicyColorOfDirectory(e) : "",
                    ChildrenPolicyColor = isPocAgent? GetChildPolicyAssign(e) : null
                }).OrderBy(o=>o.Title);
            // find all children of parent node (they have parentId = id of parent node)
            var nodes2 = ApplicationContext.EndPointListTree.Where(
                e => e.FolderId != null && e.FolderId == parentNode.NodeId)
                .Select(
                    e =>
                        new DirectoryNode
                        {
                            NodeId = e.EndpointId,
                            Title = e.SystemName,
                            IsFolder = false,
                            ComputerType = (e.ComputerType == "server" ? 0 : e.ComputerType == "desktop" ? 1 : 2),
                            PowerState = (e.PowerState == "offline" ? 0 : 1),
                            IsNoAgent = string.IsNullOrEmpty(e.GUIID),
                            //NodeColor = (e.PowerState == "offline" ? CommonConstants.GREEN_OFFLINE_COLOR : CommonConstants.GREEN_ONLINE_COLOR)
                            NodeColor = (e.PowerState == "offline" ? CommonConstants.POWERSTATE_OFFLINE_ENDPOINT : CommonConstants.POWERSTATE_ONLINE_ENDPOINT),
                            FontColor = e.Color,
                            NodeWidth = ApplicationContext.GridRightOriginalWidth,
                            Guid = e.Guid,
                            NodeHoverWidth = isPocAgent ? 0 : ApplicationContext.GridRightOriginalWidth,
                            NodeSelectedWidth = isPocAgent ? ApplicationContext.GridRightOriginalWidth : 0,
                            NodePolicyColor = isPocAgent ? GetPolicyColorOfEndpoint(e) : ""
                        }).OrderBy(o=>o.Title);
            // build subtree for each child and add it in parent's children collection
            foreach (var node in nodes)
            {
                MakeSubTree(node, isPocAgent);
                parentNode.DirectoryNodes.Add(node);
            }
            foreach (var node in nodes2)
            {
                node.ImagePath = EndPoint.GetImages(node.ComputerType.ToString(), node.FontColor, true);
                parentNode.DirectoryNodes.Add(node);
            }
        }

        public List<EndPoint> GetDirectoryEndpoints(int folderId, string searchKey="")
        {
            try
            {
                var endPointListAll = new List<EndPoint>();

                Logger.Info("Start getting Endpoint datas in a folder with folderId =" + folderId);
                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var searchObj = new EndpointSearch {FolderId = folderId, SearchKey = searchKey};
                    var result = sc.GetDirectoryData(EncryptionHelper.EncryptString(JsonConvert.SerializeObject(searchObj), KeyEncryption));
                    var resultDeserialize =
                        JsonConvert.DeserializeObject<List<EndPointData>>(EncryptionHelper.DecryptRijndael(result,
                            KeyEncryption));
                    if (resultDeserialize == null)
                    {
                        PageNavigatorHelper._MainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                        {
                            var messageDialog = PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                            messageDialog.ShowMessageDialog("Data is null","Message");
                        }));
                        return null;
                    }

                    foreach (var epd in resultDeserialize)
                    {
                        var ep = new EndPoint(epd);
                        endPointListAll.Add(ep);
                    }
                }
                Logger.Info("End getting EndPoint datas");
                return endPointListAll;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                PageNavigatorHelper._MainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    var messageDialog = PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                    
                    messageDialog.ShowMessageDialog("Cannot get endpoint due to exception occured, please see the log file under the Logs for more information", "Message");
                }));
                return null;
            }
        }
        

        public List<Directory> GetAllFolders()
        {
            try
            {
                var folderListAll = new List<Directory>();
                Logger.Info("Start get all folders");
                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var result = sc.GetAllFolders(EncryptionHelper.EncryptString("OK", KeyEncryption));
                    var resultDeserialize =
                        JsonConvert.DeserializeObject<List<Directory>>(EncryptionHelper.DecryptRijndael(result,
                            KeyEncryption));
                    if (resultDeserialize == null)
                    {
                        PageNavigatorHelper._MainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                        {
                            var messageDialog = PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                            
                            messageDialog.ShowMessageDialog("Data is null", "Message");
                            
                        }));
                        return null;
                    }

                    foreach (var folder in resultDeserialize)
                    {
                        var fol = new Directory
                        {
                            FolderId = folder.FolderId,
                            FolderName = folder.FolderName,
                            ParentId = folder.ParentId,
                            Guid = Guid.NewGuid()
                        };
                        folderListAll.Add(fol);
                    }
                }
                Logger.Info("End getting folder datas");
                return folderListAll;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                PageNavigatorHelper._MainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    var messageDialog = PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                    
                    messageDialog.ShowMessageDialog("Cannot get directory due to exception occured, please see the log file under the Logs for more information", "Message");
                }));
                return null;
            }
        }

        /// <summary>
        /// Get searched data
        /// </summary>
        /// <param name="searchKey"></param>
        public void GetSearchedData(string searchKey)
        {
            using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            {
                var jsonSearchkey = JsonConvert.SerializeObject(searchKey);
                var encryptedJsonSearchkey = EncryptionHelper.EncryptString(jsonSearchkey, KeyEncryption);
                var jsonResult = sc.SearchEndPoint(encryptedJsonSearchkey);

                var decryptedJsonResult = EncryptionHelper.DecryptRijndael(jsonResult, KeyEncryption);
                var endPointDatas = JsonConvert.DeserializeObject<List<EndPointData>>(decryptedJsonResult);

                var endPoints = new List<EndPoint>();
                var dirs = new List<Directory>();
                
                foreach (var endPointData in endPointDatas)
                {
                    var ep = new EndPoint(endPointData);
                    endPoints.Add(ep);
                    var dir = ApplicationContext.FolderListAll.Find(d => d.FolderId == ep.FolderId);
                    if (dir != null)
                    {
                        GetParentNode(dirs, dir);
                    }
                }

                dirs.Reverse();

                ApplicationContext.EndPointListTree = endPoints;

                if (string.IsNullOrEmpty(searchKey))
                {
                    ApplicationContext.FolderListTree = ApplicationContext.FolderListAll;
                }
                else
                {
                    ApplicationContext.FolderListTree = dirs;
                }
            }
        }

        public void DeleteNodes(List<DirectoryNode> nodes)
        {
            var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();

            var listNode = new ObservableCollection<DirectoryNode>();
            var dirTreeRootId = rightViewModel.TreeDataSource.First().NodeId;
            if (nodes.Find(r => r.NodeId == dirTreeRootId && r.IsFolder) != null)
            {
                rightViewModel.TreeDataSource.First().DirectoryNodes = new ObservableCollection<DirectoryNode>();
                foreach (var node in nodes)
                {
                    RemoveOrEditLabelNode(node, false);
                }
            }
            else
            {
                foreach (var node in nodes)
                {
                    RemoveNode(node, rightViewModel.TreeDataSource, listNode);
                    RemoveOrEditLabelNode(node, false);
                }
            }
            
        }
        
        private void RemoveNode(DirectoryNode innode, ObservableCollection<DirectoryNode> searchnodes, ObservableCollection<DirectoryNode> outnodes)
        {
            for (int i= searchnodes.Count-1; i>-1; i--)
            {
                if (innode.NodeId == searchnodes[i].NodeId && innode.IsFolder == searchnodes[i].IsFolder)
                {
                    searchnodes.RemoveAt(i);
                    break;
                }

                RemoveNode(innode, searchnodes[i].DirectoryNodes, outnodes);
            }
        }
        
        public void RemoveOrEditLabelNode(DirectoryNode innode, bool isEdit)
        {
            for (int i = 0; i < ApplicationContext.LableEndpointDatas.Count; i++)
            {
                for (int j = ApplicationContext.LableEndpointDatas[i].EndPointDatas.Count-1; j > -1; j--)
                {
                    var end = ApplicationContext.LableEndpointDatas[i].EndPointDatas[j];
                    if (innode.NodeId == end.EndpointId&&!innode.IsFolder)
                    {
                        if(!isEdit)
                            ApplicationContext.LableEndpointDatas[i].EndPointDatas.RemoveAt(j);
                        else
                            end.SystemName = innode.Title;
                        break;
                    }
                }
                
            }
        }

        /// <summary>
        /// Get the parent folders of a folder
        /// </summary>
        /// <param name="listNode"></param>
        /// <param name="dir"></param>
        public void GetParentNode(List<Directory> listNode, Directory dir)
        {
            if (!listNode.Contains(dir))
            {
                listNode.Add(dir);
            }

            foreach (var ep in ApplicationContext.FolderListAll)
            {
                if (dir.ParentId == ep.FolderId && dir.FolderId != ep.ParentId) //prevent  circle recursive
                {
                    GetParentNode(listNode, ep);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Indicate that whether we can refresh screen or not
        /// </summary>
        /// <returns></returns>
        protected bool CanRefresh()
        {
            ApplicationContext.CanRefreshEndpoint = false;
            ApplicationContext.CanRefreshDiscovery = false;
            ApplicationContext.CanRefreshLDAP = false;
            ApplicationContext.CanRefreshSoftware = false;
            ApplicationContext.CanRefreshUpdateSource = false;
            var lastRefreshUpdate = ServiceManager.GetLastUpdateData();
            var canrefresh = false;
            foreach (var lastupd in lastRefreshUpdate)
            {
                foreach (var inLastUpd in LastRefresh)
                {
                    if (inLastUpd.Name == lastupd.Name && inLastUpd.LastSync < lastupd.LastSync)
                    {
                        if (lastupd.Name == "DirectoryEndpoint")
                        {
                            ApplicationContext.CanRefreshEndpoint = true;
                        }
                        if (lastupd.Name == "Discovery")
                        {
                            ApplicationContext.CanRefreshDiscovery = true;
                        }
                        if (lastupd.Name == "LDAPDirectoryEndpoint")
                        {
                            ApplicationContext.CanRefreshLDAP = true;
                        }
                        if (lastupd.Name == "Software")
                        {
                            ApplicationContext.CanRefreshSoftware = true;
                        }
                        if (lastupd.Name == "UpdateSource")
                        {
                            ApplicationContext.CanRefreshUpdateSource = true;
                        }
	                    if (lastupd.Name == "Task")
	                    {
		                    ApplicationContext.CanRefreshTask = true;
	                    }
                        canrefresh = true;
                    }
                }
            }
            
            if (canrefresh)
            {
                var mainVm = PageNavigatorHelper.GetMainModel();
                if (mainVm != null)
                {
                    ApplicationContext.IsReload = true;
                    ApplicationContext.IsReloadForRefresh = true;
                    mainVm.ReloadData();
                    LastRefresh = lastRefreshUpdate;
                }
                
            }
            
            return canrefresh;
        }
        
        #region NoitfyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #endregion
    }
}