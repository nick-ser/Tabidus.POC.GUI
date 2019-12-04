using System.Collections.Generic;
using VendorCriteriaElement = Tabidus.POC.GUI.UserControls.DirectoryAssignment.VendorCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class VendorCriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private VendorCriteriaElement _view;

        public VendorCriteriaViewModel(VendorCriteriaElement view)
        {
            _view = view;
            CbVendorOpeItems = ApplicationContext.CbVendorOpeItems;
            CbVendorOpeSelected = CbVendorOpeItems.Count>0? CbVendorOpeItems[0]:"";
            CbVendorCriteriaItems = ApplicationContext.CbVendorCriteriaItems;
            CbVendorCriteriaSelected = CbVendorCriteriaItems.Count > 0 ? CbVendorCriteriaItems[0] : "";
        }
        public VendorCriteriaViewModel(VendorCriteriaElement view, bool isVisibleBtn):this(view)
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
        private List<string> _cbVendorCriteriaItems;

        public List<string> CbVendorCriteriaItems
        {
            get { return _cbVendorCriteriaItems; }
            set
            {
                _cbVendorCriteriaItems = value;
                OnPropertyChanged("CbVendorCriteriaItems");
            }
        }

        private string _cbVendorCriteriaSelected;

        public string CbVendorCriteriaSelected
        {
            get { return _cbVendorCriteriaSelected; }
            set
            {
                _cbVendorCriteriaSelected = value;
                OnPropertyChanged("CbVendorCriteriaSelected");
            }
        }

        private List<string> _cbVendorOpeItems;

        public List<string> CbVendorOpeItems
        {
            get { return _cbVendorOpeItems; }
            set
            {
                _cbVendorOpeItems = value;
                OnPropertyChanged("CbVendorOpeItems");
            }
        }

        private string _cbVendorOpeSelected;

        public string CbVendorOpeSelected
        {
            get { return _cbVendorOpeSelected; }
            set
            {
                _cbVendorOpeSelected = value;
                OnPropertyChanged("CbVendorOpeSelected");
            }
        }

        private string _txtVendorCriteria;

        public string TxtVendorCriteria
        {
            get { return _txtVendorCriteria; }
            set
            {
                _txtVendorCriteria = value;
                OnPropertyChanged("TxtVendorCriteria");
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
        private bool _btnDelVisible =true;
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