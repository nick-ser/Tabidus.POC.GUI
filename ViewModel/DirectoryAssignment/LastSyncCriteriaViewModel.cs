using System.Collections.Generic;
using LastSyncCriteriaElement = Tabidus.POC.GUI.UserControls.DirectoryAssignment.LastSyncCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class LastSyncCriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private LastSyncCriteriaElement _view;

        public LastSyncCriteriaViewModel(LastSyncCriteriaElement view)
        {
            _view = view;
            CbLastSyncOpeItems = ApplicationContext.CbLastSyncOpeItems;
            CbLastSyncOpeSelected = CbLastSyncOpeItems.Count>0? CbLastSyncOpeItems[0]:"";
        }
        public LastSyncCriteriaViewModel(LastSyncCriteriaElement view, bool isVisibleBtn):this(view)
        {
            BtnAddVisible = isVisibleBtn;
        }
        private int _criteriaId;
        public int CriteriaId
        {
            get { return _criteriaId; }
            set
            {
                _criteriaId = value;
                OnPropertyChanged("CriteriaId");
            }
        }
        private List<string> _cbLastSyncOpeItems;

        public List<string> CbLastSyncOpeItems
        {
            get { return _cbLastSyncOpeItems; }
            set
            {
                _cbLastSyncOpeItems = value;
                OnPropertyChanged("CbLastSyncOpeItems");
            }
        }

        private string _cbLastSyncOpeSelected;

        public string CbLastSyncOpeSelected
        {
            get { return _cbLastSyncOpeSelected; }
            set
            {
                _cbLastSyncOpeSelected = value;
                OnPropertyChanged("CbLastSyncOpeSelected");
            }
        }

        private string _txtLastSyncCriteria;

        public string TxtLastSyncCriteria
        {
            get { return _txtLastSyncCriteria; }
            set
            {
                _txtLastSyncCriteria = value;
                OnPropertyChanged("TxtLastSyncCriteria");
            }
        }

        public bool BtnAddVisible
        {
            get { return _btnAddVisible; }
            set
            {
                _btnAddVisible = value;
                OnPropertyChanged("BtnAddVisible");
            }
        }
        private bool _btnDelVisible = true;
        public bool BtnDelVisible
        {
            get { return _btnDelVisible; }
            set
            {
                _btnDelVisible = value;
                OnPropertyChanged("BtnDelVisible");
            }
        }

        private bool _labelOrVisible = true;
        public bool LabelOrVisible
        {
            get { return _labelOrVisible; }
            set
            {
                _labelOrVisible = value;
                OnPropertyChanged("LabelOrVisible");
            }
        }
    }
}