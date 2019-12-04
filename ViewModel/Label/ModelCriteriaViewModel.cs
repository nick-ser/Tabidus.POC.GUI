using System.Collections.Generic;
using ModelCriteriaElement = Tabidus.POC.GUI.UserControls.Label.ModelCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.Label
{
    public class ModelCriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private ModelCriteriaElement _view;

        public ModelCriteriaViewModel(ModelCriteriaElement view)
        {
            _view = view;
            CbModelOpeItems = ApplicationContext.CbModelOpeItems;
            CbModelOpeSelected = CbModelOpeItems.Count>0? CbModelOpeItems[0]:"";
            CbModelCriteriaItems = ApplicationContext.CbModelCriteriaItems;
            CbModelCriteriaSelected = CbModelCriteriaItems.Count > 0 ? CbModelCriteriaItems[0] : "";
        }
        public ModelCriteriaViewModel(ModelCriteriaElement view, bool isVisibleBtn):this(view)
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
        private List<string> _cbModelOpeItems;

        public List<string> CbModelOpeItems
        {
            get { return _cbModelOpeItems; }
            set
            {
                _cbModelOpeItems = value;
                OnPropertyChanged("CbModelOpeItems");
            }
        }

        private string _txtModelCriteria;

        public string TxtModelCriteria
        {
            get { return _txtModelCriteria; }
            set
            {
                _txtModelCriteria = value;
                OnPropertyChanged("TxtModelCriteria");
            }
        }

        private List<string> _cbModelCriteriaItems;

        public List<string> CbModelCriteriaItems
        {
            get { return _cbModelCriteriaItems; }
            set
            {
                _cbModelCriteriaItems = value;
                OnPropertyChanged("CbModelCriteriaItems");
            }
        }

        private string _cbModelCriteriaSelected;

        public string CbModelCriteriaSelected
        {
            get { return _cbModelCriteriaSelected; }
            set
            {
                _cbModelCriteriaSelected = value;
                OnPropertyChanged("CbModelCriteriaSelected");
            }
        }

        private string _cbModelOpeSelected;

        public string CbModelOpeSelected
        {
            get { return _cbModelOpeSelected; }
            set
            {
                _cbModelOpeSelected = value;
                OnPropertyChanged("CbModelOpeSelected");
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