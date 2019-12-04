using System.Collections.Generic;
using DomainCriteriaElement = Tabidus.POC.GUI.UserControls.Label.DomainCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.Label
{
    public class DomainCriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private DomainCriteriaElement _view;

        public DomainCriteriaViewModel(DomainCriteriaElement view)
        {
            _view = view;
            CbDomainOpeItems = ApplicationContext.CbDomainOpeItems;
            CbDomainOpeSelected = CbDomainOpeItems.Count>0? CbDomainOpeItems[0]:"";
            CbDomainCriteriaItems = ApplicationContext.CbDomainCriteriaItems;
            CbDomainCriteriaSelected = CbDomainCriteriaItems.Count > 0 ? CbDomainCriteriaItems[0] : "";
        }
        public DomainCriteriaViewModel(DomainCriteriaElement view, bool isVisibleBtn):this(view)
        {
            BtnAddVisible = isVisibleBtn;
        }
        private List<string> _cbDomainCriteriaItems;

        public List<string> CbDomainCriteriaItems
        {
            get { return _cbDomainCriteriaItems; }
            set
            {
                _cbDomainCriteriaItems = value;
                OnPropertyChanged("CbDomainCriteriaItems");
            }
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

        private string _cbDomainCriteriaSelected;

        public string CbDomainCriteriaSelected
        {
            get { return _cbDomainCriteriaSelected; }
            set
            {
                _cbDomainCriteriaSelected = value;
                OnPropertyChanged("CbDomainCriteriaSelected");
            }
        }

        private List<string> _cbDomainOpeItems;

        public List<string> CbDomainOpeItems
        {
            get { return _cbDomainOpeItems; }
            set
            {
                _cbDomainOpeItems = value;
                OnPropertyChanged("CbDomainOpeItems");
            }
        }

        private string _cbDomainOpeSelected;

        public string CbDomainOpeSelected
        {
            get { return _cbDomainOpeSelected; }
            set
            {
                _cbDomainOpeSelected = value;
                OnPropertyChanged("CbDomainOpeSelected");
            }
        }

        private string _txtDomainCriteria;

        public string TxtDomainCriteria
        {
            get { return _txtDomainCriteria; }
            set
            {
                _txtDomainCriteria = value;
                OnPropertyChanged("TxtDomainCriteria");
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