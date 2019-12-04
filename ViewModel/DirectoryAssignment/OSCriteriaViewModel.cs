using System.Collections.Generic;
using OSCriteriaElement = Tabidus.POC.GUI.UserControls.DirectoryAssignment.OSCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class OSCriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private OSCriteriaElement _view;

        public OSCriteriaViewModel(OSCriteriaElement view)
        {
            _view = view;
            CbOsOpeItems = ApplicationContext.CbOsOpeItems;
            CbOsOpeSelected = CbOsOpeItems.Count>0? CbOsOpeItems[0]:"";
            CbOsCriteriaItems = ApplicationContext.CbOsCriteriaItems;
            CbOsCriteriaSelected = CbOsCriteriaItems.Count > 0 ? CbOsCriteriaItems[0] : "";
        }
        public OSCriteriaViewModel(OSCriteriaElement view, bool isVisibleBtn):this(view)
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
        private List<string> _cbOsOpeItems;

        public List<string> CbOsOpeItems
        {
            get { return _cbOsOpeItems; }
            set
            {
                _cbOsOpeItems = value;
                OnPropertyChanged("CbOsOpeItems");
            }
        }

        private List<string> _cbOsCriteriaItems;

        public List<string> CbOsCriteriaItems
        {
            get { return _cbOsCriteriaItems; }
            set
            {
                _cbOsCriteriaItems = value;
                OnPropertyChanged("CbOsCriteriaItems");
            }
        }

        private string _cbOsCriteriaSelected;

        public string CbOsCriteriaSelected
        {
            get { return _cbOsCriteriaSelected; }
            set
            {
                _cbOsCriteriaSelected = value;
                OnPropertyChanged("CbOsCriteriaSelected");
            }
        }

        private string _cbOsOpeSelected;

        public string CbOsOpeSelected
        {
            get { return _cbOsOpeSelected; }
            set
            {
                _cbOsOpeSelected = value;
                OnPropertyChanged("CbOsOpeSelected");
            }
        }

        private string _txtOsCriteria;

        public string TxtOsCriteria
        {
            get { return _txtOsCriteria; }
            set
            {
                _txtOsCriteria = value;
                OnPropertyChanged("TxtOsCriteria");
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