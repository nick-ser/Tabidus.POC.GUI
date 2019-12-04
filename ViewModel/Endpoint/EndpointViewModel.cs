using System;
using System.Windows;
using System.Windows.Threading;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.Model.Discovery;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Endpoint
{
    public class EndpointViewModel : PageViewModelBase
    {
        private static EndPointViewPage _view;
        private int _endpointId;

        public EndpointViewModel(EndPointViewPage view)
        {
            _view = view;
        }

        public void SetParams(int endpointId)
        {
            _endpointId = endpointId;
            EndpointId = _endpointId;
        }

        public void ReloadData()
        {
            GetData();
        }

        public void EditOrAdd(string name, bool isAdd = false)
        {
            var headerViewModel = _view.EndpointViewHeaderElement.DataContext as EndpointViewHeaderViewModel;
            if (headerViewModel != null)
            {
                headerViewModel.SystemName = name;
                SystemName = name;
                if (isAdd)
                {
                    headerViewModel.ImageHeader = "../../Images/logo_noagent.png";
                    headerViewModel.ActivedButtonIndex = 1;
                    headerViewModel.ColorCodeMessageColor = CommonConstants.GREEN_OFFLINE_COLOR;
                    headerViewModel.TextStatusVisible = true;
                    headerViewModel.ColorCodeMessage = "POC Agent not installed";
                    OSName = "";
                    UserName = "";
                    SystemManufacturer = "";
                    SystemType = "";
                    SystemModel = "";
                    LastSync = "";
                    PowerState = "Offline";
                    Processor = "";
                    TotalPhysicalMemory = 0;
                    ComputerType = "";
                    Domain = "";
                    IPv4 = "";
                    IPv6 = "";
                    MACAddress = "";
                    HDDCapacity = string.Empty;
                    ProductVersion = "1.0.0.0";
                }
            }
        }

        public void DisplayDiscoveryEndpoint(NeighborhoodWatch nw)
        {
            var headerViewModel = _view.EndpointViewHeaderElement.DataContext as EndpointViewHeaderViewModel;
            if (headerViewModel != null)
            {
                headerViewModel.SystemName = nw.Computer;
                SystemName = nw.Computer;

                headerViewModel.ImageHeader = "../../Images/logo_noagent.png";
                headerViewModel.ActivedButtonIndex = 1;
                headerViewModel.ColorCodeMessageColor = CommonConstants.GREEN_OFFLINE_COLOR;
                headerViewModel.TextStatusVisible = true;
                headerViewModel.ColorCodeMessage = "";
                OSName = nw.OperatingSystem;
                UserName = "";
                SystemManufacturer = nw.Vendor;
                SystemType = "";
                SystemModel = "";
                LastSync = nw.LastDetected;
                PowerState = "Offline";
                Processor = "";
                TotalPhysicalMemory = 0;
                ComputerType = "";
                Domain = nw.Domain;
                IPv4 = nw.IPv4;
                IPv6 = nw.IPv6;
                MACAddress = nw.MAC;
                HDDCapacity = "0";
                ProductVersion = "1.0.0.0";
            }
        }

        private void GetData()
        {
            try
            {
                var epd = ApplicationContext.EndPointListTree.Find(r => r.EndpointId == _endpointId);
                var comType = epd.ComputerType;

                EndpointId = epd.EndpointId;
                ID = epd.GUIID;
                OSName = epd.OSName;
                UserName = epd.UserName;
                SystemManufacturer = epd.SystemManufacturer;
                SystemName = epd.SystemName;
                SystemType = epd.SystemType;
                SystemModel = epd.SystemModel;
                LastSync = epd.LastSync;
                PowerState = Functions.UppercaseFirst(epd.PowerState);
                Processor = epd.Processor;
                TotalPhysicalMemory = (int) Math.Round(epd.TotalPhysicalMemory ?? 0);
                ComputerType = string.IsNullOrWhiteSpace(ID) ? "" : Functions.UppercaseFirst(comType);
                Domain = epd.Domain;
                IPv4 = epd.IPv4;
                IPv6 = epd.IPv6;
                MACAddress = epd.MACAddress;
                HDDCapacity = epd.HDDCapacity.Replace(";"," GB;");
                FolderId = epd.FolderId;
                ProductVersion = epd.ProductVersion;
                FontColor = "#FFF";
                if (PowerState.ToLower() == "offline")
                {
                    FontColor = "#808e8f98";
                }
                else if(PowerState.ToLower() == "online")
                {
                    FontColor = "#FFF";
                }
                //ImageHeader = string.IsNullOrEmpty(ID)
                //    ? "../../Images/logo_noagent.png"
                //    : string.IsNullOrWhiteSpace(ComputerType)? "../../Images/Notebook.png" : string.Format("../../Images/{0}.png", ComputerType);

                Color = epd.Color;
                ImageHeader ="../"+ EndPoint.GetImages(ComputerType, Color);
                var headerViewModel = _view.EndpointViewHeaderElement.DataContext as EndpointViewHeaderViewModel;
                if (headerViewModel != null)
                {
                    headerViewModel.SystemName = SystemName;
                    headerViewModel.ImageHeader = ImageHeader;
                    headerViewModel.ActivedButtonIndex = 1;
                    headerViewModel.ColorCodeMessageColor = Color;
                    headerViewModel.FontColor = FontColor;
                    //Reset color message
                    headerViewModel.ColorCodeMessage = "";
                    headerViewModel.TextStatusVisible = true;
                    
                    if (string.IsNullOrEmpty(ID))
                    {
                        headerViewModel.ColorCodeMessage = "POC Agent not installed";
                    }
                    else if (!string.IsNullOrEmpty(epd.LastSyncDayText))
                    {
                        headerViewModel.ColorCodeMessage =
                            string.Concat(epd.AgentText, "\n", epd.LastSyncDayText).Trim();
                    }
                    else if (string.IsNullOrEmpty(epd.LastSyncDayText)
                             && (Color == CommonConstants.GREEN_OFFLINE_COLOR
                                 || Color == CommonConstants.GREEN_ONLINE_COLOR)
                             && !string.IsNullOrEmpty(ID)
                        )
                    {
                        headerViewModel.TextStatusVisible = false;
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                PageNavigatorHelper._MainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
                {
                    var messageDialog =
                        PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.TxtMessageText.Text =
                        "Cannot get endpoint due to exception occured, please see the log file under the Logs for more information";
                    messageDialog.Visibility = Visibility.Visible;
                }));
            }
        }

        public override void Refresh()
        {
            ReloadData();
        }

        #region Properties

        private int _eId;

        public int EndpointId
        {
            get { return _eId; }
            set
            {
                _eId = value;
                OnPropertyChanged("EndpointId");
            }
        }

        private string _id;

        public string ID
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("ID");
            }
        }

        private string _osName;

        public string OSName
        {
            get { return _osName; }
            set
            {
                _osName = value;
                OnPropertyChanged("OSName");
            }
        }

        private string _lastSync;

        public string LastSync
        {
            get { return _lastSync; }
            set
            {
                _lastSync = value;
                OnPropertyChanged("LastSync");
            }
        }

        private string _systemName;

        public string SystemName
        {
            get { return _systemName; }
            set
            {
                _systemName = value;
                OnPropertyChanged("SystemName");
            }
        }

        private string _systemManu;

        public string SystemManufacturer
        {
            get { return _systemManu; }
            set
            {
                _systemManu = value;
                OnPropertyChanged("SystemManufacturer");
            }
        }

        private string _systemModel;

        public string SystemModel
        {
            get { return _systemModel; }
            set
            {
                _systemModel = value;
                OnPropertyChanged("SystemModel");
            }
        }

        private string _systemType;

        public string SystemType
        {
            get { return _systemType; }
            set
            {
                _systemType = value;
                OnPropertyChanged("SystemType");
            }
        }

        private string _processor;

        public string Processor
        {
            get { return _processor; }
            set
            {
                _processor = value;
                OnPropertyChanged("Processor");
            }
        }

        private string _userName;

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged("UserName");
            }
        }

        private int? _totalPhysic;

        public int? TotalPhysicalMemory
        {
            get { return _totalPhysic; }
            set
            {
                _totalPhysic = value;
                OnPropertyChanged("TotalPhysicalMemoryString");
            }
        }

        public string TotalPhysicalMemoryString
        {
            get { return TotalPhysicalMemory + " GB"; }
        }

        private string _domain;

        public string Domain
        {
            get { return _domain; }
            set
            {
                _domain = value;
                OnPropertyChanged("Domain");
            }
        }

        private string _ipv4;

        public string IPv4
        {
            get { return _ipv4; }
            set
            {
                _ipv4 = value;
                OnPropertyChanged("IPv4");
            }
        }

        private string _ipv6;

        public string IPv6
        {
            get { return _ipv6; }
            set
            {
                _ipv6 = value;
                OnPropertyChanged("IPv6");
            }
        }

        private string _macAddress;

        public string MACAddress
        {
            get { return _macAddress;}
            set
            {
                _macAddress = value; 
                OnPropertyChanged("MACAddress");
            }
        }

        private string _hddcapa;

        public string HDDCapacity
        {
            get { return _hddcapa; }
            set
            {
                _hddcapa = value;
                OnPropertyChanged("HDDCapacityString");
            }
        }

        public string HDDCapacityString
        {
            get { return HDDCapacity + " GB"; }
        }

        private string _computerType;

        public string ComputerType
        {
            get { return _computerType; }
            set
            {
                _computerType = value;
                OnPropertyChanged("ComputerType");
            }
        }

        private string _powerState;

        public string PowerState
        {
            get { return _powerState; }
            set
            {
                _powerState = value;
                OnPropertyChanged("PowerState");
            }
        }

        public int? FolderId { get; set; }
        private string _productVer;

        public string ProductVersion
        {
            get { return _productVer; }
            set
            {
                _productVer = value;
                OnPropertyChanged("ProductVersion");
            }
        }

        private string _color;

        public string Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("Color");
            }
        }
        private string font_color;
        public string FontColor
        {
            get { return font_color; }
            set
            {
                font_color = value;
                OnPropertyChanged("Color");
            }
        }
        private string _imageHeader;

        public string ImageHeader
        {
            get { return _imageHeader; }
            set
            {
                _imageHeader = value;
                OnPropertyChanged("ImageHeader");
            }
        }

        #endregion
    }
}