using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Newtonsoft.Json;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.LDAP;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.UserControls.Discovery;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Discovery
{
    public class LDAPViewModel : PageViewModelBase
    {
        private readonly LDAPPage _view;
        private ObservableCollection<DirectoryNode> _treeDataSource;

        public LDAPViewModel(LDAPPage view)
        {
            _view = view;
            TabSelectedCommand = new RelayCommand<Button>(OnMenuSelected, CanMenuAction);
            var ldapTypes = Functions.GetConfig("LDAP_TYPES", "");
            ApplicationContext.LDAPTypes = GetLDAPTypes(ldapTypes);
            ApplicationContext.LdapDirectoriesEndpointsDictionary = new ConcurrentDictionary<int, LDAPDirectoriesEndpoints>();
            BuildPage();
        }
        public ICommand TabSelectedCommand { get; private set; }

        public ObservableCollection<DirectoryNode> LDAPTreeDataSource
        {
            get { return _treeDataSource; }
            set
            {
                _treeDataSource = value;
                OnPropertyChanged("LDAPTreeDataSource");
            }
        }
        private string _domainName;

        public string DomainName
        {
            get { return _domainName; }
            set { _domainName = value; OnPropertyChanged("DomainName");}
        }


        public override void Refresh()
        {
            BuildPage();
        }

        private List<string> GetLDAPTypes(string listText)
        {
            var typestring = new List<string>();
            ApplicationContext.LdapTypeDictionary = new Dictionary<string, int>();
            var cw = listText.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            foreach (var type in cw)
            {
                var tl = type.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                ApplicationContext.LdapTypeDictionary.Add(tl[0], int.Parse(tl[1]));
                typestring.Add(tl[0]);
            }
            return typestring;
        }

        private void BuildPage()
        {
            var ldaps = GetLDAPs();
            ApplicationContext.LDAPList = ldaps;
            var ldapExpanders = _view.PnlDomainContainer.Children;
            var listExpanded = new List<int>();
            foreach (var exp in ldapExpanders)
            {
                if (exp.GetType() == typeof(LDAPDomainExpanderElement))
                {
                    var expVm = (exp as LDAPDomainExpanderElement).Model;
                    if (expVm.IsExpanded)
                    {
                        listExpanded.Add(expVm.Id);
                    }
                }
            }
            _view.PnlDomainContainer.Children.Clear();
            var workers = new List<BackgroundWorker>();
            var ldapActive = ApplicationContext.LDAPActived;
            foreach (var ldap in ldaps)
            {
                var domain = new LDAPDomainExpanderElement();
                domain.Model.Id = ldap.Id;
                domain.Model.ComputerCount = ldap.TotalComputers;
                domain.Model.TitleName = ldap.Domain;
                domain.Model.DomainName = ldap.Domain;
                domain.Model.IsHideEmptyFolders = ldap.HideEmptyFolders;
                domain.Model.IsHideManagedEndPoints = ldap.HideManagedEndpoints;
                domain.Model.User = ldap.User;
                domain.Model.Password = ldap.Password;
                domain.Model.IsShowEndpoints = ldap.ShowEndpoints;
                domain.Model.IsShowFolders = ldap.ShowFolders;
                domain.Model.Port = ldap.Port;
                domain.Model.SyncInterval = ldap.SyncInterval;
                domain.Model.Server = ldap.Server;
                domain.Model.IsSecure = ldap.IsSecureLDAP;
                if (ldapActive == null)
                {
                    domain.Model.IsExpanded = true;
                    ldapActive = ldap;
                }
                if (ldapActive.Id == ldap.Id)
                {
                    domain.Model.IsActived = domain.Model.IsExpanded = true;
                }
                if (listExpanded.Contains(ldap.Id))
                {
                    domain.Model.IsExpanded = true;
                }
                _view.PnlDomainContainer.Children.Add(domain);
                domain.Model.LoadData(workers);
            }
        }

        public void BuildLDAPTree(LDAP ldap)
        {
            var ldapDirEnd = ApplicationContext.LdapDirectoriesEndpointsDictionary.ContainsKey(ldap.Id) ? ApplicationContext.LdapDirectoriesEndpointsDictionary[ldap.Id] : null;
            if (ldapDirEnd != null)
            {
                var directories = ldapDirEnd.Directories;
                if (directories != null && directories.Count > 0)
                {
                    var domainDir = directories.Find(r => r.FolderName == ldap.Domain);
                    if (domainDir != null)
                    {
                        var rootId = domainDir.Id;
                        var rootName = ldap.Domain;

                        // create root, build subtree and return it
                        var node = new DirectoryNode
                        {
                            GuidString = rootId,
                            Title = rootName,
                            IsFolder = true,
                            NodeWidth = ApplicationContext.GridRightOriginalWidth,
                            NodeColor = "#525963"
                        };
                        var listNode = new ObservableCollection<DirectoryNode>();
                        if (ldap.ShowFolders)
                        {
                            MakeLDAPSubTree(node, ldap);

                            listNode.Add(node);
                        }
                        else
                        {
                            var endpoints = ldapDirEnd.Endpoints.OrderBy(r => r.SystemName).ToList();
                            if (!ldap.ShowEndpoints)
                            {
                                endpoints = new List<LDAPEndpoint>();
                            }
                            if (ldap.HideManagedEndpoints)
                            {
                                endpoints = endpoints.Where(r => !r.Managed).ToList();
                            }
                            foreach (var ldapEnd in endpoints)
                            {
                                var dir = new DirectoryNode
                                {
                                    GuidString = ldapEnd.Id,
                                    ComputerType = ldapEnd.ComputerType,
                                    Title = ldapEnd.SystemName,
                                    ParentGuid = ldapEnd.LDAPDirectoryId,
                                    Managed = ldapEnd.Managed,
                                    NodeColor = ldapEnd.Managed ? "#264A50" : "#525963"
                                };
                                node.DirectoryNodes.Add(dir);
                            }
                            listNode.Add(node);
                        }
                        if (listNode.Count > 0)
                        {
                            LDAPTreeDataSource = listNode[0].DirectoryNodes;
                            DomainName = listNode[0].Title;
                            SetNodeExpandedState(_view.DataTreeLDAP.Nodes);
                        }
                    }

                }
                else
                {
                    DomainName = string.Empty;
                    LDAPTreeDataSource = new ObservableCollection<DirectoryNode>();
                }
            }
            else
            {
                DomainName = string.Empty;
                LDAPTreeDataSource = new ObservableCollection<DirectoryNode>();
            }
        }

        private void SetNodeExpandedState(IEnumerable<XamDataTreeNode> nodes)
        {
            foreach (var item in nodes)
            {
                var data = item.Data as DirectoryNode;
                if (data != null)
                {
                    if (ApplicationContext.LdapExpandedIdList != null && ApplicationContext.LdapExpandedIdList.Contains(data.GuidString))
                    {
                        item.IsExpanded = true;
                    }

                }

                SetNodeExpandedState(item.Nodes);
            }
        }
        
        private void MakeLDAPSubTree(DirectoryNode parentNode, LDAP ldap)
        {
			// find all children of parent node (they have parentId = id of parent node)
			try
			{
				var nodes = new List<DirectoryNode>();
				LDAPDirectoriesEndpoints ldapDirsEnds;
				if (ApplicationContext.LdapDirectoriesEndpointsDictionary.TryGetValue(ldap.Id, out ldapDirsEnds))
				{
					if (!ldap.HideEmptyFolders)
					{
						nodes = ldapDirsEnds.Directories.Where(
						e => e.ParentId == parentNode.GuidString)
						.Select(e => new DirectoryNode
						{
							GuidString = e.Id,
							Title = e.FolderName,
							IsFolder = true,
							NodeColor = "#525963",
							NodeWidth = ApplicationContext.GridRightOriginalWidth
						}).OrderBy(o => o.Title).ToList();
					}
					else
					{
						nodes = ldapDirsEnds.Directories.Where(
						e => e.ParentId == parentNode.GuidString && !e.IsEmptyFolder)
						.Select(e => new DirectoryNode
						{
							GuidString = e.Id,
							Title = e.FolderName,
							IsFolder = true,
							NodeColor = "#525963",
							NodeWidth = ApplicationContext.GridRightOriginalWidth
						}).OrderBy(o => o.Title).ToList();
					}
					// find all children of parent node (they have parentId = id of parent node)
					var nodes2 = ldapDirsEnds.Endpoints.Where(
						e => e.LDAPDirectoryId != null && e.LDAPDirectoryId == parentNode.GuidString)
						.Select(
							e =>
								new DirectoryNode
								{
									GuidString = e.Id,
									Title = e.SystemName,
									IsFolder = false,
									ComputerType = e.ComputerType,
									NodeWidth = ApplicationContext.GridRightOriginalWidth,
									Managed = e.Managed,
									NodeColor = e.Managed ? "#264A50" : "#525963"
								}).OrderBy(o => o.Title);

					// build subtree for each child and add it in parent's children collection
					foreach (var node in nodes)
					{
						MakeLDAPSubTree(node, ldap);
						parentNode.DirectoryNodes.Add(node);
					}
					if (ldap.ShowEndpoints)
					{
						if (ldap.HideManagedEndpoints)
						{
							foreach (var node in nodes2.Where(r => !r.Managed))
							{
								parentNode.DirectoryNodes.Add(node);
							}
						}
						else
						{
							foreach (var node in nodes2)
							{
								parentNode.DirectoryNodes.Add(node);
							}
						}

					}
				}
			}
            catch (Exception ex)
            {
                
                throw;
            }
        }

        private List<LDAP> GetLDAPs()
        {
            var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK"
            };
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<LDAP>>(
                sc.GetAllLDAP,
                requestObj));

            if (resultDeserialize == null)
            {
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                   {
                       var messageDialog =
                           PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                       messageDialog.ShowMessageDialog("Data is null", "Message");
                   }));
                return new List<LDAP>();
            }

            return resultDeserialize;
        }

        /// <summary>
        ///     Call when menu button clicked
        /// </summary>
        /// <param name="btn"></param>
        private void OnMenuSelected(Button btn)
        {
            if (btn == null)
                return;

            switch (btn.Name)
            {
                case UIConstant.LDAPSyncAllName:
                    SyncAll();
                    break;
                case UIConstant.LDAPAddDomain:
                    AddNewLDAP();
                    break;
                case UIConstant.LDAPDeleteDomain:
                    DeleteDomain();
                    break;
                case UIConstant.LDAPMove:
                    DisplayMove();
                    break;
            }
        }

        private void DisplayMove()
        {
            ApplicationContext.NodeTargetId = 0;
            var dlg = new MoveLdapDialog();
            dlg.Model.MakeTree();
            PageNavigatorHelper._MainWindow.DynamicShowDialog(dlg);
        }
        private bool CanMenuAction(Button btn)
        {
            if (btn == null)
                return false;
            var children = _view.PnlDomainContainer.Children;
            if ((from object child in children
                where child.GetType() == typeof (LDAPDomainExpanderElement)
                select child as LDAPDomainExpanderElement).Any(expander => expander.Model.IsLoading))
            {
                return false;
            }
            switch (btn.Name)
            {
                case UIConstant.LDAPDeleteDomain:
                    return (from object child in children
                        where child.GetType() == typeof (LDAPDomainExpanderElement)
                        select child as LDAPDomainExpanderElement).Any(expander => expander.Model.IsActived);
                case UIConstant.LDAPMove:
                    return ApplicationContext.LDAPNodesSelected!=null && ApplicationContext.LDAPNodesSelected.Count>0;
                case UIConstant.LDAPSyncAllName:
                    return ApplicationContext.LdapDirectoriesEndpointsDictionary.Count != 0;
            }
            return true;
        }

        private void DeleteDomain()
        {
            var children = _view.PnlDomainContainer.Children;
            for (var i = children.Count - 1; i >= 0; i--)
            {
                if (children[i].GetType() == typeof(LDAPDomainExpanderElement))
                {
                    var expander = children[i] as LDAPDomainExpanderElement;
                    if (expander.Model.IsActived)
                    {
                        var confirmdialog = new ConfirmDialog("Are you sure you want to delete selected domain?","CONFIRM DELETE");
                        confirmdialog.ConfirmText.Text = "Are you sure you want to delete selected domain?";
                        confirmdialog.BtnOk.Focus();
                        if (confirmdialog.ShowDialog() == true)
                        {
                            if (!expander.Model.IsNotSaved)
                            {
                                var deleteBg = new BackgroundWorker();
                                deleteBg.DoWork += DeleteBg_DoWork;
                                deleteBg.RunWorkerAsync(expander.Model.Id);
                            }

                            _view.PnlDomainContainer.Children.RemoveAt(i);
                            ApplicationContext.LDAPList.RemoveAll(r => r.Id == expander.Model.Id);
                            LDAPTreeDataSource = new ObservableCollection<DirectoryNode>();
                        }
                        break;
                    }
                }
            }
        }

        private void DeleteBg_DoWork(object sender, DoWorkEventArgs e)
        {
            var ldapId = (int)e.Argument;
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
           {
               using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
               {
                   var stringAuth = new StringAuthenticateObject
                   {
                       StringAuth = "OK",
                       StringValue = ldapId.ToString()
                   };
                   var request = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(stringAuth),
                       KeyEncryption);
                   sc.DeleteLDAP(request);
                   DomainName = string.Empty;
               }
           }));
        }

        private void AddNewLDAP()
        {
            var ldapExpander = new LDAPDomainExpanderElement();
            ldapExpander.Model.DomainName = "New domain";
            ldapExpander.Model.IsLoading = false;
            ldapExpander.Model.IsNotSaved = true;
            ldapExpander.Model.IsSecure = false;
            ldapExpander.Model.IsShowEndpoints = true;
            ldapExpander.Model.IsShowFolders = true;
            ldapExpander.Model.Port = 389;
            ldapExpander.Model.SyncInterval = 60;
            ldapExpander.Model.IsHideManagedEndPoints = true;
            _view.PnlDomainContainer.Children.Add(ldapExpander);
            ldapExpander.Expander.IsExpanded = true;
        }

        List<LDAPDomainExpanderElement> _ldapDomain = new List<LDAPDomainExpanderElement>();
        List<BackgroundWorker> workers = new List<BackgroundWorker>();
        private void SyncAll()
        {
            try
            {
                ApplicationContext.IsBusy = true;
                var ldaps = GetLDAPs();
                _ldapDomain = new List<LDAPDomainExpanderElement>();
                var children = _view.PnlDomainContainer.Children;
                foreach (var domain in children)
                {
                    if (domain.GetType() == typeof(LDAPDomainExpanderElement))
                    {
                        var expander = domain as LDAPDomainExpanderElement;
                        expander.Model.IsLoading = true;
                        _ldapDomain.Add(expander);
                    }
                }
                foreach (var ldap in ldaps)
                {
                    var loadDataBg = new BackgroundWorker();
                    loadDataBg.DoWork += SyncLDAPBg_DoWork;
                    workers.Add(loadDataBg);
                    loadDataBg.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
                    {
                        //_view.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action) (() =>
                        //   { 
                        BackgroundWorker wk = (BackgroundWorker)s;
                        workers.Remove(wk);
                        foreach (var ldapDomain in _ldapDomain)
                        {
                            if (ldapDomain.Model.Id == ldap.Id)
                            {
                                _view.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(() =>
                                {
                                    ldapDomain.Model.IsLoading = false;
                                    ldapDomain.Model.ComputerCount = GetEndPointCount(ldap.Id);
                                    CommandManager.InvalidateRequerySuggested();
                                }));
                            }
                        }
                        if (workers.Count == 0)
                        {
                            Refresh();
                            ApplicationContext.IsBusy = false;
                            //MessageBox.Show("Done");
                        }

                        // }));
                    };
                    loadDataBg.RunWorkerAsync(ldap);
                }
            }
            catch (Exception)
            {
                ApplicationContext.IsBusy = false;
            }
            
        }

        private int GetEndPointCount(int ldapId)
        {
            return ApplicationContext.LdapDirectoriesEndpointsDictionary.ContainsKey(ldapId) ? ApplicationContext.LdapDirectoriesEndpointsDictionary[ldapId].Endpoints.Count : 0;
        }

        private void SyncLDAPBg_DoWork(object sender, DoWorkEventArgs e)
        {
            //  _view.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action) (() =>
            //  {

            LDAP ldap = (LDAP)e.Argument;
            var ldapLv1 = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<LDAPDirectory>>(
              sc.GetLDAPLv1,
              ldap));
            if (ldapLv1 == null) return;
            if (!ApplicationContext.LdapDirectoriesEndpointsDictionary.ContainsKey(ldap.Id))
            {
                ApplicationContext.LdapDirectoriesEndpointsDictionary[ldap.Id] = new LDAPDirectoriesEndpoints();
            }
            var ldapEndpointDictionary = ApplicationContext.LdapDirectoriesEndpointsDictionary[ldap.Id];
            ldapEndpointDictionary.Directories = new List<LDAPDirectory>();
            ldapEndpointDictionary.Directories.AddRange(ldapLv1);
            ldapEndpointDictionary.Endpoints = new List<LDAPEndpoint>();
            foreach (var ldapDirectory in ldapLv1)
            {

                if (ldapDirectory.ParentId == null) continue;
                ldap.DistinguishedName = ldapDirectory.DistinguishedName;
                var ldapChild = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<LDAPDirectoriesEndpoints>(
                sc.GetLDAPByDistinguishedName,
                ldap));
                if (ldapChild.Directories.Count > 0)
                    ldapEndpointDictionary.Directories.AddRange(
                            ldapChild.Directories);
                if (ldapChild.Endpoints.Count > 0)
                    ldapEndpointDictionary.Endpoints.AddRange(ldapChild.Endpoints);

            }
            // }));
        }
    }
}