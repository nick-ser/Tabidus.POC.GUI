using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;
using ColorCodeControl = Tabidus.POC.GUI.UserControls.Endpoint.ColorCodeControl;

namespace Tabidus.POC.GUI.ViewModel.Endpoint
{
    /// <summary>
    /// Class ColorCodePageViewModel.
    /// </summary>
    public class ColorCodePageViewModel : ViewModelBase
    {
        /// <summary>
        /// The _directory node
        /// </summary>
        private DirectoryNode _directoryNode;

        private ColorCodePage _view;
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorCodePageViewModel"/> class.
        /// </summary>
        /// <param name="directoryNode">The directory node.</param>
        public ColorCodePageViewModel(DirectoryNode directoryNode, ColorCodePage view)
        {
            _directoryNode = directoryNode;
            _view = view;
            Refresh();
            RefreshSuccessfully();
        }
        public ColorCodePageViewModel(ColorCodePage view)
        {
            _view = view;
        }
        public void GetData()
        {
            var backgroundWk = new BackgroundWorker();
            backgroundWk.DoWork += BackgroundWk_DoWork;
            backgroundWk.RunWorkerCompleted += BackgroundWk_RunWorkerCompleted;
            backgroundWk.RunWorkerAsync();
        }

        public void SetParams(DirectoryNode directoryNode)
        {
            _directoryNode = directoryNode;
        }
        private void BackgroundWk_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            RefreshSuccessfully();
            var headerViewModel = _view.GroupHeaderElement.DataContext as GroupHeaderViewModel;
            if (headerViewModel != null)
            {
                headerViewModel.TotalEndpoints = _totalEndpoints;
                headerViewModel.FolderPathName = _pathName;

            }
        }

        private void BackgroundWk_DoWork(object sender, DoWorkEventArgs e)
        {
            Refresh();
            LoadData();
        }

        private void RefreshSuccessfully()
        {
            ColorCodeControlCollection = new ObservableCollection<ColorCodeControl>();
            foreach (var colorModel in _colorModelList)
            {
                ColorCodeControlCollection.Add(BuildColorCodeControl(colorModel));
            }
        }
        private string _totalEndpoints;
        private string _pathName;
        private void LoadData()
        {
            var endplst = new List<EndPoint>();
            
                var endpointlist = GetDirectoryEndpoints(_directoryNode.NodeId);
                foreach (var e in endpointlist)
                {
                    if (!(endplst.Select(ep => ep.EndpointId).Contains(e.EndpointId)))
                    {
                        endplst.Add(e);
                    }
                }
            
                var dirSelected = ApplicationContext.FolderListAll.Find(r => r.FolderId == _directoryNode.NodeId);
                var listParentId = new List<string>();
                GetPathNode(listParentId, dirSelected);
                listParentId.Reverse();
            _pathName = string.Join(" | ", listParentId);
            
            _totalEndpoints = endplst.Count == 0
                ? ""
                : endplst.Count == 1 ? endplst.Count + " Endpoint" : endplst.Count + " Endpoints";

        }

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

        private List<ColorModel> _colorModelList;

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public override void Refresh()
        {
            _colorModelList = ServiceManager.Invoke(
                sc => RequestResponseUtils.GetData<List<ColorModel>>(
                    sc.GetColorModels, new ColorDataRequest(_directoryNode.NodeId)));
        }
        /// <summary>
        /// Builds the color code control.
        /// </summary>
        /// <param name="colorModel">The color model.</param>
        /// <returns>ColorCodeControl.</returns>
        private ColorCodeControl BuildColorCodeControl(ColorModel colorModel)
        {
            return new ColorCodeControl(colorModel);
        }
        /// <summary>
        /// The _color code control collection
        /// </summary>
        private ObservableCollection<ColorCodeControl> _colorCodeControlCollection;
        /// <summary>
        /// Gets or sets the color code control collection.
        /// </summary>
        /// <value>The color code control collection.</value>
        public ObservableCollection<ColorCodeControl> ColorCodeControlCollection
        {
            get
            {
                return _colorCodeControlCollection;
            }
            set
            {
                _colorCodeControlCollection = value;
                OnPropertyChanged("ColorCodeControlCollection");
            }
        }
    }
}
