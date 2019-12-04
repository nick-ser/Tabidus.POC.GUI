using Tabidus.POC.GUI.UserControls.Policy;

namespace Tabidus.POC.GUI.ViewModel.Policy
{
    public class PolicyEnhancementViewModel : ViewModelBase
    {
        public readonly PolicyEnhancementElement _view;

        public PolicyEnhancementViewModel(PolicyEnhancementElement view)
        {
            _view = view;
        }

        #region Properties

        #region Checkbox

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        #endregion

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private int _id;

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        private bool _isAddState;

        public bool IsAddState
        {
            get { return _isAddState; }
            set
            {
                _isAddState = value;
                OnPropertyChanged("IsAddState");
            }
        }

        public bool IsNotAddState
        {
            get { return !_isAddState; }
        }

        private string _color;

        public string ExpanderBackgroundColor
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("ExpanderBackgroundColor");
            }
        }

        public string TextColor
        {
            get { return "#D2D2D3"; }
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
            }
        }

        public string POCServer
        {
            get { return _pOCServer; }
            set
            {
                _pOCServer = value;
                OnPropertyChanged("POCServer");
            }
        }

        public int Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }

        public int SynchronizationInterval
        {
            get { return _synchronizationInterval; }
            set
            {
                _synchronizationInterval = value;
                OnPropertyChanged("SynchronizationInterval");
            }
        }

        public int TransferInterval
        {
            get { return _transferInterval; }
            set
            {
                _transferInterval = value;
                OnPropertyChanged("TransferInterval");
            }
        }

        public bool NeighborhoodWatch
        {
            get { return _neighborhoodWatch; }
            set
            {
                _neighborhoodWatch = value;
                OnPropertyChanged("NeighborhoodWatch");
            }
        }

        public bool UpdateSource
        {
            get { return _updateSource; }
            set
            {
                _updateSource = value;
                OnPropertyChanged("UpdateSource");
            }
        }

        public bool ActiveTransfer
        {
            get { return _activeTransfer; }
            set
            {
                _activeTransfer = value;
                OnPropertyChanged("ActiveTransfer");
            }
        }

        private bool _activeTransfer;
        private string _key;
        private bool _neighborhoodWatch;
        private string _pOCServer;

        private int _port;

        private int _synchronizationInterval;
        private int _transferInterval;
        private bool _updateSource;

        #endregion
    }
}