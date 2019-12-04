using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.LDAP;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls.Discovery;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Discovery
{
    public class LDAPDomainExpanderElementViewModel : ViewModelBase
    {
        private static LDAPDomainExpanderElement _view;
        DispatcherTimer _loadingTimer = new DispatcherTimer();
        public LDAPDomainExpanderElementViewModel(LDAPDomainExpanderElement view)
        {
            _view = view;
            LDAPtypes = ApplicationContext.LDAPTypes;
            LDAPTypeSelected = LDAPtypes.Count > 0 ? LDAPtypes[0] : "";
            ShowLDAPDirectory = new RelayCommand(OnShowLDAPCommand);
            SaveCommand = new RelayCommand(SaveLDAP, CanSaveCommand);
            SyncNowCommand = new RelayCommand(OnSyncNow, CanSyncNowCommand);
            IsLoading = true;
            //Show loading
            _loadingTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _loadingTimer.Tick += loadingTimer_Tick;
            _loadingTimer.Start();
            
        }

        public void LoadData(List<BackgroundWorker> workers)
        {
            //Load data
            var loadDataBg = new BackgroundWorker();
            loadDataBg.DoWork += LoadDataBg_DoWork;
            workers.Add(loadDataBg);
            loadDataBg.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
            {
                IsLoading = false;
                ReCountComputer();
                BackgroundWorker wk = (BackgroundWorker)s;
                workers.Remove(wk);
                if (workers.Count == 0)
                {
                    var ldapViewModel = PageNavigatorHelper.GetMainContentViewModel<LDAPViewModel>();
                    //Handle error when switch page, backgroundworker does not auto stop
                    if (ApplicationContext.LDAPActived != null && ldapViewModel != null)
                    {
                        ldapViewModel.BuildLDAPTree(ApplicationContext.LDAPActived);
                    }
                        
                }
            };
            loadDataBg.RunWorkerAsync();
        }


        private void LoadDataBg_DoWork(object sender, DoWorkEventArgs e)
        {
            
                var addValue = GetData();
                ApplicationContext.LdapDirectoriesEndpointsDictionary.AddOrUpdate(Id, addValue, (key,oldValue)=> addValue);
            
        }

        private LDAPDirectoriesEndpoints GetData()
        {
            var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK",
                StringValue = Id.ToString()
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

        void loadingTimer_Tick(object sender, EventArgs e)
        {
            LoadingAngle += 45;
        }
        #region Properties
        public ICommand ShowLDAPDirectory { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand SyncNowCommand { get; set; }
        private void OnShowLDAPCommand(object arg)
        {
            var ldapViewModel = PageNavigatorHelper.GetMainContentViewModel<LDAPViewModel>();
            if (ldapViewModel != null)
            {
                ApplicationContext.LDAPActived = new LDAP { Id = Id, Domain = DomainName, ShowEndpoints = IsShowEndpoints, ShowFolders = IsShowFolders, HideEmptyFolders = IsHideEmptyFolders, HideManagedEndpoints = IsHideManagedEndPoints };
                ldapViewModel.BuildLDAPTree(ApplicationContext.LDAPActived);
            }
        }

        private void SaveLDAP(object arg)
        {
            var messageDialog =
                            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
            var requestObj = new LDAP
            {
                Id = Id,
                Domain = DomainName,
                HideEmptyFolders = IsHideEmptyFolders,
                HideManagedEndpoints = IsHideManagedEndPoints,
                Password = Password,
                User = User,
                Port = Port,
                ShowFolders = IsShowFolders,
                ShowEndpoints = IsShowEndpoints,
                Server = Server,
                SyncInterval = SyncInterval,
                TitleName = DomainName,
                Type = LDAPTypeStore,
                IsSecureLDAP = IsSecure
            };
            if (ApplicationContext.LDAPList!=null&&ApplicationContext.LDAPList.Find(r =>r.Server == requestObj.Server && r.Port == requestObj.Port) != null)
            {
                if (IsNotSaved)
                {
                    messageDialog.ShowMessageDialog("This domain is existed in the system", "Message");
                    return;
                }
                else
                {
                    if (
                        ApplicationContext.LDAPList.Find(r => r.Server == requestObj.Server && r.Port == requestObj.Port && r.Id!=requestObj.Id) !=
                        null)
                    {
                        messageDialog.ShowMessageDialog("This domain is existed in system", "Message");
                        return;
                    }
                }
            }
            
            if (string.IsNullOrWhiteSpace(requestObj.Domain)|| string.IsNullOrWhiteSpace(requestObj.Server) || string.IsNullOrWhiteSpace(requestObj.User) || string.IsNullOrWhiteSpace(requestObj.Password))
            {
                messageDialog.ShowMessageDialog("All of text fields must be not empty", "Warning");
                return;
            }
            if (!IsShowEndpoints && !IsShowFolders)
            {
                messageDialog.ShowMessageDialog("Please select atleast ShowEndpoints or ShowFolders", "Warning");
                return;
            }
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<LDAP>(
                sc.AddNewLDAP,
                requestObj));

            if (resultDeserialize == null)
            {
                messageDialog.ShowMessageDialog("Data is null", "Message");
                return;
            }
            if (resultDeserialize.CannotConnect)
            {
                messageDialog.ShowMessageDialog("Cannot access to this domain, but the domain information is still saved into the database", "Warning");
            }
            Id = resultDeserialize.Id;
            ApplicationContext.LDAPList.Add(resultDeserialize);
            IsNotSaved = false;
            OnSyncNow(null);
        }

        private void OnSyncNow(object arg)
        {
            IsLoading = true;
            ApplicationContext.IsBusy = true;
            var syncDataBk = new BackgroundWorker();
            syncDataBk.DoWork += SyncDataBk_DoWork;
            syncDataBk.RunWorkerCompleted += SyncDataBk_RunWorkerCompleted;
            syncDataBk.RunWorkerAsync();
            
        }

        private List<LDAPDirectory> _ldapDirectories = new List<LDAPDirectory>(); 
        private void SyncDataBk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!ApplicationContext.LdapDirectoriesEndpointsDictionary.ContainsKey(Id))
            {
                var addValue = new LDAPDirectoriesEndpoints();
                ApplicationContext.LdapDirectoriesEndpointsDictionary.AddOrUpdate(Id, addValue, (key, oldValue) => addValue);
            }
            ApplicationContext.LdapDirectoriesEndpointsDictionary[Id].Directories = _ldapDirectories;
            ApplicationContext.LdapDirectoriesEndpointsDictionary[Id].Endpoints = new List<LDAPEndpoint>();
            var ldapViewModel = PageNavigatorHelper.GetMainContentViewModel<LDAPViewModel>();
            if (IsActived)
            {
                
                if (ldapViewModel != null)
                {
                    var ldap = new LDAP
                    {
                        Type = LDAPTypeStore,
                        Domain = DomainName,
                        Server = Server,
                        User = User,
                        Port = Port,
                        IsSecureLDAP = IsSecure,
                        Password = Password,
                        SyncInterval = SyncInterval,
                        ShowEndpoints = IsShowEndpoints,
                        ShowFolders = IsShowFolders,
                        Id = Id,
                        HideManagedEndpoints = IsHideManagedEndPoints,
                        HideEmptyFolders = IsHideEmptyFolders
                    };
                    ldapViewModel.BuildLDAPTree(ldap);
                }
            }
            if (_ldapDirectories == null || (_ldapDirectories != null && !_ldapDirectories.Any()))
            {
                IsLoading = false;
                ComputerCount = 0;
                ApplicationContext.IsBusy = false;
            }
            List<BackgroundWorker> workers = new List<BackgroundWorker>();
            foreach (var dir in _ldapDirectories)
            {
                if (dir.FolderName != DomainName)
                {
                    var worker = new BackgroundWorker();
                    worker.DoWork += Worker_DoWork; 
                    workers.Add(worker);
                    worker.RunWorkerCompleted += delegate (object s, RunWorkerCompletedEventArgs args)
                    {
                        BackgroundWorker wk = (BackgroundWorker)s;
                        workers.Remove(wk);
                        if (workers.Count == 0)
                        {
                            IsLoading = false;
                            ComputerCount = ApplicationContext.LdapDirectoriesEndpointsDictionary.ContainsKey(Id)? ApplicationContext.LdapDirectoriesEndpointsDictionary[Id].Endpoints.Count:0;
                            ldapViewModel.Refresh();
                            ApplicationContext.IsBusy = false;
                        }
                    };
                    worker.RunWorkerAsync(dir);
                }
                
            }
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var dir = e.Argument as LDAPDirectory;
                if (dir != null)
                {
                    var ldap = new LDAP
                    {
                        Type = LDAPTypeStore,
                        Domain = DomainName,
                        Server = Server,
                        User = User,
                        Port = Port,
                        IsSecureLDAP = IsSecure,
                        Password = Password,
                        SyncInterval = SyncInterval,
                        ShowEndpoints = IsShowEndpoints,
                        ShowFolders = IsShowFolders,
                        Id = Id,
                        HideManagedEndpoints = IsHideManagedEndPoints,
                        HideEmptyFolders = IsHideEmptyFolders,
                        DistinguishedName = dir.DistinguishedName
                    };
                    var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<LDAPDirectoriesEndpoints>(
                    sc.GetLDAPByDistinguishedName,
                    ldap));
                    if (resultDeserialize != null)
                    {
                        if (!ApplicationContext.LdapDirectoriesEndpointsDictionary.ContainsKey(Id))
                        {
                            var addValue = new LDAPDirectoriesEndpoints();
                            ApplicationContext.LdapDirectoriesEndpointsDictionary.AddOrUpdate(Id, addValue, (key, oldValue) => addValue);
                        }
                        ApplicationContext.LdapDirectoriesEndpointsDictionary[Id].Directories.AddRange(resultDeserialize.Directories);
                        ApplicationContext.LdapDirectoriesEndpointsDictionary[Id].Endpoints.AddRange(resultDeserialize.Endpoints);
                        _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                        {
                            if (IsActived)
                            {
                                var ldapViewModel = PageNavigatorHelper.GetMainContentViewModel<LDAPViewModel>();
                                if (ldapViewModel != null)
                                {
                                    var ldapData = new LDAP
                                    {
                                        Type = LDAPTypeStore,
                                        Domain = DomainName,
                                        Server = Server,
                                        User = User,
                                        Port = Port,
                                        IsSecureLDAP = IsSecure,
                                        Password = Password,
                                        SyncInterval = SyncInterval,
                                        ShowEndpoints = IsShowEndpoints,
                                        ShowFolders = IsShowFolders,
                                        Id = Id,
                                        HideManagedEndpoints = IsHideManagedEndPoints,
                                        HideEmptyFolders = IsHideEmptyFolders
                                    };
                                    ldapViewModel.BuildLDAPTree(ldapData);
                                }
                            }
                        }));
                    }
                }
            }
            catch (Exception)
            {
                ApplicationContext.IsBusy = false;
            }
            
        }

        private void SyncDataBk_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var ldap = new LDAP
                {
                    IsSecureLDAP = IsSecure,
                    Domain = DomainName,
                    Server = Server,
                    Port = Port,
                    User = User,
                    Password = Password,
                    Id = Id
                };
                var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<LDAPDirectory>>(
                    sc.GetLDAPLv1,
                    ldap));
                if (resultDeserialize != null)
                {
                    _ldapDirectories = resultDeserialize;
                }
            }
            catch (Exception)
            {
                ApplicationContext.IsBusy = false;
            }
            
        }

        private void FilterData()
        {
            var filterBk = new BackgroundWorker();
            filterBk.DoWork += FilterBk_DoWork;
            filterBk.RunWorkerAsync();
        }

        private void FilterBk_DoWork(object sender, DoWorkEventArgs e)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                if (IsActived)
                {
                    var ldapViewModel = PageNavigatorHelper.GetMainContentViewModel<LDAPViewModel>();
                    if (ldapViewModel != null)
                    {
                        var ldap = new LDAP
                        {
                            Type = LDAPTypeStore,
                            Domain = DomainName,
                            Server = Server,
                            User = User,
                            Port = Port,
                            IsSecureLDAP = IsSecure,
                            Password = Password,
                            SyncInterval = SyncInterval,
                            ShowEndpoints = IsShowEndpoints,
                            ShowFolders = IsShowFolders,
                            Id = Id,
                            HideManagedEndpoints = IsHideManagedEndPoints,
                            HideEmptyFolders = IsHideEmptyFolders
                        };
                        ReCountComputer();
                        ldapViewModel.BuildLDAPTree(ldap);
                    }
                }
            }));
            
        }

        private void ReCountComputer()
        {
            if (!IsShowEndpoints)
            {
                ComputerCount = 0;
            }
            else
            {
                if (IsHideManagedEndPoints)
                    ComputerCount = ApplicationContext.LdapDirectoriesEndpointsDictionary.ContainsKey(Id)
                    ? ApplicationContext.LdapDirectoriesEndpointsDictionary[Id].Endpoints.Count(
                        r => !r.Managed)
                    : 0;
                else
                {
                    ComputerCount = ApplicationContext.LdapDirectoriesEndpointsDictionary.ContainsKey(Id)
                    ? ApplicationContext.LdapDirectoriesEndpointsDictionary[Id].Endpoints.Count() : 0;
                }
            }
        }

        private bool CanSaveCommand(object arg)
        {
            return !IsLoading;
        }
        private bool CanSyncNowCommand(object arg)
        {
            if (IsNotSaved || IsLoading)
                return false;
            return true;
        }

        public int Id { get; set; }
        private string _domainName;

        public string DomainName
        {
            get { return _domainName; }
            set
            {
                _domainName = value;
                OnPropertyChanged("DomainName");
            }
        }

        private double _loadingAngle;

        public double LoadingAngle
        {
            get { return _loadingAngle; }
            set
            {
                _loadingAngle = value;
                OnPropertyChanged("LoadingAngle");
            }
        }

        private int _computerCount;

        public int ComputerCount
        {
            get { return _computerCount; }
            set
            {
                _computerCount = value;
                OnPropertyChanged("ComputerCount");
            }
        }

        private List<string> _LDAPtypes;

        public List<string> LDAPtypes
        {
            get { return _LDAPtypes; }
            set
            {
                _LDAPtypes = value;
                OnPropertyChanged("LDAPtypes");
            }
        }

        private string _titleName;

        public string TitleName
        {
            get { return _titleName; }
            set
            {
                _titleName = value;
                OnPropertyChanged("TitleName");
            }
        }

        private string _server;

        public string Server
        {
            get { return _server; }
            set
            {
                _server = value;
                OnPropertyChanged("Server");
            }
        }

        private int _port;

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        private string _user;

        public string User
        {
            get { return _user; }
            set
            {
                _user = value;
                OnPropertyChanged("User");
            }
        }

        private string _password;

        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }

        private int _syncInterval;

        public int SyncInterval
        {
            get { return _syncInterval; }
            set
            {
                _syncInterval = value;
                OnPropertyChanged("SyncInterval");
            }
        }

        private bool _isShowEndpoints;

        public bool IsShowEndpoints
        {
            get { return _isShowEndpoints; }
            set
            {
                _isShowEndpoints = value;
                
                OnPropertyChanged("IsShowEndpoints");
            }
        }

        private bool _isShowFolders;

        public bool IsShowFolders
        {
            get { return _isShowFolders; }
            set
            {
                _isShowFolders = value;
                
                OnPropertyChanged("IsShowFolders");
            }
        }


        private bool _isHideEmptyFolders;

        public bool IsHideEmptyFolders
        {
            get { return _isHideEmptyFolders; }
            set
            {
                _isHideEmptyFolders = value;
                
                OnPropertyChanged("IsHideEmptyFolders");
            }
        }
        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                _isLoading = value;
                CommandManager.InvalidateRequerySuggested();
                OnPropertyChanged("IsLoading");
            }
        }
        private bool _isActived;

        public bool IsActived
        {
            get { return _isActived; }
            set
            {
                _isActived = value;
                OnPropertyChanged("IsActived");
                OnPropertyChanged("ExpanderBackgroundColor");
                OnPropertyChanged("TextColor");
                OnPropertyChanged("TextNoColor");
            }
        }
        private bool _isHideManagedEndPoints;

        public bool IsHideManagedEndPoints
        {
            get { return _isHideManagedEndPoints; }
            set
            {
                _isHideManagedEndPoints = value;
                
                OnPropertyChanged("IsHideManagedEndPoints");
            }
        }
        private bool _isExpanded;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged("IsExpanded");
            }
        }
        
        private bool _isSecure;

        public bool IsSecure
        {
            get { return _isSecure; }
            set
            {
                _isSecure = value;
                OnPropertyChanged("IsSecure");
            }
        }
        

        private string _LDAPTypeSelected;

        public string LDAPTypeSelected
        {
            get { return _LDAPTypeSelected; }
            set
            {
                _LDAPTypeSelected = value;
                OnPropertyChanged("LDAPTypeSelected");
            }
        }
        
        public string ExpanderBackgroundColor
        {
            get { return IsActived? "#331DABED": "#08FFFFFF"; }
        }

        public string TextColor
        {
            get { return IsActived ? "#FFF" : "#FFF"; }
        }
        public string TextNoColor
        {
            get { return IsActived ? "#FFF" : "#1DABED"; }
        }
        public string TextColorRight
        {
            get { return IsActived ? "#FFF" : "#1DABED"; }
        }
        public int LDAPTypeStore
        {
            get { return ApplicationContext.LdapTypeDictionary[LDAPTypeSelected]; }
        }

        private bool _isNotSaved;

        public bool IsNotSaved
        {
            get { return _isNotSaved; }
            set
            {
                _isNotSaved = value; 
                OnPropertyChanged("IsNotSaved");
            }
        }

        #endregion
    }
}