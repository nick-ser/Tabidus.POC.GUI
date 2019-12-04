using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ViewModel.MainWindowView;
using ColorCodeControl = Tabidus.POC.GUI.UserControls.Endpoint.ColorCodeControl;

namespace Tabidus.POC.GUI.ViewModel.Endpoint
{
    /// <summary>
    /// Class ColorCodeViewModel.
    /// </summary>
    public class ColorCodeViewModel : INotifyPropertyChanged
    {
        #region Private Variable
        /// <summary>
        /// The _main view model
        /// </summary>
        private readonly MainWindowViewModel _mainViewModel;
        /// <summary>
        /// The _view
        /// </summary>
        private ColorCodeControl _view;
        /// <summary>
        /// The _can save
        /// </summary>
        private bool _canSave = true;
        /// <summary>
        /// list of endpoint
        /// </summary>
        private List<EndPoint> _endpointList;
        #endregion
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorCodeViewModel"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="colorModel">The color model.</param>
        public ColorCodeViewModel(ColorCodeControl view, ColorModel colorModel)
        {
            _view = view;
            UpdateModel(colorModel);
            _view.Model = this;
            _mainViewModel = PageNavigatorHelper.GetMainModel();
        }
        #endregion
        #region Properties
        /// <summary>
        /// The _folder identifier
        /// </summary>
        private int _folderId;

        /// <summary>
        /// Gets or sets the folder identifier.
        /// </summary>
        /// <value>The folder identifier.</value>
        public int FolderId
        {
            get { return _folderId; }
            set { _folderId = value; OnPropertyChanged("FolderId"); }
        }
        /// <summary>
        /// The _color code
        /// </summary>
        private string _colorCode;

        /// <summary>
        /// Gets or sets the color code.
        /// </summary>
        /// <value>The color code.</value>
        public string ColorCode
        {
            get { return _colorCode; }
            set { _colorCode = value; OnPropertyChanged("ColorCode"); }
        }
        /// <summary>
        /// The _inherited
        /// </summary>
        private bool? _inherited;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="ColorCodeViewModel"/> is inherited.
        /// </summary>
        /// <value><c>null</c> if [inherited] contains no value, <c>true</c> if [inherited]; otherwise, <c>false</c>.</value>
        public bool? Inherited
        {
            get { return _inherited; }
            set { 
                _inherited = value;
                if (value.HasValue && value.Value)
                {
                    OnPropertyChanged("Inherited");
                    if (_canSave)
                    {
                        var data = new UpdateColumnColorRequest(FolderId, ColorCode, string.Empty, string.Empty);
                        var bgSave = new BackgroundWorker();
                        bgSave.DoWork += bgSave_DoWork;
                        bgSave.RunWorkerCompleted += bgSave_RunWorkerCompleted;
                        bgSave.RunWorkerAsync(data);
                    }
                }
                else
                {
                    OnPropertyChanged("Inherited", value); 
                }
            }
        }
        /// <summary>
        /// The _agent not installed
        /// </summary>
        private bool? _agentNotInstalled;

        /// <summary>
        /// Gets or sets a value indicating whether [agent not installed].
        /// </summary>
        /// <value><c>null</c> if [agent not installed] contains no value, <c>true</c> if [agent not installed]; otherwise, <c>false</c>.</value>
        public bool? AgentNotInstalled
        {
            get { return _agentNotInstalled; }
            set { _agentNotInstalled = value; OnPropertyChanged("AgentNotInstalled", value); }
        }
        /// <summary>
        /// The _last synchronize day
        /// </summary>
        private int? _lastSyncDay;

        /// <summary>
        /// Gets or sets the last synchronize day.
        /// </summary>
        /// <value>The last synchronize day.</value>
        public int? LastSyncDay
        {
            get { return _lastSyncDay; }
            set 
            {
                _lastSyncDay = value; 
                OnPropertyChanged("LastSyncDay", value);
                OnPropertyChanged("CanLastSyncDay");
            }
        }
        /// <summary>
        /// The _inherited from
        /// </summary>
        private int? _inheritedFrom;

        /// <summary>
        /// Gets or sets the inherited from.
        /// </summary>
        /// <value>The inherited from.</value>
        public int? InheritedFrom
        {
            get { return _inheritedFrom; }
            set { _inheritedFrom = value; OnPropertyChanged("InheritedFrom", value); }
        }
        private string _inheritedName;

        /// <summary>
        /// Gets or sets the name of the inherited.
        /// </summary>
        /// <value>The name of the inherited.</value>
        public string InheritedName
        {
            get { return _inheritedName; }
            set 
            {
                _inheritedName = value; 
                OnPropertyChanged("InheritedName");
                OnPropertyChanged("CanInherited");
            }
        }
        public bool CanInherited
        {
            get
            {
                return !string.IsNullOrWhiteSpace(InheritedName);
            }
        }
        public bool CanLastSyncDay
        {
            get
            {
                return LastSyncDay.HasValue;
            }
            set
            {
                if (!value)
                {
                    LastSyncDay = null;
                    OnPropertyChanged("CanLastSyncDay");
                }
            }
        }
        #endregion
        #region Public Function

        #endregion
        #region Private Function
        private void RefreshTreeRight(int folderId)
        {
            //Refresh Right Tree
            PageNavigatorHelper.GetRightElementViewModel().UpdateTree(folderId);
        }
        /// <summary>
        /// Updates the model.
        /// </summary>
        /// <param name="model">The model.</param>
        private void UpdateModel(ColorModel model)
        {
            _canSave = false;
            FolderId = model.FolderId;
            ColorCode = model.ColorCode;
            Inherited = model.Inherited;
            AgentNotInstalled = model.AgentNotInstalled;
            LastSyncDay = model.LastSyncDay;
            InheritedFrom = model.InheritedFrom;
            InheritedName = model.InheritedName;
            _canSave = true;
        }
        #endregion
        #region INotifyPropertyChanged
        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void OnPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        private void OnPropertyChanged(string propertyName, object value)
        {
            OnPropertyChanged(propertyName);
            if (_canSave)
            {
                var data = new UpdateColumnColorRequest(FolderId, ColorCode, propertyName,
                    value == null ? "null" : GetValue(value));

                var bgSave = new BackgroundWorker();
                bgSave.DoWork += bgSave_DoWork;
                bgSave.RunWorkerCompleted += bgSave_RunWorkerCompleted;
                bgSave.RunWorkerAsync(data);
            }
        }

        void bgSave_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _canSave = true;
            
            SetColorForTreeNodes(_endpointList);
            var rightView = PageNavigatorHelper.GetRightElementViewModel();
            rightView.ReBuildTree(ApplicationContext.NodesSelected);
            rightView.LoadLabelView(true);
            var bgSave = sender as BackgroundWorker;
            if (bgSave != null)
            {
                bgSave.DoWork -= bgSave_DoWork;
                bgSave.RunWorkerCompleted -= bgSave_RunWorkerCompleted;
            }
        }

        void bgSave_DoWork(object sender, DoWorkEventArgs e)
        {
            _canSave = false;
            var data = e.Argument as UpdateColumnColorRequest;
            if (data != null)
            {
                if (!string.IsNullOrWhiteSpace(data.ColumnName))
                {
                    ServiceManager.Invoke(sc =>
                        RequestResponseUtils.GetData<UpdateColumnColorResponse>(
                            sc.UpdateColumnColor, data));
                }
                else
                {
                    var result = ServiceManager.Invoke(sc =>
                                        RequestResponseUtils.GetData<UpdateColumnColorResponse>(
                                            sc.UpdateColorByParent, data));
                    if (result != null && result.Status)
                    {
                        AgentNotInstalled = result.AgentNotInstalled;
                        LastSyncDay = result.LastSyncDay;
                    }
                }
                var respone = new EndpointSearch
                {
                    FolderId = FolderId
                };
                var endpList = ServiceManager.Invoke(sc =>
                                        RequestResponseUtils.GetData<List<EndPointData>>(
                                            sc.GetDirectoryEndpointColor, respone));
                _endpointList = new List<EndPoint>();
                foreach (var endd in endpList)
                {
                    var end = new EndPoint(endd);
                    _endpointList.Add(end);
                }
            }
        }

        private void SetColorForTreeNodes(List<EndPoint> nodes)
        {

            foreach (var node in nodes)
            {
                SetDirNodeColor(node);
            }
            
        }
        
        private void SetDirNodeColor(EndPoint innode)
        {
            foreach (var end in ApplicationContext.EndPointListTree)
            {
                if (end.EndpointId == innode.EndpointId)
                {
                    end.Color = innode.Color;
                    end.LastSyncDayText = innode.LastSyncDayText;
                    end.LastSyncDay = innode.LastSyncDay;
                }
            }
        }

        private void SetLabelNodeColor(EndPoint innode)
        {
            for (int i = 0; i < ApplicationContext.LableEndpointDatas.Count; i++)
            {
                for (int j = 0; j < ApplicationContext.LableEndpointDatas[i].EndPointDatas.Count; j++)
                {
                    var end = ApplicationContext.LableEndpointDatas[i].EndPointDatas[j];
                    if (innode.EndpointId == end.EndpointId)
                    {
                        end.ColorCode = innode.Color;
                        end.LastSyncDay = innode.LastSyncDay;
                        break;
                    }
                }

            }
        }

        private string GetValue(object value)
        {
            if (null == value)
            {
                return "null";
            }
            if (value is bool)
                return (bool)value ? "1" : "0";
            return value.ToString();
        }
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
