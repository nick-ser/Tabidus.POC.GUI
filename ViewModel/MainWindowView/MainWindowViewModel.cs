using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.ServiceModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.Discovery;
using Tabidus.POC.GUI.ViewModel.Endpoint;
using Tabidus.POC.GUI.ViewModel.Policy;
using Tabidus.POC.GUI.ViewModel.Software;
using Tabidus.POC.GUI.ViewModel.Task;

namespace Tabidus.POC.GUI.ViewModel.MainWindowView
{
    /// <summary>
    ///     ViewModel of MainWindow
    /// </summary>
    public partial class MainWindowViewModel : ViewModelBase
    {
        private const int F5_KEY_TIME = 3;
        private readonly MainWindow _view;
        private DateTime refreshDateTime = DateTime.Now;

        /// <summary>
        ///     Constructor
        /// </summary>
        public MainWindowViewModel(MainWindow view)
        {
            _view = view;
            IsBusy = true;
            try
            {
                LastRefresh = ServiceManager.GetLastUpdateData();
            }
            catch (CommunicationObjectFaultedException ex)
            {
                MessageBox.Show("Have error during connecting poc server", "Error");
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Have error during starting GUI", "Error");
                return;
            }
            
#if !DEBUG
			   //Write Main Update Source Url service to config
			   ServiceManager.UpdateMainUpdateSourceConfig();
#else
	        ApplicationContext.MainUpdateSourceUrl = "http://localhost:9003/MainUpdateSource";
#endif
			PageNavigatorHelper._MainWindow = view;
            FromLDAPCommand = new RelayCommand(OnFromLDAPCommand);
            SyncCommand = new RelayCommand(OnSyncNowCommand, CanSyncCommand);
            var loadInitDataBackgroundWorker = new BackgroundWorker();
            loadInitDataBackgroundWorker.DoWork += loadInitDataBackgroundWorker_DoWork;
            loadInitDataBackgroundWorker.RunWorkerCompleted += loadInitDataBackgroundWorker_RunWorkerCompleted;

            //insert main update source if is not existed
            var insertMUpdSourceBg = new BackgroundWorker();
            insertMUpdSourceBg.DoWork += InsertMUpdSourceBg_DoWork;
            insertMUpdSourceBg.RunWorkerAsync();

            loadInitDataBackgroundWorker.RunWorkerAsync();
            //call constructor of MainWindowFileImportViewModel
            MainWindowFileImportViewModel();
            //call constructor of MainWindowDirectoryViewModel
            MainWindowDirectoryViewModel();
            //call constructor of MainWindowGroupViewModel
            MainWindowGroupViewModel();
            VisibleMessageBox = Visibility.Collapsed;

            //start a timer to refresh data 
            StartAppTimer();
        }

        private void InsertMUpdSourceBg_DoWork(object sender, DoWorkEventArgs e)
        {
            //using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            //{
            //    var data = JsonConvert.SerializeObject(new StringAuthenticateObject {StringAuth = "OK"});

            //    sc.AddMainUpdateSource(EncryptionHelper.EncryptString(data, KeyEncryption));
            //}
        }

        public ICommand FromLDAPCommand { get; private set; }
        public ICommand SyncCommand { get; private set; }

        /// <summary>
        ///     Constructor of Partial class
        /// </summary>
        partial void MainWindowDirectoryViewModel();

        partial void MainWindowFileImportViewModel();
        partial void MainWindowGroupViewModel();

        public void ShowMessage(string message)
        {
            MessageTxt = message;
            VisibleMessageBox = Visibility.Visible;
        }

        public void HideMessage()
        {
            VisibleMessageBox = Visibility.Collapsed;
        }

#region Properties

        //specify what navigation button is clicked. 1=Endpoints, 2=Discovery, 3=Software, 4=licenses, 5=Policies, 6=Reporting, 7=Notifications, 8=Settings
        private int _navigationIndex = (int) NavigationIndexes.Endpoint;

        public int NavigationIndex
        {
            get { return _navigationIndex; }
            set
            {
                _navigationIndex = value;
                OnPropertyChanged("NavigationIndex");
            }
        }

        private bool _isBusy;

        //this property uses to visible or hidden loading panel, if IsBusy = true, loading panel is visible, if IsBusy = false, loading panel is hidden
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        private int _nodeId;

        public int NodeId
        {
            get { return _nodeId; }
            set
            {
                _nodeId = value;
                OnPropertyChanged("NodeId");
            }
        }

        private Visibility _visibleMessageBox;

        public Visibility VisibleMessageBox
        {
            get { return _visibleMessageBox; }
            set
            {
                _visibleMessageBox = value;
                OnPropertyChanged("VisibleMessageBox");
            }
        }


        private string _messageTxt;

        public string MessageTxt
        {
            get { return _messageTxt; }
            set
            {
                _messageTxt = value;
                OnPropertyChanged("MessageTxt");
            }
        }

#endregion

#region Private methods

        private void OnFromLDAPCommand(object arg)
        {
            _view.BdAddButton.Visibility = Visibility.Collapsed;
            _view.menugrid.Visibility = Visibility.Hidden;

            var leftViewModel = PageNavigatorHelper.GetLeftElementViewModel();
            if (leftViewModel != null)
            {
                leftViewModel.SelectedLDAPNavigation();
                PageNavigatorHelper.Switch(new LDAPPage());
            }
        }

        private void OnSyncNowCommand(object arg)
        {
            using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            {
                var fids = new List<int>();
                var eids = new List<int>();
                var endplst = new List<EndPoint>();
                var rightVm = PageNavigatorHelper.GetRightElementViewModel();
                if (rightVm.DirectoryPushed)
                {
                    //Select all endpoint in selected folders and selected endpoints
                    foreach (var cnode in ApplicationContext.NodesSelected)
                    {
                        if (cnode != null)
                        {
                            if (cnode.IsFolder)
                            {
                                fids.Add(cnode.NodeId);
                            }
                            else
                            {
                                eids.Add(cnode.NodeId);
                            }
                        }
                    }

                    foreach (var fid in fids)
                    {
                        GetAllEndpointOfFolder(endplst, fid);
                    }
                    foreach (var eid in eids)
                    {
                        var endpoint = ApplicationContext.EndPointListAll.Find(e => e.EndpointId == eid);
                        if (!endplst.Select(ep => ep.EndpointId).Contains(endpoint.EndpointId))
                        {
                            endplst.Add(endpoint);
                        }
                    }
                }
                else
                {
                    //Select all endpoint in selected labels and selected endpoints
                    foreach (var cnode in ApplicationContext.LabelNodesSelected)
                    {
                        if (cnode != null)
                        {
                            if (cnode.IsFolder)
                            {
                                fids.Add(cnode.NodeId);
                            }
                            else
                            {
                                eids.Add(cnode.NodeId);
                            }
                        }
                    }
                    var endpointDatas = new List<LabelEndPointsData>();
                    foreach (var id in fids)
                    {
                        var listEndpoints = ApplicationContext.LableEndpointDatas.Where(r => r.Id == id).ToList();
                        endpointDatas.AddRange(listEndpoints);
                    }
                    foreach (var le in endpointDatas)
                    {
                        foreach (var en in le.EndPointDatas)
                        {
                            if (!endplst.Select(r => r.EndpointId).Contains(en.EndpointId))
                            {
                                var endpoint = new EndPoint(en);
                                endplst.Add(endpoint);
                            }
                        }
                    }
                    foreach (var eid in eids)
                    {
                        var endpoint = ApplicationContext.EndPointListAll.Find(e => e.EndpointId == eid);
                        if (!endplst.Select(ep => ep.EndpointId).Contains(endpoint.EndpointId))
                        {
                            endplst.Add(endpoint);
                        }
                    }
                }
                if (endplst.Count == 0)
                {
                    var messageDialog =
                        PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.ShowMessageDialog("You must select at least one Endpoint to sync","Sync Action");
                    return;
                }
                var requestObj = new StringAuthenticateObject
                {
                    StringAuth = "OK",
                    StringValue = string.Join(",", endplst.Select(r => r.EndpointId).ToList())
                };
                var requestData = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(requestObj), KeyEncryption);
                
                try
                {
                    sc.SyncNow(requestData);
                }
                catch (Exception ex)
                {
                    
                }
                
            }
        }

        public void GetAllEndpointOfFolder(List<EndPoint> lend, int fid)
        {
            foreach (var end in ApplicationContext.EndPointListTree)
            {
                if (end.FolderId == fid)
                {
                    if (!lend.Select(ep => ep.EndpointId).Contains(end.EndpointId))
                    {
                        lend.Add(end);
                    }
                }
            }
            foreach (var dir in ApplicationContext.FolderListTree)
            {
                if (dir.ParentId == fid)
                {
                    GetAllEndpointOfFolder(lend, dir.FolderId);
                }
            }
        }

        private void OnReloadData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (Action) (() =>
            {
                HideMessage();
                var worker = sender as BackgroundWorker;
                worker.RunWorkerCompleted -= OnReloadData_RunWorkerCompleted;
                worker.DoWork -= OnReloadData_Dowork;
                worker.Dispose();

                CommandManager.InvalidateRequerySuggested();
            }));
        }

        private void OnReloadData_Dowork(object sender, DoWorkEventArgs e)
        {
            ShowMessage("Loading data...");
            _view.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (Action) (() =>
            {
                ShowMessage("Loading data...");
                ReloadData();
                ApplicationContext.IsBusy = false;
                CommandManager.InvalidateRequerySuggested();
            }));
        }

        private void loadInitDataBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            //Releases all resources used by BackgroundWorker
            var worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= loadInitDataBackgroundWorker_RunWorkerCompleted;
            worker.DoWork -= loadInitDataBackgroundWorker_DoWork;
            worker.Dispose();
        }

        private void loadInitDataBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action) (() =>
                {
                    MakeTree(0, false, string.Empty, true, true);
                    var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
                    rightViewModel.LoadLabelView(true);
                    ApplicationContext.LabelTreeSourceBeforeSearch =
                        _view.RightTreeElement.LabelDataTree.ItemsSource as List<DirectoryNode>;
                }));
        }

        /// <summary>
        ///     Make tree data
        /// </summary>
        private void MakeTreeNode(int nodeSelected, bool resText = false, bool isReload = false)
        {
            _view.menugrid.Visibility = Visibility.Hidden;
            _view.BdAddButton.Visibility = Visibility.Hidden;
            _view.RightTreeViewModel.UpdateTree(nodeSelected, resText, isReload);
        }

        /// <summary>
        ///     Make tree data
        /// </summary>
        private void MakeTreeNode(DirectoryNode nodeSelected, bool isEdited = false, bool isReload = false)
        {
            _view.menugrid.Visibility = Visibility.Hidden;
            _view.BdAddButton.Visibility = Visibility.Hidden;
            _view.RightTreeViewModel.UpdateTree(nodeSelected, false, isEdited, isReload);
        }

        public void StartAppTimer()
        {
            AppTimer.StopAppTimer();
            AppTimer.StartAppTimer(dispatcherTimer_Tick);
        }

        /// <summary>
        ///     After interval time, application will refresh data
        /// </summary>
        private void dispatcherTimer_Tick()
        {
            if (!_isRefreshing && !ApplicationContext.IsBusy)
            {
                var refreshBackgroundWk = new BackgroundWorker();
                refreshBackgroundWk.DoWork += RefreshBackgroundWk_DoWork;
                refreshBackgroundWk.RunWorkerCompleted += RefreshBackgroundWk_RunWorkerCompleted;
                refreshBackgroundWk.RunWorkerAsync();
            }
            
        }

        private void RefreshBackgroundWk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _isRefreshing = false;
        }

        private bool _isRefreshing;
        private void RefreshBackgroundWk_DoWork(object sender, DoWorkEventArgs e)
        {
            GettingData();
        }

        /// <summary>
        ///     Getting data using BackgroundWorker
        /// </summary>
        public void GettingData()
        {
            _isRefreshing = true;
            RefreshApp();
        }
        /// <summary>
        /// Indicate that whether we can refresh screen or not
        /// </summary>
        /// <returns></returns>
        private void RefreshApp()
        {
            ApplicationContext.CanRefreshEndpoint = false;
            ApplicationContext.CanRefreshDiscovery = false;
            ApplicationContext.CanRefreshLDAP = false;
            ApplicationContext.CanRefreshSoftware = false;
            ApplicationContext.CanRefreshUpdateSource = false;
            ApplicationContext.CanRefreshPolicyAgent = false;
            ApplicationContext.CanRefreshPolicyAgentDirectory = false;
	        ApplicationContext.CanRefreshTask = false;
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
                        if (lastupd.Name == "PolicyAgent")
                        {
                            ApplicationContext.CanRefreshPolicyAgent = true;
                        }
                        if (lastupd.Name == "PolicyAgentDirectoryEndpoint")
                        {
                            ApplicationContext.CanRefreshPolicyAgentDirectory = true;
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
                    ApplicationContext.IsReload = true;
                    ApplicationContext.IsReloadForRefresh = true;
                    ReloadData();
                    LastRefresh = lastRefreshUpdate;
            }
            
        }
        private void loadDataBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Releases all resources used by BackgroundWorker
            var worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= loadDataBackgroundWorker_RunWorkerCompleted;
            worker.DoWork -= loadDataBackgroundWorker_DoWork;
            worker.Dispose();
        }

        private void loadDataBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() => { ReloadData(); }));
        }

        private void aBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) ReloadData);
        }

        public void ReloadAsysnc()
        {
            var loadDataBackgroundWorker = new BackgroundWorker();
            loadDataBackgroundWorker.DoWork += aBackgroundWorker_DoWork;
            loadDataBackgroundWorker.RunWorkerAsync();
        }

        public void ReloadData()
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                var basePage = PageNavigatorHelper.GetMainContentViewModel<PageViewModelBase>();
                if (ApplicationContext.CanRefreshEndpoint)
                {
                    switch (NavigationIndex)
                    {
                        case (int)NavigationIndexes.Endpoint:
                        case (int)NavigationIndexes.Label:
                        case (int)NavigationIndexes.Discovery:
                        case (int)NavigationIndexes.NeighborhoodWatch:
                        case (int)NavigationIndexes.LDAP:
                        case (int)NavigationIndexes.Policy:
                        case (int)NavigationIndexes.POCAgent:
                            if (IsDirectory && ApplicationContext.NodesSelected == null)
                            {
                                //refresh tree data
                                _view.RightTreeViewModel.UpdateTree(0, false, true);
                                _view.RightTreeViewModel.LoadLabelView(true);
                            }
                            else if (IsDirectory && ApplicationContext.NodesSelected != null &&
                                     ApplicationContext.NodesSelected.Count >= 0)
                            {
                                //refresh tree data
                                _view.RightTreeViewModel.UpdateTree(ApplicationContext.NodesSelected, false, true);
                                _view.RightTreeViewModel.LoadLabelView(true);
                            }
                            break;
                        default:
                            break;
                    }
                    //refresh neighborhood watch page
                    if (basePage != null &&
                        (basePage.GetType() == typeof(NeighborhoodWatchViewModel) ||
                         basePage.GetType() == typeof(LDAPViewModel)) ||
						 PageNavigatorHelper.IsCurrent<EndPointListPage>())
                        basePage.Refresh();
                }
                else if (ApplicationContext.CanRefreshDiscovery)
                {
                    //refresh neighborhood watch page
                    if (basePage != null && basePage.GetType() == typeof(NeighborhoodWatchViewModel))
                        basePage.Refresh();
                }
                if (ApplicationContext.CanRefreshLDAP)
                {
                    //refresh LDAP page
                    if (basePage != null && basePage.GetType() == typeof(LDAPViewModel))
                        basePage.Refresh();
                }
                if (ApplicationContext.CanRefreshSoftware)
                {
                    if (basePage != null && basePage.GetType() == typeof(SoftwareViewModel))
                        basePage.Refresh();
                    else
                    {
                        Functions.GetAllSoftware();
                        Functions.GetAllUpdateSourceSoftware();
                    }
                }
                if (ApplicationContext.CanRefreshUpdateSource)
                {
                    switch (NavigationIndex)
                    {
                        case (int)NavigationIndexes.Software:
                        case (int)NavigationIndexes.Transfer:
                        case (int)NavigationIndexes.Download:
                            _view.RightTreeViewModel.BuilSoftwareTree(true);
                            if (basePage != null && basePage.GetType() == typeof(TransferViewModel))
                                basePage.Refresh();
                            break;
                        default:
                            _view.RightTreeViewModel.RebuildUpdateSourceCache();
                            break;
                    }
                }
                if (ApplicationContext.CanRefreshPolicyAgentDirectory)
                {
                    Functions.LoadFolderPolicy();
                    Functions.LoadEndpointPolicy();
                    switch (NavigationIndex)
                    {
                        case (int)NavigationIndexes.POCAgent:
                            _view.RightTreeViewModel.UpdateTree(ApplicationContext.NodesSelected, false, true);
                            break;
                        default:
                            break;
                    }
                }
                if (ApplicationContext.CanRefreshPolicyAgent)
                {
                    if (basePage != null && basePage.GetType() == typeof(POCAgentViewModel))
                        basePage.Refresh();
                    else
                    {
                        Functions.GetAllPolicies();
                    }
                }
				if (ApplicationContext.CanRefreshTask)
				{
					if (basePage != null && basePage.GetType() == typeof(TaskListViewModel))
						basePage.Refresh();
				}
			}));
            
        }

        private bool CanSyncCommand(object arg)
        {
            var rightVm = PageNavigatorHelper.GetRightElementViewModel();
            if (rightVm.DirectoryPushed)
            {
                return ApplicationContext.NodesSelected != null && ApplicationContext.NodesSelected.Count > 0;
            }
            else
            {
                return ApplicationContext.LabelNodesSelected != null && ApplicationContext.LabelNodesSelected.Count > 0;
            }
            
        }
#endregion
    }
}