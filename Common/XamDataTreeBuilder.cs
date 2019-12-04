using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Utils;

namespace Tabidus.POC.GUI.Common
{
    /// <summary>
    /// Class XamDataTreeBuilder.
    /// </summary>
    public class XamDataTreeBuilder
    {
        /// <summary>
        /// The _current directory node
        /// </summary>
        private static DirectoryNode _currentDirectoryNode;
        /// <summary>
        /// Makes the tree.
        /// </summary>
        /// <param name="folderId">The folder identifier.</param>
        /// <param name="currentDirectoryNode">The current directory node.</param>
        /// <returns>ObservableCollection&lt;DirectoryNode&gt;.</returns>
        public static ObservableCollection<DirectoryNode> MakeTree(int folderId, out DirectoryNode currentDirectoryNode, TreeViewSelectMode selectMode = TreeViewSelectMode.All)
        {
            var directoryComputerRequest = new DirectoryComputerRequest();
            var allDirectoryComputer =
                ServiceManager.Invoke(
                    sc =>
                        RequestResponseUtils.GetData<List<DirectoryComputerItem>>(sc.GetAllDirectoryComputer,
                            directoryComputerRequest));
            _currentDirectoryNode = null;
            if (selectMode == TreeViewSelectMode.Folder)
                allDirectoryComputer = (allDirectoryComputer.Where(a => a.IsDirectory)).ToList();
            else if(selectMode==TreeViewSelectMode.Endpoint)
                allDirectoryComputer = (allDirectoryComputer.Where(a => !a.IsDirectory)).ToList();
            ApplicationContext.ImportNodes = new ObservableCollection<DirectoryNode>();
            var result = BuildTree(allDirectoryComputer, 0, folderId);
            currentDirectoryNode = _currentDirectoryNode;
            return result;
        }
       /// <summary>
        /// Appends the tree.
        /// </summary>
        /// <param name="directoryNode">The directory node.</param>
        /// <param name="listDirectoryComputerItems">The list directory computer items.</param>
        /// <param name="level">The level.</param>
        public static void AppendTree(DirectoryNode directoryNode,DirectoryComputerItem directoryComputerItem,
            IEnumerable<DirectoryComputerItem> listDirectoryComputerItems, ref bool hasNewItem, int level = 1)
        {
            var items = listDirectoryComputerItems.Where(c => c.Level == level && c.ParentPath == directoryComputerItem.ParentPath + directoryComputerItem.Name).OrderBy(i=>i.Name);
            foreach (var item in items)
            {
                var childNode = directoryNode.DirectoryNodes.FirstOrDefault(c => c.Title == item.Name);
                //childNode.LDAPIconPath = null;
                if (childNode == null&&item.Id==0)
                {
                    childNode = new DirectoryNode
                    {
                        NodeId = item.Id,
                        Title = item.Name,
                        IsFolder = item.IsDirectory,
                        //NodeColor = "#264A50",
                        NodeColor = "#FFF",
                        IsExpanded = true,
                      
                        
                    };
                    ApplicationContext.ImportNodes.Add(childNode);
                    directoryNode.IsExpanded = true;
                    directoryNode.DirectoryNodes.Add(childNode);
                    hasNewItem = true;
                }
                if (item.IsDirectory)
                {
                    AppendTree(childNode, item, listDirectoryComputerItems,ref hasNewItem, level + 1);
                }
            }
            
        }

        public static void ExpandedPathNode(DirectoryNode currentDirectoryNode)
        {
            currentDirectoryNode.IsExpanded = true;
            ApplicationContext.ImportNodesExpanded.Add(currentDirectoryNode.NodeId);
            foreach (var node in ApplicationContext.ImportNodes)
            {
                if (node.DirectoryNodes.Contains(currentDirectoryNode))
                {
                    node.IsExpanded = true;
                    ExpandedPathNode(node);
                    break;
                }
            }

        }
        public static void ExpandedPathNode(DirectoryNode currentDirectoryNode, DirectoryNode tagetNode)
        {
            if (currentDirectoryNode == tagetNode) return;
           
            currentDirectoryNode.IsExpanded = true;
            ApplicationContext.ImportNodesExpanded.Add(currentDirectoryNode.NodeId);
            foreach (var node in ApplicationContext.ImportNodes)
            {
                if (node.DirectoryNodes.Contains(currentDirectoryNode))
                {
                    node.IsExpanded = true;
                    ExpandedPathNode(node, tagetNode);
                    break;
                }
            }

        }
        public static void ExpandedChildNode(DirectoryNode currentDirectoryNode)
        {
            foreach (var node in currentDirectoryNode.DirectoryNodes)
            {
                if (node.IsExpanded)
                {
                    currentDirectoryNode.IsExpanded = true;
                    ExpandedPathNode(currentDirectoryNode,ApplicationContext.NodesSelected.FirstOrDefault());
                }
                ExpandedChildNode(node);
            }

        }
        public static void SetNodeExpandedState(IEnumerable<XamDataTreeNode> nodes)
        {
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null)
                {
                    if (ApplicationContext.ImportNodesExpanded != null && ApplicationContext.ImportNodesExpanded.Contains(data.NodeId))
                    {
                        item.IsExpanded = true;
                    }

                }

                SetNodeExpandedState(item.Nodes);
            }
        }
        /// <summary>
        /// Builds the tree.
        /// </summary>
        /// <param name="alldDirectoryComputerItems">The alld directory computer items.</param>
        /// <param name="level">The level.</param>
        /// <param name="folderId">The folder identifier.</param>
        /// <param name="parentId">The parent identifier.</param>
        /// <returns>ObservableCollection&lt;DirectoryNode&gt;.</returns>
        private static ObservableCollection<DirectoryNode> BuildTree(IEnumerable<DirectoryComputerItem> alldDirectoryComputerItems, 
                                                                        int level, int folderId, int? parentId = null)
        {
            var allNode = alldDirectoryComputerItems.Where(c => c.Level == level && (parentId == null || parentId == c.ParentId));
            var listDirectoryNode = new ObservableCollection<DirectoryNode>();
            foreach (var node in allNode)
            {
                var directoryNode = new DirectoryNode
                {
                    NodeId = node.Id,
                    Title = node.Name,
                    IsFolder = node.IsDirectory,
                    DirectoryNodes = BuildTree(alldDirectoryComputerItems, level + 1, folderId, node.Id)
                };
                if (directoryNode.IsFolder)
                {
                    listDirectoryNode.Add(directoryNode);
                    ApplicationContext.ImportNodes.Add(directoryNode);
                }
                if (_currentDirectoryNode == null && node.IsDirectory && folderId == node.Id)
                {
                    _currentDirectoryNode = directoryNode;
                }
            }
            return listDirectoryNode;
        }

    }
}
