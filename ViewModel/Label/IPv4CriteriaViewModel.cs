using System.Collections.Generic;
using IPv4CriteriaElement = Tabidus.POC.GUI.UserControls.Label.IPv4CriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.Label
{
    public class IPv4CriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private IPv4CriteriaElement _view;

        public IPv4CriteriaViewModel(IPv4CriteriaElement view)
        {
            _view = view;
            CbIPv4OpeItems = ApplicationContext.CbIPv4OpeItems;
            CbIPv4OpeSelected = CbIPv4OpeItems[0];
        }
        public IPv4CriteriaViewModel(IPv4CriteriaElement view, bool isVisibleBtn):this(view)
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
        private List<string> _cbIPv4OpeItems;

        public List<string> CbIPv4OpeItems
        {
            get { return _cbIPv4OpeItems; }
            set
            {
                _cbIPv4OpeItems = value;
                OnPropertyChanged("CbIPv4OpeItems");
            }
        }

        private string _cbIPv4OpeSelected;

        public string CbIPv4OpeSelected
        {
            get { return _cbIPv4OpeSelected; }
            set
            {
                _cbIPv4OpeSelected = value;
                OnPropertyChanged("CbIPv4OpeSelected");
            }
        }

        private string _txtIPv4Criteria1;

        public string TxtIPv4Criteria1
        {
            get { return _txtIPv4Criteria1; }
            set
            {
                _txtIPv4Criteria1 = value;
                OnPropertyChanged("TxtIPv4Criteria1");
            }
        }

        private string _txtIPv4Criteria2;

        public string TxtIPv4Criteria2
        {
            get { return _txtIPv4Criteria2; }
            set
            {
                _txtIPv4Criteria2 = value;
                OnPropertyChanged("TxtIPv4Criteria2");
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