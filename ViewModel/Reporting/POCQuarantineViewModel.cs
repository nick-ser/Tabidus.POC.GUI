using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Reporting
{
    public class POCQuarantineViewModel : PageViewModelBase
    {
        private ObservableCollection<QuarantineExtended> _quarantines;
        public ObservableCollection<QuarantineExtended> Quarantines
        {
            get { return _quarantines; }
            set
            {
                _quarantines = value;
                OnPropertyChanged("Quarantines");
            }
        }

        public override void Refresh()
        {
            if (0 == 0) {
            }
            //Functions.GetAllSoftware();
            //Functions.GetAllUpdateSourceSoftware();
            //BuidPage();
        }

        private POCQuarantinePage View { get; set; }

        public POCQuarantineViewModel(POCQuarantinePage view)
        {
            View = view;
            Quarantines = new ObservableCollection<QuarantineExtended>();
            foreach (var item in getQuarantineItems(new List<int>()))
            {
                Quarantines.Add(item);
            }
        }

        public void RefreshQuarantineByComputerIds(IEnumerable<int> endpointIds)
        {
            Quarantines.Clear();
            foreach (var item in getQuarantineItems(endpointIds))
            {
                Quarantines.Add(item);
            }
        }

        private void RefreshQuarantineByComputerIds(IEnumerable<EndPoint> endPoints)
        {
            var endpointIds = endPoints.Select(e => e.EndpointId).ToList();
            Quarantines.Clear();
            
            foreach (var item in getQuarantineItems(endpointIds))
            {
                Quarantines.Add(item);
            }
        }

        public void RefreshQuarantineByFolder(int folderId)
        {
            var endPoints = new List<EndPoint>();
            GetAllEndpointOfFolder(endPoints, folderId);
            RefreshQuarantineByComputerIds(endPoints);
        }

        public void RefreshByGroupEntities(IEnumerable<int> fids, IEnumerable<int> eids)
        {
            var folderIds = fids.ToList();
            var endpointIds = eids.ToList();
            var endPoints = new List<EndPoint>();
            foreach (var folderId in folderIds)
            {
                GetAllEndpointOfFolder(endPoints, folderId);
            }
            foreach (var endpointId in endpointIds)
            {
                var endpoint = ApplicationContext.EndPointListTree.FirstOrDefault(e => e.EndpointId == endpointId);
                if(endPoints.FirstOrDefault(e => e.EndpointId == endpoint?.EndpointId) == null)
                    endPoints.Add(endpoint);
            }
            RefreshQuarantineByComputerIds(endPoints);
        }

        private void GetAllEndpointOfFolder(List<EndPoint> lend, int fid)
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

        private static List<QuarantineExtended> getQuarantineItems(IEnumerable<int> endpointIDs)
        {
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<QuarantineExtended>>(
                sc.GetQuarantine,
                endpointIDs));

            if (resultDeserialize == null)
            {
                PageNavigatorHelper._MainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                    {
                        var messageDialog =
                            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                        messageDialog.ShowMessageDialog("Data is null", "Message");
                    }));
                return new List<QuarantineExtended>();
            }

            return resultDeserialize;
        }

    }
}
