using System.Collections.Generic;
using IPv6CriteriaElement = Tabidus.POC.GUI.UserControls.DirectoryAssignment.IPv6CriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class IPv6CriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private IPv6CriteriaElement _view;

        public IPv6CriteriaViewModel(IPv6CriteriaElement view)
        {
            _view = view;
            CbIPv6OpeItems = ApplicationContext.CbIPv6OpeItems;
            CbIPv6OpeSelected = CbIPv6OpeItems.Count>0? CbIPv6OpeItems[0]:"";
        }
        public IPv6CriteriaViewModel(IPv6CriteriaElement view, bool isVisibleBtn):this(view)
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
        private List<string> _cbIPv6OpeItems;

        public List<string> CbIPv6OpeItems
        {
            get { return _cbIPv6OpeItems; }
            set
            {
                _cbIPv6OpeItems = value;
                OnPropertyChanged("CbIPv6OpeItems");
            }
        }

        private string _cbIPv6OpeSelected;

        public string CbIPv6OpeSelected
        {
            get { return _cbIPv6OpeSelected; }
            set
            {
                _cbIPv6OpeSelected = value;
                OnPropertyChanged("CbIPv6OpeSelected");
            }
        }

        private string _txtIPv6Criteria1;

        public string TxtIPv6Criteria1
        {
            get { return _txtIPv6Criteria1; }
            set
            {
                _txtIPv6Criteria1 = value;
                OnPropertyChanged("TxtIPv6Criteria1");
            }
        }

        private string _txtIPv6Criteria2;

        public string TxtIPv6Criteria2
        {
            get { return _txtIPv6Criteria2; }
            set
            {
                _txtIPv6Criteria2 = value;
                OnPropertyChanged("TxtIPv6Criteria2");
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