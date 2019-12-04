using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;
using LabelCriteriaElement = Tabidus.POC.GUI.UserControls.Label.LabelCriteriaElement;

namespace Tabidus.POC.GUI.ViewModel.Label
{
    public class LabelCriteriaViewModel : ViewModelBase
    {
        private readonly LabelCriteriaElement _view;
        public ICommand DeleteLabelCommand { get; private set; }
        private bool _isBusy;

        public LabelCriteriaViewModel(LabelCriteriaElement view)
        {
            _view = view;
            DeleteLabelCommand = new RelayCommand(OnDeleteLabel);
        }

        //private bool CanExecuteCommand(object pars)
        //{
        //    return !IsBusy;
        //}

        //public bool IsBusy
        //{
        //    get { return _isBusy; }
        //    set
        //    {
        //        _isBusy = value;
        //        OnPropertyChanged("IsBusy");
        //    }
        //}
        //private void OnDeleteLabel(object pars)
        //{
        //    try
        //    {
        //        var labelCris = _view.PnlLabelContainer.Children;
        //        var lbids = new List<int>();
        //        foreach (var ex in labelCris)
        //        {
        //            if (ex.GetType() == typeof(LabelCriteriaElement))
        //            {
        //                var expander = (ex as LabelCriteriaElement).Expander;
        //                var chbheader = expander.FindChild<CheckBox>("CbExpandHeader");
        //                if (chbheader != null && chbheader.IsChecked == true)
        //                {
        //                    lbids.Add(((ex as LabelCriteriaElement).DataContext as LabelCriteriaViewModel).LabelId);
        //                }
        //            }
        //        }

        //        if (lbids.Count == 0)
        //        {
        //            var messageDialog =
        //                PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
        //            messageDialog.ShowMessageDialog("please select at least a label to delete", "Delete Label");
        //        }
        //        else
        //        {
        //            var confirmdialog = new ConfirmDialog();
        //            confirmdialog.ConfirmText.Text = "Are you sure you want to delete selected labels?";
        //            confirmdialog.BtnOk.Focus();
        //            if (confirmdialog.ShowDialog() == true)
        //            {
        //                Logger.Info("Starting delete label");
        //                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
        //                {
        //                    var request = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(lbids),
        //                        KeyEncryption);
        //                    var result = sc.DeleteLabel(request);
        //                    var resultDeserialize =
        //                        JsonConvert.DeserializeObject<LabelCriteriaEndpointList>(
        //                            EncryptionHelper.DecryptRijndael(result,
        //                                KeyEncryption));
        //                    if (resultDeserialize == null)
        //                    {
        //                        var messageDialog =
        //                            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
        //                        messageDialog.ShowMessageDialog("Data is null", "Get label data");
        //                    }

        //                    BuildLabelPage(resultDeserialize);
        //                    //Reload LabelFilterViewModel
        //                    foreach (var lid in lbids)
        //                    {
        //                        if (ApplicationContext.LabelNodesSelected.Select(l => l.NodeId).Contains(lid))
        //                        {
        //                            ApplicationContext.LabelNodesSelected.Remove(
        //                                ApplicationContext.LabelNodesSelected.Find(r => r.NodeId == lid));
        //                        }
        //                    }
        //                    ApplicationContext.IsRebuildTree = true;
        //                    RightTreeViewModel.LoadLabelView(true);
        //                }
        //                Logger.Info("Ended delete label");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex);

        //        var messageDialog =
        //            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
        //        messageDialog.ShowMessageDialog(
        //            "Cannot delete label due to exception occured, please see the log file under the Logs for more information",
        //            "Delete label");
        //    }
        //}

        private void OnDeleteLabel(object arg)
        {
            var assignmentViewModel = PageNavigatorHelper.GetMainContentViewModel<LabelViewModel>();
            assignmentViewModel.OnDeleteLabel(LabelId);
        }
        public void SetAddState()
        {
            var btnHeader = _view.Expander.FindChild<Button>("BtnExpandHeader");
            var tbHeader = _view.Expander.FindChild<TextBox>("TbExpandHeader");
            btnHeader.Visibility = Visibility.Hidden;
            tbHeader.Visibility = Visibility.Visible;
            tbHeader.Focus();
        }

        #region Properties

        #region Checkbox

        private bool _chbComputerChecked;

        public bool ChbComputerChecked
        {
            get { return _chbComputerChecked; }
            set
            {
                _chbComputerChecked = value;
                OnPropertyChanged("ChbComputerChecked");
            }
        }

        private bool _chbVendorChecked;

        public bool ChbVendorChecked
        {
            get { return _chbVendorChecked; }
            set
            {
                _chbVendorChecked = value;
                OnPropertyChanged("ChbVendorChecked");
            }
        }

        private bool _chbModelChecked;

        public bool ChbModelChecked
        {
            get { return _chbModelChecked; }
            set
            {
                _chbModelChecked = value;
                OnPropertyChanged("ChbModelChecked");
            }
        }

        private bool _chbOsChecked;

        public bool ChbOsChecked
        {
            get { return _chbOsChecked; }
            set
            {
                _chbOsChecked = value;
                OnPropertyChanged("ChbOsChecked");
            }
        }

        private bool _chbPlatformChecked;

        public bool ChbPlatformChecked
        {
            get { return _chbPlatformChecked; }
            set
            {
                _chbPlatformChecked = value;
                OnPropertyChanged("ChbPlatformChecked");
            }
        }

        private bool _chbComputerTypeChecked;

        public bool ChbComputerTypeChecked
        {
            get { return _chbComputerTypeChecked; }
            set
            {
                _chbComputerTypeChecked = value;
                OnPropertyChanged("ChbComputerTypeChecked");
            }
        }

        private bool _chbDomainChecked;

        public bool ChbDomainChecked
        {
            get { return _chbDomainChecked; }
            set
            {
                _chbDomainChecked = value;
                OnPropertyChanged("ChbDomainChecked");
            }
        }

        private bool _chbMemoryChecked;

        public bool ChbMemoryChecked
        {
            get { return _chbMemoryChecked; }
            set
            {
                _chbMemoryChecked = value;
                OnPropertyChanged("ChbMemoryChecked");
            }
        }

        private bool _chbHarddiskChecked;

        public bool ChbHarddiskChecked
        {
            get { return _chbHarddiskChecked; }
            set
            {
                _chbHarddiskChecked = value;
                OnPropertyChanged("ChbHarddiskChecked");
            }
        }

        private bool _chbIPv4Checked;

        public bool ChbIPv4Checked
        {
            get { return _chbIPv4Checked; }
            set
            {
                _chbIPv4Checked = value;
                OnPropertyChanged("ChbIPv4Checked");
            }
        }

        private bool _chbIPv6Checked;

        public bool ChbIPv6Checked
        {
            get { return _chbIPv6Checked; }
            set
            {
                _chbIPv6Checked = value;
                OnPropertyChanged("ChbIPv6Checked");
            }
        }

        private bool _chbLastSyncChecked;

        public bool ChbLastSyncChecked
        {
            get { return _chbLastSyncChecked; }
            set
            {
                _chbLastSyncChecked = value;
                OnPropertyChanged("ChbLastSyncChecked");
            }
        }

        private bool _chbColorCodeChecked;

        public bool ChbColorCodeChecked
        {
            get { return _chbColorCodeChecked; }
            set
            {
                _chbColorCodeChecked = value;
                OnPropertyChanged("ChbColorCodeChecked");
            }
        }

        #endregion


        private string _labelName;

        public string LabelName
        {
            get { return _labelName; }
            set
            {
                _labelName = value;
                OnPropertyChanged("LabelName");
            }
        }

        private int _labelId;

        public int LabelId
        {
            get { return _labelId; }
            set
            {
                _labelId = value;
                OnPropertyChanged("LabelId");
            }
        }

        private bool _isAddState;

        public bool IsAddState
        {
            get { return _isAddState; }
            set
            {
                _isAddState = value;
                OnPropertyChanged("IsAddState");
            }
        }

        private bool _isNotAddState;

        public bool IsNotAddState
        {
            get { return !_isAddState; }
        }


        // Implement selected expander changes
        public string ExpanderBackgroundColor
        {
            //get { return IsActived ? "#284B51" : "#C6CCD8"; }
            get { return IsActived ? "#331dabed" : "#08FFFFFF"; }
        }

        public string Bordercolor
        {
            get { return IsActived ? "#1dabed" : "Transparent"; }
        }

        public string TextColor
        {
            get { return IsActived ? "#FFFFFF" : "#FFFFFF"; }
        }

        public string DeleteImagePath
        {
            get { return IsActived ? "../../Images/delete_white.png" : "../../Images/delete_gray.png"; }
        }

        public string checkbox_image
        {
            get { return IsActived ? "../../Images/check.png" : "../../Images/box.png"; }
        }
        private bool _isActived;

        public bool IsActived
        {
            get { return _isActived; }
            set
            {
                _isActived = value;
                OnPropertyChanged("IsActived");
                OnPropertyChanged("ExpanderBackgroundColor");
                OnPropertyChanged("TextColor");
                OnPropertyChanged("DeleteImagePath");
                OnPropertyChanged("Bordercolor");
                OnPropertyChanged("checkbox_image");
            }
        }
        private bool _chbUnmangedChecked;

        public bool ChbUnmangedChecked
        {
            get { return _chbUnmangedChecked; }
            set
            {
                _chbUnmangedChecked = value;
                OnPropertyChanged("ChbUnmangedChecked");
                OnPropertyChanged("IsDisableCriteria");
            }
        }

        public int UnmanagedCriteriaId { get; set; }
        private bool _chbMangedChecked;

        public bool ChbMangedChecked
        {
            get { return _chbMangedChecked; }
            set
            {
                _chbMangedChecked = value;
                OnPropertyChanged("ChbMangedChecked");
                OnPropertyChanged("IsDisableCriteria");
            }
        }
        private bool _isEnable;

        public bool IsEnable
        {
            get { return _isEnable; }
            set
            {
                _isEnable = value;
                OnPropertyChanged("IsEnable");
            }
        }

        public bool IsDisableCriteria
        {
            get { return !ChbUnmangedChecked && !ChbMangedChecked; }
        }
        #endregion

        public ICommand OnLabelElementChanged
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = param =>
                    {
                        PageNavigatorHelper.GetMainContentViewModel<LabelViewModel>().OnSaveLabel(null);
                    }
                };
            }
        }
    }
}