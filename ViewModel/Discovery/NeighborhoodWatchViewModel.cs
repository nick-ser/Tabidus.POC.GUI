using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Discovery;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.UserControls.Discovery;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.Endpoint;

namespace Tabidus.POC.GUI.ViewModel.Discovery
{
    /// <summary>
    ///     Class GroupHeaderViewModel.
    /// </summary>
    public class NeighborhoodWatchViewModel : PageViewModelBase
    {
        /// <summary>
        ///     The _view
        /// </summary>
        private static NeighborhoodWatchPage _view;

        private ObservableCollection<NeighborhoodWatch> _neighborhoodList;
        private bool _unmanagedActived = true;
        private bool _managedActived;
        private bool _confirmedActived;
        private string _subnetMask;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="view"></param>
        public NeighborhoodWatchViewModel(NeighborhoodWatchPage view)
        {
            _view = view;
            var neighborhoodState = Functions.GetConfig("NEIGHBORHOOD_VIEW_STATE_KEY", "");
            var confirmedState = Functions.GetColumnWidth("Confirmed", neighborhoodState);
            var unmanagedState = Functions.GetColumnWidth("Unmanaged", neighborhoodState);
            var managedState = Functions.GetColumnWidth("Managed", neighborhoodState);
            ManagedActived = managedState == 1;
            UnmanagedActived = unmanagedState == 1;
            ConfirmedActived = confirmedState == 1;
            NeighborhoodList = new ObservableCollection<NeighborhoodWatch>();
            TabSelectedCommand = new RelayCommand<Button>(OnMenuSelected, MenuCanCommand);
            CreateNetworks();
            foreach (var member in NeighborhoodList)
                member.PropertyChanged += (sender, e) =>
                {
                    if (e.PropertyName == "IsSelected")
                        this.OnPropertyChanged("AllMembersAreChecked");
                };
        }

        #region Command

        /// <summary>
        ///     Gets the tab selected command.
        /// </summary>
        /// <value>The tab selected command.</value>
        public ICommand TabSelectedCommand { get; private set; }

        #endregion

        public ObservableCollection<NeighborhoodWatch> NeighborhoodList
        {
            get { return _neighborhoodList; }
            set
            {
                _neighborhoodList = value;
                OnPropertyChanged("NeighborhoodList");
            }
        }

        public bool UnmanagedActived
        {
            get { return _unmanagedActived; }
            set
            {
                _unmanagedActived = value;
                OnPropertyChanged("UnmanagedActived");
                OnPropertyChanged("UnmanagedIconPath");
            }
        }
        public bool ManagedActived
        {
            get { return _managedActived; }
            set
            {
                _managedActived = value;
                OnPropertyChanged("ManagedActived");
                OnPropertyChanged("ManagedIconPath");
            }
        }
        public bool ConfirmedActived
        {
            get { return _confirmedActived; }
            set
            {
                _confirmedActived = value;
                OnPropertyChanged("ConfirmedActived");
                OnPropertyChanged("ConfirmedIconPath");
            }
        }

        public string UnmanagedIconPath
        {
            get { return UnmanagedActived ? "../../Images/green_on_ico.png" : "../../Images/red_off_ico.png"; }
        }
        public string ManagedIconPath
        {
            get { return ManagedActived ? "../../Images/green_on_ico.png" : "../../Images/red_off_ico.png"; }
        }
        public string ConfirmedIconPath
        {
            get { return ConfirmedActived ? "../../Images/green_on_ico.png" : "../../Images/red_off_ico.png"; }
        }
        public override void Refresh()
        {
            var listSelectedId = ApplicationContext.AllNeighborhoodWatch.Where(r => r.IsSelected).Select(r => r.Id).ToList();
            var refreshNeighborhoodBwk = new BackgroundWorker();
            refreshNeighborhoodBwk.DoWork += RefreshNeighborhoodBwk_DoWork;
            refreshNeighborhoodBwk.RunWorkerCompleted += RefreshNeighborhoodBwk_RunWorkerCompleted;
            refreshNeighborhoodBwk.RunWorkerAsync(listSelectedId);
            
        }

        private void RefreshNeighborhoodBwk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshPage();
        }

        private void WriteStateToConfig()
        {
            var stateConfigText =
                string.Format(
                    "Confirmed:{0};Unmanaged:{1};Managed:{2}",
                    ConfirmedActived?1:0, UnmanagedActived?1:0,
                    ManagedActived?1:0);
            Functions.WriteToConfig("NEIGHBORHOOD_VIEW_STATE_KEY", stateConfigText);
        }

        private void RefreshNeighborhoodBwk_DoWork(object sender, DoWorkEventArgs e)
        {
            var listSelectedId = e.Argument as List<int>;
            ApplicationContext.AllNeighborhoodWatch = GetNeighborhoods(string.Empty);
            if (listSelectedId != null)
            {
                foreach (var nhw in ApplicationContext.AllNeighborhoodWatch.Where(nhw => listSelectedId.Contains(nhw.Id)))
                {
                    nhw.IsSelected = true;
                }
            }
            
        }

        #region Private Function

        /// <summary>
        ///     Called when [tab selected].
        /// </summary>
        public void OnTabSelected()
        {
            var obserNeighborhood = new ObservableCollection<NeighborhoodWatch>();
            if (ConfirmedActived || ManagedActived || UnmanagedActived)
            {
                var neighborhood =
                    ApplicationContext.AllNeighborhoodWatch.FindAll(
                        r =>
                            r.SubnetMask == _subnetMask && ((ConfirmedActived && r.Confirmed) ||
                             (ManagedActived && r.Managed) || (UnmanagedActived && !r.Managed && !r.Confirmed)));
                foreach (var nb in neighborhood)
                {
                    obserNeighborhood.Add(nb);
                }
            }
            var totalUnmanagedEndps = ApplicationContext.AllNeighborhoodWatch.Count(c => c.SubnetMask == _subnetMask
                        && !c.Managed && !c.Confirmed);
            var nws = _view.PnlNetworks.Children;
            foreach (var nw in nws)
            {
                if (nw.GetType() == typeof (NetworkButtonElement))
                {
                    var nwe = nw as NetworkButtonElement;
                    if (nwe.Model.SubnetMark == _subnetMask)
                    {
                        nwe.Model.TotalEndpoint = totalUnmanagedEndps;
                        break;
                    }
                }
            }
            NeighborhoodList = obserNeighborhood;
        }

        public bool? AllMembersAreChecked
        {
            get
            {
                // Determine if all members have the same 
                // value for the IsChecked property.
                bool? value = null;
                for (int i = 0; i < NeighborhoodList.Count; ++i)
                {
                    if (i == 0)
                    {
                        value = NeighborhoodList[0].IsSelected;
                    }
                    else if (value != NeighborhoodList[i].IsSelected)
                    {
                        value = false;
                        break;
                    }
                }

                return value;
            }
            set
            {
                if (value == null)
                    return;

                foreach (NeighborhoodWatch member in NeighborhoodList)
                    member.IsSelected = value.Value;
            }
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
                case UIConstant.NeighborhoodClearUnmanaged:
                    ClearUnmanagedNeighborhood();
                    break;
                case UIConstant.NeighborhoodUnmanaged:
                    UnmanagedActived = !UnmanagedActived;
                    WriteStateToConfig();
                    break;
                case UIConstant.NeighborhoodManaged:
                    ManagedActived = !ManagedActived;
                    WriteStateToConfig();
                    break;
                case UIConstant.NeighborhoodConfirmed:
                    ConfirmedActived = !ConfirmedActived;
                    WriteStateToConfig();
                    break;
                case UIConstant.NeighborhoodMove:
                    OnMoveFileExecute();
                    break;
            }
            if(btn.Name!= UIConstant.NeighborhoodMove)
                OnTabSelected();
        }

        private void OnMoveFileExecute()
        {
            var moveTreeDialog = PageNavigatorHelper._MainWindow.NeighborhoodWatchMoveTargetDialogView;
            var moveTreeViewModel = moveTreeDialog.Model;
            if (moveTreeViewModel != null)
            {
                moveTreeViewModel.MakeTree();
                var selectedData = ApplicationContext.AllNeighborhoodWatch.Where(r => r.IsSelected).ToList();
                moveTreeViewModel.SetData(selectedData);
            }
            ApplicationContext.NodeTargetId = 0;
            //Show choose target to move dialog
            moveTreeDialog.ShowWindow();
        }

        private bool MenuCanCommand(Button btn)
        {
            if (btn == null)
                return false;

            switch (btn.Name)
            {
                case UIConstant.NeighborhoodMove:
                    return ApplicationContext.AllNeighborhoodWatch.Find(r => r.IsSelected) != null;
                default:
                    return true;
            }
        }

        private void ClearUnmanagedNeighborhood()
        {
            var nids = ApplicationContext.AllNeighborhoodWatch.Where(r => !r.Managed && !r.Confirmed && r.SubnetMask==_subnetMask).Select(r => r.Id).ToList();
            var clearUnmanagedBk = new BackgroundWorker();
            clearUnmanagedBk.DoWork += ClearUnmanagedBk_DoWork;
            clearUnmanagedBk.RunWorkerAsync(nids);
            for (var i = ApplicationContext.AllNeighborhoodWatch.Count - 1; i >= 0; i--)
            {
                if (nids.Contains(ApplicationContext.AllNeighborhoodWatch[i].Id))
                {
                    ApplicationContext.AllNeighborhoodWatch.RemoveAt(i);
                }
            }
            RefreshPage();
            
        }

        private void ClearUnmanagedBk_DoWork(object sender, DoWorkEventArgs e)
        {
            var nids = e.Argument as List<int>;
            ClearUnmanagedNeighborhoodDb(nids);
        }

        private void ClearUnmanagedNeighborhoodDb(List<int> nids)
        {
            var nidsJoin = string.Join(",", nids);
            var strAuth = new StringAuthenticateObject
            {
                StringAuth = "OK",
                StringValue = nidsJoin
            };
            using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            {
                var data = JsonConvert.SerializeObject(strAuth);
                sc.DeleteNeighborhoodWatch(EncryptionHelper.EncryptString(data, KeyEncryption));
            }
        }

        /// <summary>
        ///     Get all discovery endpoints that its subnet mark is subnetMark
        /// </summary>
        /// <param name="subnetMark"></param>
        private List<NeighborhoodWatch> GetNeighborhoods(string subnetMark)
        {

            var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK",
                StringValue = subnetMark
            };
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<NeighborhoodWatch>>(
                sc.GetAllNeighborhoodWatch,
                requestObj));

            if (resultDeserialize == null)
            {
                PageNavigatorHelper._MainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action) (() =>
                    {
                        var messageDialog =
                            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                        messageDialog.ShowMessageDialog("Data is null", "Message");
                    }));
                return new List<NeighborhoodWatch>();
            }
            
            return resultDeserialize;
        }
        
        
        /// <summary>
        ///     Build network buttons
        /// </summary>
        private void CreateNetworks()
        {
            _view.PnlNetworks.Children.Clear();
            var networkList = new List<Network>();
            ApplicationContext.AllNeighborhoodWatch = GetNeighborhoods(string.Empty);
            foreach (var nb in ApplicationContext.AllNeighborhoodWatch)
            {
                if (!networkList.Select(r => r.Name).Contains(nb.SubnetMask))
                {
                    //The Networks-Grid should count only unmanaged devices. This means no managed (this works) and no confirmed ones
                    var totalEndps = ApplicationContext.AllNeighborhoodWatch.Count(c => c.SubnetMask == nb.SubnetMask 
                        && !c.Managed && !c.Confirmed);
                    var nw = new Network
                    {
                        Name = nb.SubnetMask,
                        TotalEndpoint = totalEndps
                    };
                    networkList.Add(nw);
                }
            }
            if (networkList != null)
            {
                int count = 0;
                foreach (var net in networkList)
                {
                    var networkButton = new NetworkButtonElement();
                    networkButton.Model.SubnetMark = net.Name;
                    networkButton.Model.TotalEndpoint = net.TotalEndpoint;
                    networkButton.BtnNetwork.Click += BtnNetwork_Click;
                    _view.PnlNetworks.Children.Add(networkButton);
                    if (count == 0)
                    {
                        networkButton.SetActived();
                        _subnetMask = net.Name;
                        OnTabSelected();
                    }
                    count++;
                }
                
            }
        }

        private void RefreshPage()
        {
            _view.PnlNetworks.Children.Clear();
            var networkList = new List<Network>();
            foreach (var nb in ApplicationContext.AllNeighborhoodWatch)
            {
                if (!networkList.Select(r => r.Name).Contains(nb.SubnetMask))
                {
                    //The Networks-Grid should count only unmanaged devices. This means no managed (this works) and no confirmed ones
                    var totalEndps = ApplicationContext.AllNeighborhoodWatch.Count(c => c.SubnetMask == nb.SubnetMask 
                        && !c.Managed & !c.Confirmed);
                    var nw = new Network
                    {
                        Name = nb.SubnetMask,
                        TotalEndpoint = totalEndps
                    };
                    networkList.Add(nw);
                }
            }
            if (networkList != null)
            {
                var flag = false;
                foreach (var net in networkList)
                {
                    var networkButton = new NetworkButtonElement();
                    networkButton.Model.SubnetMark = net.Name;
                    networkButton.Model.TotalEndpoint = net.TotalEndpoint;
                    networkButton.BtnNetwork.Click += BtnNetwork_Click;
                    _view.PnlNetworks.Children.Add(networkButton);
                    if (_subnetMask == net.Name)
                    {
                        networkButton.SetActived();
                        OnTabSelected();
                        flag = true;
                    }
                    
                }
                if (!flag)
                {
                    var networkBtns = _view.PnlNetworks.Children.Cast<NetworkButtonElement>().ToList();
                    if (networkBtns.Count > 0)
                    {
                        networkBtns[0].SetActived();
                        _subnetMask = networkBtns[0].Model.SubnetMark;
                        OnTabSelected();
                    }
                    else
                    {
                        _subnetMask = string.Empty;
                    }
                }
            }
        }

        private void BtnNetwork_Click(object sender, RoutedEventArgs e)
        {
            var netElem = ((Button) sender).TryFindParent<NetworkButtonElement>();
            if (netElem != null)
            {
                var netElemViewModel = netElem.Model;
                _subnetMask = netElemViewModel.SubnetMark;
                foreach (var nhw in ApplicationContext.AllNeighborhoodWatch)
                {
                    nhw.IsSelected = false;
                }
                
                OnTabSelected();
            }
        }

        #endregion
    }
}