using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.UserControls.Software;

namespace Tabidus.POC.GUI.ViewModel.Software
{
    public class UpdateSourceElementViewModel : ViewModelBase
    {
        private int _currentSize;

        private bool _isSelected;

        private int _totalSize;
        private bool _progressVisible;
        private string _updateSourceName;
        private UpdateSourceElement _view;

        public UpdateSourceElementViewModel(UpdateSourceElement view)
        {
            _view = view;
            _view.Loaded += view_Loaded;
        }

        private void view_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!ApplicationContext.UpdateSourceElementViewModelsSelected.Select(r => r.Id).Contains(Id))
            {
                ApplicationContext.UpdateSourceElementViewModelsSelected.Add(this);
            }
        }
        
        public int TotalSize
        {
            get { return _totalSize; }
            set
            {
                _totalSize = value;
                OnPropertyChanged("TotalSize");
            }
        }

        public int CurrentSize
        {
            get { return _currentSize; }
            set
            {
                _currentSize = value;
                OnPropertyChanged("CurrentSize");
            }
        }

        public string UpdateSourceName
        {
            get { return _updateSourceName; }
            set
            {
                _updateSourceName = value;
                OnPropertyChanged("UpdateSourceName");
            }
        }
        //To give effects when checkbox is selected on 22-04-19
        public string ExpanderBackgroundColor
        {
            get { return IsSelected ? "#331dabed" : "#08FFFFFF"; }
        }

        public string Bordercolor
        {
            get { return IsSelected ? "#1dabed" : "Transparent"; }
        }

        public string TextColor
        {
            get { return IsSelected ? "#FFFFFF" : "#8e8f93"; }
        }
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
                OnPropertyChanged("ExpanderBackgroundColor");
                OnPropertyChanged("TextColor");
                OnPropertyChanged("Bordercolor");
            }
        }


        public bool ProgressVisible
        {
            get { return _progressVisible; }
            set
            {
                _progressVisible = value;
                OnPropertyChanged("ProgressVisible");
            }
        }
		
		public int Id { get; set; }
        public string UpdateSourceUrl { get; set; }
    }
}