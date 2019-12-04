using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Infragistics.DragDrop;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.LDAP;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls.Discovery;
using Tabidus.POC.GUI.ViewModel.Discovery;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for EndPointListPage.xaml
    /// </summary>
    public partial class LDAPPage : Page
    {
        // The CheckBoxes in the XamDataGrid execute this command when clicked.
        public static readonly string KeyEncryption = Functions.GetConfig("MESSAGE_KEY", "");

        public LDAPPage()
        {
            InitializeComponent();
            Model = new LDAPViewModel(this);
            ApplicationContext.LdapExpandedIdList = new List<string>();
        }

        public LDAPViewModel Model
        {
            get { return DataContext as LDAPViewModel; }
            set { DataContext = value; }
        }

        private void DataTreeLDAP_OnInitializeNode(object sender, InitializeNodeEventArgs e)
        {
            var data = e.Node.Data as DirectoryNode;
            if (data != null)
            {
                e.Node.IsExpanded = false;
            }
        }

        public void MoveNotes(ObservableCollection<DirectoryNode> listMoves, string guid, int parentId, bool isUseMoveButton = false)
        {
            var workers = new List<BackgroundWorker>();
            foreach (var dn in listMoves)
            {
                var listMove = new ObservableCollection<DirectoryNode>();
                listMove.Add(dn);
                var dirs = new List<DirectoryNode>();
                DataFromNode(dirs, listMove, guid, parentId);
                foreach (var dir in dirs)
                {
                    if (!dir.IsFolder)
                    {
                        var isExisted = ApplicationContext.EndPointListAll.Select(r => r.SystemName).Contains(dir.Title);
                        if (isExisted)
                        {
                            var messageDialog =
                            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                            messageDialog.ShowMessageDialog("Endpoint can not be moved because it is already Existed", "Message");
                            if (!isUseMoveButton)
                            {
                                ApplicationContext.IsReloadForRefresh = true;
                                var rootId = ApplicationContext.FolderListAll.Select(r => r.FolderId).Min();
                                Model.MakeTree(rootId);
                                var domains = PnlDomainContainer.Children;
                                foreach (var domain in domains)
                                {
                                    if (domain.GetType() == typeof(LDAPDomainExpanderElement))
                                    {
                                        var expander = domain as LDAPDomainExpanderElement;
                                        if (expander.Model.IsActived)
                                        {
                                            var ldap = new LDAP { Id = expander.Model.Id, Domain = expander.Model.DomainName, ShowEndpoints = expander.Model.IsShowEndpoints, ShowFolders = expander.Model.IsShowFolders, HideEmptyFolders = expander.Model.IsHideEmptyFolders, HideManagedEndpoints = expander.Model.IsHideManagedEndPoints };
                                            Model.BuildLDAPTree(ldap);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                PageNavigatorHelper.GetMainModel().HideMessage();
                            }

                            return;
                        }
                    }
                }
            }
            foreach (var dn in listMoves)
            {
                var listMove = new ObservableCollection<DirectoryNode>();
                listMove.Add(dn);
                var dirs = new List<DirectoryNode>();
                DataFromNode(dirs, listMove, guid, parentId);
                foreach (var dir in dirs)
                {
                    if (!dir.IsFolder)
                    {
                        if (
                            ApplicationContext.LdapDirectoriesEndpointsDictionary.ContainsKey(
                                ApplicationContext.LDAPActived.Id))
                        {
                            var ldapDirEnd = ApplicationContext.LdapDirectoriesEndpointsDictionary[ApplicationContext.LDAPActived.Id];
                            var endLDAP = ldapDirEnd.Endpoints.FindAll(r => r.SystemName == dir.Title);
                            foreach (var end in endLDAP)
                            {
                                end.Managed = true;
                            }
                        }
                    }
                }
                var addBkg = new BackgroundWorker();
                addBkg.DoWork += AddBkg_DoWork;
                workers.Add(addBkg);
                addBkg.RunWorkerCompleted+= delegate (object s, RunWorkerCompletedEventArgs arg)
                {
                    Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        var worker = s as BackgroundWorker;
                        workers.Remove(worker);
                        if (workers.Count == 0)
                        {
                            var cdomains = PnlDomainContainer.Children;
                            foreach (var domain in cdomains)
                            {
                                if (domain.GetType() == typeof(LDAPDomainExpanderElement))
                                {
                                    var expander = domain as LDAPDomainExpanderElement;
                                    if (expander.Model.IsActived)
                                    {
                                        var ldap = new LDAP { Id = expander.Model.Id, Domain = expander.Model.DomainName, ShowEndpoints = expander.Model.IsShowEndpoints, ShowFolders = expander.Model.IsShowFolders, HideEmptyFolders = expander.Model.IsHideEmptyFolders, HideManagedEndpoints = expander.Model.IsHideManagedEndPoints };
                                        Model.BuildLDAPTree(ldap);
                                        break;
                                    }
                                }
                            }
                            var rootId = ApplicationContext.FolderListAll.Select(r => r.FolderId).Min();
                            ApplicationContext.IsReloadForRefresh = true;
                            Model.MakeTree(rootId, false, "", false, true);
                            PageNavigatorHelper.GetMainModel().HideMessage();
                        }
                    }));

                };
                addBkg.RunWorkerAsync(dirs);
            }
            
        }

        private void DataTreeLDAP_OnNodeDragEnd(object sender, DragDropEventArgs e)
        {
            var dropTarget = e.DropTarget;
            if (dropTarget != null && dropTarget.GetType() == typeof (XamDataTreeNodeControl))
            {
                var dropData = (dropTarget as XamDataTreeNodeControl).Node.Data as DirectoryNode;
                var dragData = (e.Data as XamDataTreeNode).Data as DirectoryNode;
                var listDrag = new ObservableCollection<DirectoryNode>();
                listDrag.Add(dragData);
                MoveNotes(listDrag, dropData.Guid.ToString(), dropData.NodeId);
            }
        }

        private void AddBkg_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(10);
            var listDir = e.Argument as List<DirectoryNode>;
            if (listDir != null && listDir.Count > 0)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action) (() =>
                    {
                        ServiceManager.Invoke(sc => RequestResponseUtils.GetData<StringAuthenticateObject>(
                                sc.AddFolderComputerFromLDAP,
                                listDir));
                    }));
            }
        }

        private void DataFromNode(List<DirectoryNode> dirs, ObservableCollection<DirectoryNode> nodes, string parentGuid,
            int parentId)
        {
            foreach (var dn in nodes)
            {
                if (dn.IsFolder)
                {
                    dirs.Add(new DirectoryNode
                    {
                        IsFolder = true,
                        Title = dn.Title,
                        GuidString = dn.GuidString,
                        ParentId = parentId,
                        ParentGuid = parentGuid
                    });
                    DataFromNode(dirs, dn.DirectoryNodes, dn.GuidString, dn.NodeId);
                }
                else
                {
                    dirs.Add(new DirectoryNode
                    {
                        IsFolder = false,
                        Title = dn.Title,
                        GuidString = dn.GuidString,
                        ParentId = parentId,
                        ParentGuid = parentGuid
                    });
                }
            }
        }

        private void DataTreeLDAP_OnNodeExpansionChanged(object sender, NodeExpansionChangedEventArgs e)
        {
            var nodedata = e.Node.Data as DirectoryNode;
            
            if (nodedata != null)
            {
                if (e.Node.IsExpanded)
                {
                    if (!ApplicationContext.LdapExpandedIdList.Contains(nodedata.GuidString))
                    {
                        ApplicationContext.LdapExpandedIdList.Add(nodedata.GuidString);
                    }

                }
                else
                {
                    if (ApplicationContext.LdapExpandedIdList.Contains(nodedata.GuidString))
                    {
                        ApplicationContext.LdapExpandedIdList.Remove(nodedata.GuidString);
                    }
                }
            }
        }

        private void DataTreeLDAP_OnSelectedNodesCollectionChanged(object sender, NodeSelectionEventArgs e)
        {
            if (e.CurrentSelectedNodes.Count < 1)
            {
                ApplicationContext.LDAPNodesSelected = new List<DirectoryNode>();
                
            }
            else
            {
                var ln = e.CurrentSelectedNodes.Select(cn => cn.Data).OfType<DirectoryNode>().ToList();
                ApplicationContext.LDAPNodesSelected = ln;
            }
        }
    }
}