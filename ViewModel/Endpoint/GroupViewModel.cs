using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Endpoint
{
    public class GroupViewModel : PageViewModelBase
    {
        private static List<int> _selectedFolderIds = new List<int>();
        private static List<int> _selectedEndpointIds = new List<int>();
        private static string _searchKey = "";
        private static EndPointListPage _view;
        private static List<ListLableEndpointResponse> _listEndpoints;
        private static List<int> _eids;
        private bool _isAdd;
        private Guid _guid;
        private string _title;
        public GroupViewModel(EndPointListPage view)
        {
            EndPointList = new ObservableCollection<EndPoint>();
            _view = view;
            //get init data

            ViewList = new ObservableCollection<EndPoint>();
            ViewList = EndPointList;
            //set grid column width
            IdWidth = ApplicationContext.IDWidth;
            TypeWidth = ApplicationContext.TypeWidth;
            UserNameWidth = ApplicationContext.UserNameWidth;
            OSNameWidth = ApplicationContext.OSNameWidth;
            SystemNameWidth = ApplicationContext.SystemNameWidth;
            PowerStateWidth = ApplicationContext.PowerStateWidth;
            DomainWidth = ApplicationContext.DomainWidth;
            IPv4Width = ApplicationContext.IPv4Width;
            IPv6Width = ApplicationContext.IPv6Width;
            LastSyncWidth = ApplicationContext.LastSyncWidth;
        }

        public GroupViewModel(EndPointListPage view, int selectedNodeId, string searchKey = "") : this(view)
        {
            _selectedFolderIds = new List<int>();
            _selectedEndpointIds = new List<int>();
            _selectedFolderIds.Add(selectedNodeId);
            _searchKey = searchKey;

            GetData();
        }

        public GroupViewModel(EndPointListPage view, List<int> folderIds, List<int> endpointIds, string searchKey = "")
            : this(view)
        {
            _searchKey = searchKey;
            _selectedFolderIds = folderIds;
            _selectedEndpointIds = endpointIds;
            GetData();
        }

        public GroupViewModel(EndPointListPage view, List<ListLableEndpointResponse> listEndpoints, List<int> eids)
            : this(view)
        {
            _listEndpoints = listEndpoints;
            _eids = eids;
            GetLabelData();
        }

        public void SetParamsValueForDirectory(List<int> folderIds, List<int> endpointIds, string searchKey, bool isAdd, Guid guid, string title)
        {
            _searchKey = searchKey;
            _selectedFolderIds = folderIds;
            _selectedEndpointIds = endpointIds;
            _isAdd = isAdd;
            _guid = guid;
            _title = title;
        }
        public void SetParamsValueForLabel(List<ListLableEndpointResponse> listEndpoints, List<int> eids)
        {
            _listEndpoints = listEndpoints;
            _eids = eids;
        }
        /// <summary>
        ///     Reload data for group view
        /// </summary>
        public void ReloadGroupView()
        {
            EndPointList = new ObservableCollection<EndPoint>();
            var endplst = new List<EndPoint>();
            foreach (var fid in _selectedFolderIds)
            {
                var endpointlist = GetDirectoryEndpoints(fid, _searchKey);
                foreach (var e in endpointlist)
                {
                    if (!(endplst.Select(ep => ep.EndpointId).Contains(e.EndpointId)))
                    {
                        endplst.Add(e);
                    }
                }
            }
            foreach (var eid in _selectedEndpointIds)
            {
                var endpoint = ApplicationContext.EndPointListAll.Find(e => e.EndpointId == eid);
                if (!(endplst.Select(ep => ep.EndpointId).Contains(endpoint.EndpointId)))
                {
                    endplst.Add(endpoint);
                }
            }
            foreach (var ep in endplst)
            {
                var dir = ApplicationContext.FolderListAll.Find(r => r.FolderId == ep.FolderId);
                var listNodes = new List<string>();
                GetPathNode(listNodes, dir);
                listNodes.Reverse();
                ep.ID = string.Join(" | ", listNodes);
                EndPointList.Add(ep);
            }
            if (_selectedFolderIds.Count > 0)
            {
                var dirSelected = ApplicationContext.FolderListAll.Find(r => r.FolderId == _selectedFolderIds[0]);
                var listParentId = new List<string>();
                GetPathNode(listParentId, dirSelected);
                listParentId.Reverse();
                FolderPathName = string.Join(" | ", listParentId);
            }
            TotalEndpoints = endplst.Count == 0
                ? ""
                : endplst.Count == 1 ? endplst.Count + " Endpoint" : endplst.Count + " Endpoints";
            GetDataSuccess();
        }

        public override void Refresh()
        {
            ReloadGroupView();
        }

        private void GetDataSuccess()
        {
            var headerViewModel = _view.GroupHeaderElement.DataContext as GroupHeaderViewModel;
            if (headerViewModel != null)
            {
                ApplicationContext.TotalEndpoint = headerViewModel.TotalEndpoints = TotalEndpoints;
                if (_selectedFolderIds.Count >= 2 || _selectedEndpointIds.Count >= 2)
                {
                    ApplicationContext.FolderPathName = headerViewModel.FolderPathName = "Selected Objects";
                }
                else
                {
                    ApplicationContext.FolderPathName = headerViewModel.FolderPathName = FolderPathName;
                }
            }
        }

        private void LoadData()
        {
            var endPointList = new ObservableCollection<EndPoint>();
            var endplst = new List<EndPoint>();
            foreach (var fid in _selectedFolderIds)
            {
                GetAllEndpointOfFolder(endplst, fid);
            }
            foreach (var eid in _selectedEndpointIds)
            {
                var endpoint = ApplicationContext.EndPointListAll.Find(e => e.EndpointId == eid);
                if (!(endplst.Select(ep => ep.EndpointId).Contains(endpoint.EndpointId)))
                {
                    endplst.Add(endpoint);
                }
            }
            foreach (var ep in endplst)
            {
                var dir = ApplicationContext.FolderListAll.Find(r => r.FolderId == ep.FolderId);
                var listNodes = new List<string>();
                GetPathNode(listNodes, dir);
                listNodes.Reverse();
                ep.ID = string.Join(" | ", listNodes);
                endPointList.Add(ep);
            }
            if (_selectedFolderIds.Count > 0)
            {
                var dirSelected = ApplicationContext.FolderListAll.Find(r => r.FolderId == _selectedFolderIds[0]);
                var listParentId = new List<string>();
                GetPathNode(listParentId, dirSelected);
                listParentId.Reverse();
                FolderPathName = string.Join(" | ", listParentId);
            }
            TotalEndpoints = endplst.Count == 0
                ? ""
                : endplst.Count == 1 ? endplst.Count + " Endpoint" : endplst.Count + " Endpoints";
            ViewList = endPointList;
        }

        public void GetAllEndpointOfFolder(List<EndPoint> lend, int fid)
        {
            foreach (var end in ApplicationContext.EndPointListTree)
            {
                if (end.FolderId == fid)
                {
                    if (!(lend.Select(ep => ep.EndpointId).Contains(end.EndpointId)))
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
        private void EditOrAddData()
        {
            if (_isAdd)
            {
                
                    var dirSubSelected = ApplicationContext.FolderListAll.Find(r => r.Guid == _guid);
                var dirSelected = ApplicationContext.FolderListAll.Find(r => r.FolderId == dirSubSelected.ParentId);
                var listParentId = new List<string>();
                    GetPathNode(listParentId, dirSelected);
                    listParentId.Reverse();
                    FolderPathName = string.Join(" | ", listParentId) + " | " + _title;
                
            }
            else
            {
                if (_selectedFolderIds.Count > 0)
                {
                    var dirSelected = ApplicationContext.FolderListAll.Find(r => r.FolderId == _selectedFolderIds[0]);
                    var listParentId = new List<string>();
                    GetPathNode(listParentId, dirSelected);
                    listParentId.Reverse();
                    FolderPathName = string.Join(" | ", listParentId);
                    var endlst = new ObservableCollection<EndPoint>();
                    foreach (var end in ViewList)
                    {
                        var dir = ApplicationContext.FolderListAll.Find(r => r.FolderId == end.FolderId);
                        var listNodes = new List<string>();
                        GetPathNode(listNodes, dir);
                        listNodes.Reverse();
                        end.ID = string.Join(" | ", listNodes);
                        endlst.Add(end);
                    }
                    ViewList = endlst;
                }
            }
            
        }


        public void GetData()
        {
            var backgroundWk = new BackgroundWorker();
            backgroundWk.DoWork += BackgroundWk_DoWork;
            backgroundWk.RunWorkerCompleted += BackgroundWk_RunWorkerCompleted;
            backgroundWk.RunWorkerAsync();
        }

        public void EditOrAdd()
        {
            var adBgWk = new BackgroundWorker();
            adBgWk.DoWork += AdBgWk_DoWork;
            adBgWk.RunWorkerCompleted += BackgroundWk_RunWorkerCompleted;
            adBgWk.RunWorkerAsync();
        }

        
        private void AdBgWk_DoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;
            EditOrAddData();
        }

        public void GetLabelData()
        {
            var backgroundWk = new BackgroundWorker();
            backgroundWk.DoWork += LabelBackgroundWk_DoWork;
            backgroundWk.RunWorkerCompleted += LabelBackgroundWk_RunWorkerCompleted;
            backgroundWk.RunWorkerAsync();
        }

        private void BackgroundWk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            GetDataSuccess();
            IsBusy = false;
        }

        private void BackgroundWk_DoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;
            LoadData();
        }
        private void LabelBackgroundWk_DoWork(object sender, DoWorkEventArgs e)
        {
            IsBusy = true;
            var endPointList = new ObservableCollection<EndPoint>();
            foreach (var enpointLabel in _listEndpoints)
            {
                var res = enpointLabel.Result;


                foreach (var le in res)
                {
                    foreach (var en in le.EndPointDatas)
                    {
                        if (!endPointList.Select(r => r.EndpointId).Contains(en.EndpointId))
                        {
                            var endpoint = new EndPoint(en);
                            endpoint.ID = enpointLabel.Message;
                            endPointList.Add(endpoint);
                        }
                        
                    }
                }
            }
            foreach (var endp in ApplicationContext.EndPointListAll)
            {
                if (_eids.Contains(endp.EndpointId))
                {
                    foreach (var el in ApplicationContext.LableEndpointDatas)
                    {
                        if (el.EndPointDatas.Find(r => r.EndpointId == endp.EndpointId) != null)
                        {
                            endp.ID = el.Name;
                            break;
                        }
                    }
                    if (!endPointList.Select(r => r.EndpointId).ToList().Contains(endp.EndpointId))
                    {
                        endPointList.Add(endp);
                    }
                }
            }
            EndPointList = endPointList;
            FolderPathName = (_listEndpoints.Count + _eids.Count) == 1 ? _listEndpoints[0].Message : "Selected Objects";
            TotalEndpoints = EndPointList.Count == 0
                ? ""
                : EndPointList.Count == 1 ? EndPointList.Count + " Endpoint" : EndPointList.Count + " Endpoints";
            
        }
        private void LabelBackgroundWk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ViewList = EndPointList;
            var headerViewModel = _view.GroupHeaderElement.DataContext as GroupHeaderViewModel;
            if (headerViewModel != null)
            {
                ApplicationContext.TotalEndpoint = headerViewModel.TotalEndpoints = TotalEndpoints;

                ApplicationContext.FolderPathName = headerViewModel.FolderPathName = FolderPathName;
            }
            IsBusy = false;
        }
        #region Private

        /// <summary>
        ///     Get the node path of endpoint, example: Company | Belarus | Misk
        /// </summary>
        /// <param name="listNode"></param>
        /// <param name="dir"></param>
        private void GetPathNode(List<string> listNode, Directory dir)
        {
            if (dir == null) return;
            listNode.Add(dir.FolderName);
            foreach (var ep in ApplicationContext.FolderListAll)
            {
                if (dir.ParentId == ep.FolderId)
                {
                    GetPathNode(listNode, ep);
                    break;
                }
            }
        }

        #endregion

        #region Grid column width

        public int IdWidth { get; set; }
        public int TypeWidth { get; set; }
        public int IPv4Width { get; set; }
        public int IPv6Width { get; set; }
        public int DomainWidth { get; set; }
        public int PowerStateWidth { get; set; }
        public int SystemNameWidth { get; set; }
        public int UserNameWidth { get; set; }
        public int OSNameWidth { get; set; }
        public int LastSyncWidth { get; set; }

        #endregion

        #region Fields And Properties

        private ObservableCollection<EndPoint> _viewList;

        public ObservableCollection<EndPoint> ViewList
        {
            get { return _viewList; }
            set
            {
                _viewList = value;
                OnPropertyChanged("ViewList");
            }
        }


        public ObservableCollection<EndPoint> EndPointList { get; set; }

        private string _folderPathName;

        public string FolderPathName
        {
            get { return _folderPathName; }
            set
            {
                _folderPathName = value;
                OnPropertyChanged("FolderPathName");
            }
        }

        private string _totalEndpoints;

        public string TotalEndpoints
        {
            get { return _totalEndpoints; }
            set
            {
                _totalEndpoints = value;
                OnPropertyChanged("TotalEndpoints");
            }
        }

        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        #endregion
    }
}