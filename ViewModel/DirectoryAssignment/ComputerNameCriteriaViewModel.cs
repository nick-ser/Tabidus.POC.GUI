using System.Collections.Generic;
using Tabidus.POC.GUI.Common;
using ComputerNameCriteriaElement = Tabidus.POC.GUI.UserControls.DirectoryAssignment.ComputerNameCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class ComputerNameCriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        private List<string> _cbComputerOpeItems;
        private string _cbComputerOpeSelected;
        private string _txtComputerCriteria;
        
        private ComputerNameCriteriaElement _view;

        public ComputerNameCriteriaViewModel(ComputerNameCriteriaElement view)
        {
            _view = view;
            CbComputerOpeItems = ApplicationContext.CbComputerOpeItems;
            CbComputerOpeSelected = CbComputerOpeItems[0];
        }
        public ComputerNameCriteriaViewModel(ComputerNameCriteriaElement view, bool isVisibleBtn):this(view)
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

        public List<string> CbComputerOpeItems
        {
            get { return _cbComputerOpeItems; }
            set
            {
                _cbComputerOpeItems = value;
                OnPropertyChanged("CbComputerOpeItems");
               
            }
        }

        public string CbComputerOpeSelected
        {
            get { return _cbComputerOpeSelected; }
            set
            {
                _cbComputerOpeSelected = value;
                OnPropertyChanged("CbComputerOpeSelected");
            }
        }

        public string TxtComputerCriteria
        {
            get { return _txtComputerCriteria; }
            set
            {
                _txtComputerCriteria = value;
                OnPropertyChanged("TxtComputerCriteria");
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