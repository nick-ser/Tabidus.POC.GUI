using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IdentityModel.Protocols.WSTrust;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Newtonsoft.Json;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.MainWindowView
{
    public partial class MainWindowViewModel
    {
        private Guid _guid;
        private string _labelName;
        private string _mac;
        private EndPoint _endpointIdAdded;
        private Directory _dirIdAdded;
        private int _nodeSeleted;
        private DirectoryNode _newNode;
        partial void MainWindowDirectoryViewModel()
        {
            ApplicationContext.ExpandedIds = new List<int>();
            ApplicationContext.LabelExpandedIds = new List<int>();
            ShowAddPanelCommand = new RelayCommand(OnAddFileExecute, CanImportFromFile);
            
            ShowMovePanelCommand = new RelayCommand(OnMoveFileExecute, CanMoveAndDeleteDirectory);
        }
        #region Commands

        public ICommand AddFolderCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = param =>
                    {
                        _view.BdAddButton.Visibility = Visibility.Collapsed;
                        _view.menugrid.Visibility = Visibility.Hidden;

                        if (ApplicationContext.NodesSelected.Count < 1)
                        {                         
                            var messageDialog = _view.MessageDialogContentControl.Content as MessageDialog;
                            messageDialog.TxtMessageText.Text =
                                "Select a parent directory to add a new directory, please!";
                            messageDialog.Visibility = Visibility.Visible;
                            return;
                        }
                        ApplicationContext.IsBusy = true;
                        _guid = Guid.NewGuid();
                        _labelName = GetFolderNameToAdd();
                        _nodeSeleted = ApplicationContext.NodesSelected.Count > 0
                            ? ApplicationContext.NodesSelected[0].NodeId
                            : 1;
                        AddFolderAction();
                    }
                };
            }
        }

        public ICommand AddComputerCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = param =>
                    {
                        _view.BdAddButton.Visibility = Visibility.Collapsed;
                        _view.menugrid.Visibility = Visibility.Hidden;

                        if (ApplicationContext.NodesSelected.Count < 1)
                        {                          
                            var messageDialog = _view.MessageDialogContentControl.Content as MessageDialog;
                            messageDialog.TxtMessageText.Text =
                                "Select a parent directory to add a new endpoint, please!";
                            messageDialog.Visibility = Visibility.Visible;
                            return;
                        }
                        ApplicationContext.IsBusy = true;
                        _guid = Guid.NewGuid();
                        _labelName = GetComputerNameToAdd();
                        _nodeSeleted = ApplicationContext.NodesSelected.Count > 0
                            ? ApplicationContext.NodesSelected[0].NodeId
                            : 1;
                        _mac = string.Empty;
                        AddComputerAction();
                    }
                };
            }
        }

        public void AddComputer(DirectoryNode dir, string title, string mac)
        {
            _guid = Guid.NewGuid();
            _labelName = string.IsNullOrWhiteSpace(title)? GetComputerNameToAdd(dir.NodeId):title;
            _mac = mac;
            _nodeSeleted = dir.NodeId;
            AddComputerAction(dir);
        }

        public ICommand DeleteNodesCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = param =>
                    {
                        _view.BdAddButton.Visibility = Visibility.Collapsed;
                        _view.menugrid.Visibility = Visibility.Hidden;

                        if (ApplicationContext.NodesSelected == null || ApplicationContext.NodesSelected.Count < 1)
                        {
                            var messageDialog = _view.MessageDialogContentControl.Content as MessageDialog;
                            messageDialog.TxtMessageText.Text = "Select a directory to delete, please!";
                            messageDialog.Visibility = Visibility.Visible;
                            return;
                        }
                        var confirmbox = new ConfirmDialog("Are you sure you want to delete the selected object?","CONFIRM DELETE");
                        //confirmbox.ConfirmText.Text = "Are you sure you want to delete the selected object?";
                        confirmbox.BtnOk.Focus();
                        if (confirmbox.ShowDialog() == true)
                        {
                            var data = ApplicationContext.NodesSelected.Select(ns => new DeleteDirectoryFolderRequest
                            {
                                Id = ns.NodeId,
                                IsDirectory = ns.IsFolder
                            }).ToList();

                            if (ApplicationContext.NodesSelected.Count > 0)
                            {
                                DeleteDirectoryAndEndpointAction(data);
                            }
                        }
                    },
                    CanExecuteFunc = () => { return CanMoveAndDeleteDirectory(null); }
                };
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = param =>
                    {
                        var nowDateTime = DateTime.Now;
                        var datediff = nowDateTime.Subtract(refreshDateTime).Seconds;
                        
                        if (datediff > F5_KEY_TIME)
                        {
                            refreshDateTime = nowDateTime;
                            dispatcherTimer_Tick();
                        }
                    },
                  //  CanExecuteFunc = () => { return CanRefresh(); }
                };
            }
        }
        public ICommand ShowAddPanelCommand { get; set; }

        public ICommand ShowMovePanelCommand { get; set; }

        #endregion

        #region Properties
        private bool _isDirectory = true;

        public bool IsDirectory
        {
            get { return _isDirectory; }
            set
            {
                _isDirectory = value;
                OnPropertyChanged("IsDirectory");
            }
        }
        //this property use to visible or hidden adddelete button
        private bool _addDeleteButtonVisible = true;

        public bool AddDeleteButtonVisible
        {
            get { return _addDeleteButtonVisible; }
            set
            {
                _addDeleteButtonVisible = value;
                OnPropertyChanged("AddDeleteButtonVisible");
            }
        }

        //this property use to visible or hidden add buttons
        private bool _addFromButtonVisible;

        public bool AddFromButtonVisible
        {
            get { return _addFromButtonVisible; }
            set
            {
                _addFromButtonVisible = value;
                OnPropertyChanged("AddFromButtonVisible");
            }
        }
        public ICommand ImportFromFileCommand { get; private set; }
        #endregion

        #region Public methods
        /// <summary>
        ///     Edit endpoint name
        /// </summary>
        /// <param name="data">endpoint data</param>
        public void EditEndpointAction(DirectoryEndpoint data)
        {
            var editEndpointBackgroundWorker = new BackgroundWorker();
            editEndpointBackgroundWorker.DoWork += EditEndpointBackgroundWorker_DoWork;
            editEndpointBackgroundWorker.RunWorkerCompleted += EditEndpointBackgroundWorker_RunWorkerCompleted;
            editEndpointBackgroundWorker.RunWorkerAsync(data);
            ApplicationContext.EndPointListAll.Where(r => r.EndpointId == data.FolderId)
                        .ToList()
                        .ForEach(r => r.SystemName = data.Name);
            ApplicationContext.EndPointListTree.Where(r => r.EndpointId == data.FolderId)
                .ToList()
                .ForEach(r => r.SystemName = data.Name);
            ApplicationContext.IsAddNode = false;
            ApplicationContext.IsEditNode = true;
            ApplicationContext.IsDeleteNode = false;
            //refresh tree data
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() =>
                {
                    var dn = new DirectoryNode
                    {
                        IsFolder = false,
                        NodeId = data.FolderId,
                        Title = data.Name
                    };
                    
                    SelectCurrentTreeNode(dn);
                }));
        }

        /// <summary>
        ///     Edit directory name
        /// </summary>
        /// <param name="data">directory data</param>
        public void EditDirectoryAction(DirectoryEndpoint data)
        {
            var editFolderBackgroundWorker = new BackgroundWorker();
            editFolderBackgroundWorker.DoWork += EditFolderBackgroundWorker_DoWork;
            editFolderBackgroundWorker.RunWorkerCompleted += EditFolderBackgroundWorker_RunWorkerCompleted;
            editFolderBackgroundWorker.RunWorkerAsync(data);
            ApplicationContext.FolderListAll.Where(r => r.FolderId == data.FolderId)
                        .ToList()
                        .ForEach(r => r.FolderName = data.Name);
            ApplicationContext.FolderListTree.Where(r => r.FolderId == data.FolderId)
                .ToList()
                .ForEach(r => r.FolderName = data.Name);

            ApplicationContext.IsAddNode = false;
            ApplicationContext.IsEditNode = true;
            ApplicationContext.IsDeleteNode = false;
            //refresh tree data
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() =>
                {
                    var dn = new DirectoryNode
                    {
                        IsFolder = true,
                        NodeId = data.FolderId
                    };
                    
                    SelectCurrentTreeNode(dn);

                }));
        }

        /// <summary>
        ///     Delete directories and endpoints
        /// </summary>
        /// <param name="data">list of directory id and list of endpoint id</param>
        public void DeleteDirectoryAndEndpointAction(List<DeleteDirectoryFolderRequest> data)
        {
            try
            {
				ApplicationContext.DirNodesSelectedBeforeSearch = new List<DirectoryNode>();

				ApplicationContext.IsBusy = true;
                var deleteDirAndEndpointBackgroundWorker = new BackgroundWorker();
                deleteDirAndEndpointBackgroundWorker.DoWork += DeleteDirAndEndpointBackgroundWorker_DoWork;
                deleteDirAndEndpointBackgroundWorker.RunWorkerCompleted +=
                    DeleteDirAndEndpointBackgroundWorker_RunWorkerCompleted;
                deleteDirAndEndpointBackgroundWorker.RunWorkerAsync(data);
                ApplicationContext.IsAddNode = false;
                ApplicationContext.IsDeleteNode = true;

                //refresh tree data
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    var ldata = data.Select(d => new DirectoryNode
                    {
                        NodeId = d.Id,
                        IsFolder = d.IsDirectory
                    }).ToList();
                    DeleteNodes(ldata);

                    foreach (var dat in ldata)
                    {
                        if (dat.IsFolder)
                        {
                            ApplicationContext.FolderListTree.RemoveAll(r => r.FolderId == dat.NodeId);
                            ApplicationContext.FolderListAll.RemoveAll(r => r.FolderId == dat.NodeId);
                        }
                        else
                        {
                            ApplicationContext.EndPointListTree.RemoveAll(r => r.EndpointId == dat.NodeId);
                            ApplicationContext.EndPointListAll.RemoveAll(r => r.EndpointId == dat.NodeId);
                        }
                    }
                    var belowNode = ApplicationContext.BelowNode;
                    if (belowNode != null)
                    {
                        SelectCurrentTreeNode(belowNode);
                    }
                    else
                    {
                        var dn = new DirectoryNode
                        {
                            IsFolder = true,
                            NodeId = ApplicationContext.FolderListTree[0].FolderId
                        };
                        SelectCurrentTreeNode(dn);
                    }
                }));
            }
            catch (Exception ex)
            {
                ApplicationContext.IsBusy = false;
                Logger.Error(ex.Message, ex);
            }
        }
        private void DeleteDirAndEndpointBackgroundWorker_RunWorkerCompleted(object sender,
          RunWorkerCompletedEventArgs e)
        {
            ApplicationContext.IsBusy = false;
            CommandManager.InvalidateRequerySuggested();
            //Releases all resources used by BackgroundWorker
            var worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= DeleteDirAndEndpointBackgroundWorker_RunWorkerCompleted;
            worker.DoWork -= DeleteDirAndEndpointBackgroundWorker_DoWork;
            worker.Dispose();
        }

        private void DeleteDirAndEndpointBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var data = (List<DeleteDirectoryFolderRequest>)e.Argument;
                DeleteDirectoriesAndEndpoints(data);
            }
            catch (Exception)
            {
                ApplicationContext.IsBusy = false;
            }
            
        }

        private void EditFolderBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _view.RightTreeElement.DataTree.SelectionSettings.NodeSelection = TreeSelectionType.Multiple;
            CommandManager.InvalidateRequerySuggested();
            //Releases all resources used by BackgroundWorker
            var worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= EditFolderBackgroundWorker_RunWorkerCompleted;
            worker.DoWork -= EditFolderBackgroundWorker_DoWork;
            worker.Dispose();
        }

        private void MoveNodeBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
            //Releases all resources used by BackgroundWorker
            var worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= MoveNodeBackgroundWorker_RunWorkerCompleted;
            worker.DoWork -= MoveNodeBackgroundWorker_DoWork;
            worker.Dispose();
        }

        private void MoveNodeBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var mags = (MoveFoldersAndEndpointsInputArgs)e.Argument;
            MoveDirectoriesAndEndpoints(mags);
        }

        private void EditEndpointBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _view.RightTreeElement.DataTree.SelectionSettings.NodeSelection = TreeSelectionType.Multiple;
            CommandManager.InvalidateRequerySuggested();
            //Releases all resources used by BackgroundWorker
            var worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= EditEndpointBackgroundWorker_RunWorkerCompleted;
            worker.DoWork -= EditEndpointBackgroundWorker_DoWork;
            worker.Dispose();
        }

        private void EditEndpointBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var ed = (DirectoryEndpoint)e.Argument;
            EditEndpoint(ed);
        }
        private void EditFolderBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var de = (DirectoryEndpoint)e.Argument;
            EditFolder(de);
            
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Add new Computer
        /// </summary>
        /// <param name="dir">parent node. if dir==null than add computer from adding button, else than adding computer from other</param>
        private void AddComputerAction(DirectoryNode dir = null)
        {
            try
            {
                var isPocAgent = NavigationIndex == (int) NavigationIndexes.POCAgent;
                var dn = new DirectoryNode
                {
                    IsFolder = false,
                    Title = _labelName,
                    ComputerType = 2,
                    PowerState = 1,
                    IsNoAgent = true,
                    NodeColor = CommonConstants.GREEN_OFFLINE_COLOR,
                    NodeWidth = ApplicationContext.GridRightOriginalWidth,
                    Guid = _guid,
                    NodeHoverWidth = isPocAgent ? 0 : ApplicationContext.GridRightOriginalWidth,
                    NodeSelectedWidth = isPocAgent ? ApplicationContext.GridRightOriginalWidth : 0,
                    NodePolicyColor = isPocAgent ? UIConstant.PolicyDefaultColor : ""
                };
                _newNode = dn;
                var addBackgroundWorker = new BackgroundWorker();
                addBackgroundWorker.DoWork += AddBackgroundWorker_DoWork;
                addBackgroundWorker.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
                {
                    var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
                    if (dir == null)
                    {
                        _newNode.NodeId = _endpointIdAdded.EndpointId;
                        _newNode.NodeColor = _endpointIdAdded.Color;
                        var enp = ApplicationContext.EndPointListTree.Find(r => r.Guid == _guid);
                        if (enp != null)
                        {
                            enp.GUIID = _endpointIdAdded.GUIID;
                            enp.EndpointId = _endpointIdAdded.EndpointId;
                            enp.Color = _endpointIdAdded.Color;
                            enp.AgentText = _endpointIdAdded.AgentText;
                            enp.ComputerType = _endpointIdAdded.ComputerType;
                            enp.Domain = _endpointIdAdded.Domain;
                            enp.FolderId = _endpointIdAdded.FolderId;
                            enp.HDDCapacity = _endpointIdAdded.HDDCapacity;
                            enp.ID = _endpointIdAdded.ID;
                            enp.IPv4 = _endpointIdAdded.IPv4;
                            enp.IPv6 = _endpointIdAdded.IPv6;
                            enp.LastSync = _endpointIdAdded.LastSync;
                            enp.LastSyncDayText = _endpointIdAdded.LastSyncDayText;
                            enp.MACAddress = _endpointIdAdded.MACAddress;
                            enp.OSName = _endpointIdAdded.OSName;
                            enp.PowerState = _endpointIdAdded.PowerState;
                            enp.Processor = _endpointIdAdded.Processor;
                            enp.ProductVersion = _endpointIdAdded.ProductVersion;
                            enp.SystemManufacturer = _endpointIdAdded.SystemManufacturer;
                            enp.SystemModel = _endpointIdAdded.SystemModel;
                            enp.SystemName = _endpointIdAdded.SystemName;
                            enp.SystemType = _endpointIdAdded.SystemType;
                            enp.TotalPhysicalMemory = _endpointIdAdded.TotalPhysicalMemory;
                            enp.UserName = _endpointIdAdded.UserName;
                        }
                    }
                    else
                    {
                        var snode = new DirectoryNode
                        {
                            IsFolder = false,
                            NodeId = _endpointIdAdded.EndpointId
                        };
                        MakeTreeNode(snode, false, true);
                    }

                    //refresh labels tree data
                    rightViewModel.LoadLabelView(true);
                    ApplicationContext.IsError = false;
                };
                addBackgroundWorker.RunWorkerAsync();
                ApplicationContext.IsAddNode = true;
                ApplicationContext.IsDeleteNode = false;
                //refresh tree data
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    if (!ApplicationContext.ExpandedIds.Contains(_nodeSeleted))
                    {
                        ApplicationContext.ExpandedIds.Add(_nodeSeleted);
                    }

                    var endp = new EndPoint
                    {
                        Guid = _guid,
                        FolderId = _nodeSeleted
                    };
                    var ec = ApplicationContext.EndPointListAll.Count;
                    ApplicationContext.EndPointListTree.Add(endp);
                    if (ec == ApplicationContext.EndPointListAll.Count)
                    {
                        ApplicationContext.EndPointListAll.Add(endp);
                    }

                    AddNodeToCurrentTree(dn, true, dir);
                    if (dir == null && ApplicationContext.NodeEditting != null)
                        _view.RightTreeElement.Model.TreeEnterEditMode(_view.RightTreeElement.DataTree.Nodes);
                }));
            }
            catch (Exception ex)
            {
                ApplicationContext.IsBusy = false;
                //refresh tree data
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    MakeTreeNode(0, true, true);
                }));
                Logger.Error(ex.Message, ex);
            }
            
        }

        /// <summary>
        ///     Adding a new folder
        /// </summary>
        private void AddFolderAction()
        {
            try
            {
                var addBackgroundWorker = new BackgroundWorker();
                addBackgroundWorker.DoWork += AddFolderBackgroundWorker_DoWork;
                addBackgroundWorker.RunWorkerCompleted += AddFolderBackgroundWorker_RunWorkerCompleted;
                addBackgroundWorker.RunWorkerAsync();
                ApplicationContext.IsAddNode = true;
                ApplicationContext.IsDeleteNode = false;
                //refresh tree data
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    if (!ApplicationContext.ExpandedIds.Contains(_nodeSeleted))
                    {
                        ApplicationContext.ExpandedIds.Add(_nodeSeleted);
                    }
                    
                    var isPocAgent = NavigationIndex == (int)NavigationIndexes.POCAgent;
                    var dn = new DirectoryNode
                    {
                        IsFolder = true,
                        Title = _labelName,
                        NodeColor = CommonConstants.DEFAULT_TEXT_COLOR,
                        NodeWidth = ApplicationContext.GridRightOriginalWidth,
                        Guid = _guid,
                        NodeHoverWidth = isPocAgent ? 0 : ApplicationContext.GridRightOriginalWidth,
                        NodeSelectedWidth = isPocAgent ? ApplicationContext.GridRightOriginalWidth : 0,
                        NodePolicyColor = isPocAgent ? UIConstant.PolicyDefaultColor : ""
                    };
                    var fol = new Directory
                    {
                        Guid = _guid,
                        ParentId = _nodeSeleted
                    };
                    var ec = ApplicationContext.FolderListAll.Count;
                    ApplicationContext.FolderListTree.Add(fol);
                    if (ec == ApplicationContext.FolderListAll.Count)
                    {
                        ApplicationContext.FolderListAll.Add(fol);
                    }
                    AddNodeToCurrentTree(dn, true);
                    if (ApplicationContext.NodeEditting != null && _view.RightTreeElement.DataTree!=null)
                        _view.RightTreeElement.Model.TreeEnterEditMode(_view.RightTreeElement.DataTree.Nodes);
                }));
            }
            catch (Exception ex)
            {
                ApplicationContext.IsBusy = false;
                //refresh tree data
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    MakeTreeNode(0, true, true);
                }));
                Logger.Error(ex.Message, ex);
            }
            
        }
        private void AddBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _newNode.NodeId = _endpointIdAdded.EndpointId;
            _newNode.NodeColor = _endpointIdAdded.Color;
            var enp = ApplicationContext.EndPointListTree.Find(r => r.Guid == _guid);
            if (enp != null)
            {
                enp.GUIID = _endpointIdAdded.GUIID;
                enp.EndpointId = _endpointIdAdded.EndpointId;
                enp.Color = _endpointIdAdded.Color;
                enp.AgentText = _endpointIdAdded.AgentText;
                enp.ComputerType = _endpointIdAdded.ComputerType;
                enp.Domain = _endpointIdAdded.Domain;
                enp.FolderId = _endpointIdAdded.FolderId;
                enp.HDDCapacity = _endpointIdAdded.HDDCapacity;
                enp.ID = _endpointIdAdded.ID;
                enp.IPv4 = _endpointIdAdded.IPv4;
                enp.IPv6 = _endpointIdAdded.IPv6;
                enp.LastSync = _endpointIdAdded.LastSync;
                enp.LastSyncDayText = _endpointIdAdded.LastSyncDayText;
                enp.MACAddress = _endpointIdAdded.MACAddress;
                enp.OSName = _endpointIdAdded.OSName;
                enp.PowerState = _endpointIdAdded.PowerState;
                enp.Processor = _endpointIdAdded.Processor;
                enp.ProductVersion = _endpointIdAdded.ProductVersion;
                enp.SystemManufacturer = _endpointIdAdded.SystemManufacturer;
                enp.SystemModel = _endpointIdAdded.SystemModel;
                enp.SystemName = _endpointIdAdded.SystemName;
                enp.SystemType = _endpointIdAdded.SystemType;
                enp.TotalPhysicalMemory = _endpointIdAdded.TotalPhysicalMemory;
                enp.UserName = _endpointIdAdded.UserName;
            }
            
            //refresh labels tree data
            var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
            rightViewModel.LoadLabelView(true);
            ApplicationContext.IsError = false;
            //Releases all resources used by BackgroundWorker
            var worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= AddBackgroundWorker_RunWorkerCompleted;
            worker.DoWork -= AddBackgroundWorker_DoWork;
            worker.Dispose();
        }

        private void AddBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            AddComputer();
        }

        private void AddFolderBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (ApplicationContext.NodesSelected != null && ApplicationContext.NodesSelected.Count > 0)
            {
                ApplicationContext.NodesSelected.First().NodeId = _dirIdAdded.FolderId;
                var fol = ApplicationContext.FolderListTree.First(r => r.Guid == _guid);
                fol.FolderId = _dirIdAdded.FolderId;
                fol.FolderName = _dirIdAdded.FolderName;
                fol.ParentId = _dirIdAdded.ParentId;
            }

            //Releases all resources used by BackgroundWorker
            var worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= AddFolderBackgroundWorker_RunWorkerCompleted;
            worker.DoWork -= AddFolderBackgroundWorker_DoWork;
            worker.Dispose();
        }

        private void AddFolderBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            AddFolder();
            
        }

        /// <summary>
        ///     Adding a new endpoint
        /// </summary>
        private void AddComputer()
        {
            try
            {
                Logger.Info("Starting Add a new Endpoint");
                
                //create a new computer
                var newComputer = new DirectoryEndpoint
                {
                    FolderId = _nodeSeleted,
                    Name = _labelName,
                    Mac = _mac
                };
                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var data = JsonConvert.SerializeObject(newComputer);

                    var cstr = sc.SaveDirectoryEndpoint(EncryptionHelper.EncryptString(data, KeyEncryption));
                    var com = JsonConvert.DeserializeObject<EndPointData>(EncryptionHelper.DecryptRijndael(cstr,
                        KeyEncryption));
                    
                    var endp = new EndPoint(com);
                    _endpointIdAdded = endp;
                    
                }
                
                Logger.Info("Ended adding a new endpoint");
            }
            catch (Exception ex)
            {
                ApplicationContext.IsBusy = false;
                //refresh tree data
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    MakeTreeNode(0,true,true);
                }));
                Logger.Error(ex);
            }
        }

        /// <summary>
        ///     Adding a new directory
        /// </summary>
        private void AddFolder()
        {
            try
            {
                Logger.Info("Starting Add a new Directory");
                
                //create a new folder
                var newFolder = new Folder
                {
                    FolderName = _labelName,
                    ParentId = _nodeSeleted
                };
                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var data = JsonConvert.SerializeObject(newFolder);
                    var fstr = sc.SaveNewFolder(EncryptionHelper.EncryptString(data, KeyEncryption));
                    var fol = JsonConvert.DeserializeObject<Directory>(EncryptionHelper.DecryptRijndael(fstr,
                        KeyEncryption));
                    _dirIdAdded = fol;
                    
                    
                }
                
                Logger.Info("Ended adding a new Directory");
            }
            catch (Exception ex)
            {
                //refresh tree data
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    MakeTreeNode(0,true,true);
                }));
                Logger.Error(ex.StackTrace);
            }
        }

        private string GetFolderNameToAdd()
        {
            var noteSelected = ApplicationContext.NodesSelected.Count > 0
                    ? ApplicationContext.NodesSelected[0].NodeId
                    : 1;
            var subNodes = ApplicationContext.FolderListAll.Where(e => e.ParentId == noteSelected).Select(e => e);
            //Find new folder count
            var labelName = "New folder";
            var numberCounts = new List<int>();
            foreach (var lbel in subNodes)
            {
                var header = lbel.FolderName;
                if (header.IndexOf("New folder", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    var headerUp = header.ToLower();
                    string[] separatingChars = { "new folder" };
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
            if (maxCount >= 2)
            {
                labelName += " (" + maxCount + ")";
            }
            return labelName;
        }


        private string GetComputerNameToAdd(int nodeSelectedId = -1)
        {
            var noteSelected = 1;
            if(nodeSelectedId==-1)
                noteSelected = ApplicationContext.NodesSelected.Count > 0 ? ApplicationContext.NodesSelected[0].NodeId : 1;
            else
            {
                noteSelected = nodeSelectedId;
            }
            var subNodes = ApplicationContext.EndPointListAll.Where(e => e.FolderId == noteSelected).Select(e => e);
            //Find new folder count
            var labelName = "New computer";
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
            if (maxCount >= 2)
            {
                labelName += " (" + maxCount + ")";
            }
            return labelName;
        }
        /// <summary>
        ///     Editing name of directory
        /// </summary>
        /// <param name="de">Edited directory</param>
        private void EditFolder(DirectoryEndpoint de)
        {
            try
            {
                Logger.Info("Starting edit a Directory");

                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var data = JsonConvert.SerializeObject(de);
                    sc.EditFolder(EncryptionHelper.EncryptString(data, KeyEncryption));
                }
                
                Logger.Info("Ended editing name of directory");
            }
            catch (Exception ex)
            {
                //refresh tree data
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    var dn = new DirectoryNode { IsFolder = true, NodeId = de.FolderId };
                    MakeTreeNode(dn, false, true);
                    var messageDialog = _view.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.ShowMessageDialog(
                        "Cannot edit a new directory due to exception occured, please see the log file under the Logs for more information",
                        "Message");
                }));
                Logger.Error(ex.StackTrace);
            }
        }

        /// <summary>
        ///     Editing name of endpoint
        /// </summary>
        /// <param name="de">Edited Endpoint</param>
        private void EditEndpoint(DirectoryEndpoint de)
        {
            try
            {
                Logger.Info("Starting edit an endpoint");

                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var data = JsonConvert.SerializeObject(de);
                    sc.EditEndpoint(EncryptionHelper.EncryptString(data, KeyEncryption));
                    //refresh labels tree data
                    _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
                        rightViewModel.LoadLabelView(true);
                    }));
                }
                
                Logger.Info("Ended editing name of endpoint");
            }
            catch (Exception ex)
            {
                //refresh tree data
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    var dn = new DirectoryNode { IsFolder = false, NodeId = de.FolderId };
                    MakeTreeNode(dn,false,true);
                    var messageDialog = _view.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.ShowMessageDialog(
                        "Cannot edit a new endpoint due to exception occured, please see the log file under the Logs for more information",
                        "Message");
                }));
                Logger.Error(ex.StackTrace);
            }
        }

        /// <summary>
        ///     Deleting directories and endpoints that selected
        /// </summary>
        /// <param name="data">a collection of directoryId</param>
        private void DeleteDirectoriesAndEndpoints(List<DeleteDirectoryFolderRequest> data)
        {
            try
            {
                Logger.Info("Starting delete directory and endpoint");

                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var directtoryData = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(data), KeyEncryption);

                    sc.DeleteDirectoryComputer(directtoryData);
                    
                }
                
                Logger.Info("Ended delete directory and endpoint");
            }
            catch (Exception ex)
            {
                //refresh tree data
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    MakeTreeNode(0, true, true);
                    var messageDialog = _view.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.ShowMessageDialog(
                        "Cannot delete this tree nodes due to exception occured, please see the log file under the Logs for more information",
                        "Message");
                }));
                Logger.Error(ex.StackTrace);
            }
        }
        #endregion
    }
}
