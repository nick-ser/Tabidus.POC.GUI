using System.Collections.Generic;
using HarddiskCriteriaElement = Tabidus.POC.GUI.UserControls.DirectoryAssignment.HarddiskCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class HarddiskCriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private HarddiskCriteriaElement _view;

        public HarddiskCriteriaViewModel(HarddiskCriteriaElement view)
        {
            _view = view;
            CbHarddiskOpeItems = ApplicationContext.CbHarddiskOpeItems;
            CbHarddiskOpeSelected = CbHarddiskOpeItems.Count>0? CbHarddiskOpeItems[0]:"";
        }
        public HarddiskCriteriaViewModel(HarddiskCriteriaElement view, bool isVisibleBtn):this(view)
        {
            BtnAddVisible = isVisibleBtn;
        }
        private List<string> _cbHarddiskOpeItems;

        public List<string> CbHarddiskOpeItems
        {
            get { return _cbHarddiskOpeItems; }
            set
            {
                _cbHarddiskOpeItems = value;
                OnPropertyChanged("CbHarddiskOpeItems");
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

        private string _txtHarddiskCriteria;

        public string TxtHarddiskCriteria
        {
            get { return _txtHarddiskCriteria; }
            set
            {
                _txtHarddiskCriteria = value;
                OnPropertyChanged("TxtHarddiskCriteria");
            }
        }

        private string _cbHarddiskOpeSelected;

        public string CbHarddiskOpeSelected
        {
            get { return _cbHarddiskOpeSelected; }
            set
            {
                _cbHarddiskOpeSelected = value;
                OnPropertyChanged("CbHarddiskOpeSelected");
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