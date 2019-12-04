using System.Collections.Generic;
using MemoryCriteriaElement = Tabidus.POC.GUI.UserControls.Label.MemoryCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.Label
{
    public class MemoryCriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private MemoryCriteriaElement _view;

        public MemoryCriteriaViewModel(MemoryCriteriaElement view)
        {
            _view = view;
            CbMemoryOpeItems = ApplicationContext.CbMemoryOpeItems;
            CbMemoryOpeSelected = CbMemoryOpeItems.Count>0? CbMemoryOpeItems[0]:"";
        }
        public MemoryCriteriaViewModel(MemoryCriteriaElement view, bool isVisibleBtn):this(view)
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
        private List<string> _cbMemoryOpeItems;

        public List<string> CbMemoryOpeItems
        {
            get { return _cbMemoryOpeItems; }
            set
            {
                _cbMemoryOpeItems = value;
                OnPropertyChanged("CbMemoryOpeItems");
            }
        }

        private string _cbMemoryOpeSelected;

        public string CbMemoryOpeSelected
        {
            get { return _cbMemoryOpeSelected; }
            set
            {
                _cbMemoryOpeSelected = value;
                OnPropertyChanged("CbMemoryOpeSelected");
            }
        }

        private string _txtMemoryCriteria;

        public string TxtMemoryCriteria
        {
            get { return _txtMemoryCriteria; }
            set
            {
                _txtMemoryCriteria = value;
                OnPropertyChanged("TxtMemoryCriteria");
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