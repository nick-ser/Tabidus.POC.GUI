using System.Collections.Generic;
using ColorCodeCriteriaElement = Tabidus.POC.GUI.UserControls.DirectoryAssignment.ColorCodeCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class ColorCodeCriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private ColorCodeCriteriaElement _view;

        public ColorCodeCriteriaViewModel(ColorCodeCriteriaElement view)
        {
            _view = view;
            CbColorCodeOpeItems = ApplicationContext.CbColorCodeOpeItems;
            CbColorCodeOpeSelected = CbColorCodeOpeItems.Count>0? CbColorCodeOpeItems[0]:"";
        }
        public ColorCodeCriteriaViewModel(ColorCodeCriteriaElement view, bool isVisibleBtn):this(view)
        {
            BtnAddVisible = isVisibleBtn;
        }
        private List<string> _cbColorCodeOpeItems;

        public List<string> CbColorCodeOpeItems
        {
            get { return _cbColorCodeOpeItems; }
            set
            {
                _cbColorCodeOpeItems = value;
                OnPropertyChanged("CbColorCodeOpeItems");
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

        private string _cbColorCodeOpeSelected;

        public string CbColorCodeOpeSelected
        {
            get { return _cbColorCodeOpeSelected; }
            set
            {
                _cbColorCodeOpeSelected = value;
                OnPropertyChanged("CbColorCodeOpeSelected");
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