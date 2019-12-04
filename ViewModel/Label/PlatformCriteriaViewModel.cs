using System.Collections.Generic;
using Tabidus.POC.Common.Model;
using PlatformCriteriaElement = Tabidus.POC.GUI.UserControls.Label.PlatformCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.Label
{
    public class PlatformCriteriaViewModel : ViewModelBase
    {
        private bool _btnAddVisible;
        
        private PlatformCriteriaElement _view;

        public PlatformCriteriaViewModel(PlatformCriteriaElement view)
        {
            _view = view;
            CbPlatformOpeItems = ApplicationContext.CbPlatformOpeItems;
            CbPlatformOpeSelected = CbPlatformOpeItems.Count>0? CbPlatformOpeItems[0].Value:0;
        }
        public PlatformCriteriaViewModel(PlatformCriteriaElement view, bool isVisibleBtn):this(view)
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
        private List<ComboboxItem> _cbPlatformOpeItems;

        public List<ComboboxItem> CbPlatformOpeItems
        {
            get { return _cbPlatformOpeItems; }
            set
            {
                _cbPlatformOpeItems = value;
                OnPropertyChanged("CbPlatformOpeItems");
            }
        }

        private int _cbPlatformOpeSelected;

        public int CbPlatformOpeSelected
        {
            get { return _cbPlatformOpeSelected; }
            set
            {
                _cbPlatformOpeSelected = value;
                OnPropertyChanged("CbPlatformOpeSelected");
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