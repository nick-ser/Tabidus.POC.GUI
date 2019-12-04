using System.Collections.Generic;
using ComputerTypeCriteriaElement = Tabidus.POC.GUI.UserControls.DirectoryAssignment.ComputerTypeCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class ComputerTypeCriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private ComputerTypeCriteriaElement _view;

        public ComputerTypeCriteriaViewModel(ComputerTypeCriteriaElement view)
        {
            _view = view;
            CbComputerTypeOpeItems = ApplicationContext.CbComputerTypeOpeItems;
            CbComputerTypeOpeSelected = CbComputerTypeOpeItems.Count > 0 ? CbComputerTypeOpeItems[0] : "";
        }
        public ComputerTypeCriteriaViewModel(ComputerTypeCriteriaElement view, bool isVisibleBtn):this(view)
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

        private List<string> _cbComputerTypeOpeItems;

        public List<string> CbComputerTypeOpeItems
        {
            get { return _cbComputerTypeOpeItems; }
            set
            {
                _cbComputerTypeOpeItems = value;
                OnPropertyChanged("CbComputerTypeOpeItems");
            }
        }

        private string _cbComputerTypeOpeSelected;

        public string CbComputerTypeOpeSelected
        {
            get { return _cbComputerTypeOpeSelected; }
            set
            {
                _cbComputerTypeOpeSelected = value;
                OnPropertyChanged("CbComputerTypeOpeSelected");
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