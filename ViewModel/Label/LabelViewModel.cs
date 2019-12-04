using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using Newtonsoft.Json;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.UserControls.Label;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Label
{
    public class LabelViewModel : ViewModelBase
    {
        private static LabelsPage _view;
        private bool _isBusy;
        private int _lastActivedExpander;

        public LabelViewModel(LabelsPage view)
        {
            _view = view;
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
                mainViewModel.IsBusy = true;

            if (ApplicationContext.CbComputerOpeItems == null ||
                (ApplicationContext.CbComputerOpeItems != null && ApplicationContext.CbComputerOpeItems.Count <= 0))
            {
                //Creating items of comboboxs and default selected value
                ApplicationContext.CbComputerOpeItems = CreateComboboxItems("COMPUTER_ITEMS_KEY");

                ApplicationContext.CbVendorOpeItems = CreateComboboxItems("COMPUTER_ITEMS_KEY");

                ApplicationContext.CbModelOpeItems = CreateComboboxItems("COMPUTER_ITEMS_KEY");

                ApplicationContext.CbOsOpeItems = CreateComboboxItems("COMPUTER_ITEMS_KEY");

                ApplicationContext.CbDomainOpeItems = CreateComboboxItems("COMPUTER_ITEMS_KEY");

                ApplicationContext.CbPlatformOpeItems = new List<ComboboxItem>();
                var platformItems = CreateComboboxItems("PLATFORM_ITEMS_KEY");
                foreach (var ci in platformItems)
                {
                    var fli = ci.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    var cbi = new ComboboxItem { Text = fli[0], Value = int.Parse(fli[1]) };
                    ApplicationContext.CbPlatformOpeItems.Add(cbi);
                }

                ApplicationContext.CbComputerTypeOpeItems = CreateComboboxItems("COMPUTERTYPE_ITEMS_KEY");

                ApplicationContext.CbMemoryOpeItems = CreateComboboxItems("HARDDISK_ITEMS_KEY");

                ApplicationContext.CbHarddiskOpeItems = CreateComboboxItems("HARDDISK_ITEMS_KEY");

                ApplicationContext.CbIPv4OpeItems = CreateComboboxItems("IP_ITEMS_KEY");

                ApplicationContext.CbIPv6OpeItems = CreateComboboxItems("IPv6_ITEMS_KEY");

                ApplicationContext.CbLastSyncOpeItems = CreateComboboxItems("LASTSYNC_ITEMS_KEY");

                ApplicationContext.CbColorCodeOpeItems = CreateComboboxItems("COLORCODE_ITEMS_KEY");

                ApplicationContext.CbVendorCriteriaItems = CreateComboboxFromParameters(LabelParameters.Vendor);

                ApplicationContext.CbOsCriteriaItems = CreateComboboxFromParameters(LabelParameters.OperatingSystem);

                ApplicationContext.CbDomainCriteriaItems = CreateComboboxFromParameters(LabelParameters.Domain);

                ApplicationContext.CbModelCriteriaItems = CreateComboboxFromParameters(LabelParameters.Model);
            }
            BuildPage();
            AddLabelCommand = new RelayCommand(OnAddLabel, CanExecuteCommand);
            SaveLabelCommand = new RelayCommand(OnSaveLabel, CanExecuteCommand);
            DeleteLabelCommand = new RelayCommand<int>(OnDeleteLabel, CanDeleteRule);
            ResetLabelCommand = new RelayCommand(ResetCommandAction, CanExecuteCommand);
        }

        public RightTreeViewModel RightTreeViewModel
        {
            get { return PageNavigatorHelper.GetRightElementViewModel(); }
        }

        /// <summary>
        ///     Build label page
        /// </summary>
        public void BuildPage()
        {
            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        public void EditLabel(POC.Common.Model.Endpoint.Label label)
        {
            try
            {
                Logger.Info("Starting edit a label");
                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var req = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(label),
                        KeyEncryption);
                    var rs = sc.EditLabel(req);
                    var rsr = EncryptionHelper.DecryptStringToInt(rs, KeyEncryption);
                    if (rsr >0)
                    {
                        var lch = _view.PnlLabelContainer.Children;
                        var labelCri = new LabelCriteriaElement();
                        for (var i = lch.Count-1; i>=0; i--)
                        {
                            if (lch[i].GetType() == typeof (LabelCriteriaElement))
                            {
                                var lce = (LabelCriteriaElement) lch[i];
                                var lcvm = lce.Model;
                                if (lcvm.LabelId == label.LabelId)
                                {
                                    labelCri = lce;
                                    _view.PnlLabelContainer.Children.RemoveAt(i);
                                    break;
                                }
                            }
                        }
                        _view.PnlLabelContainer.Children.Insert(rsr-1, labelCri);
                        ApplicationContext.LableEndpointDatas.Find(r => r.Id == label.LabelId).Name = label.LabelName;
                    }
                    
                }
                Logger.Info("Ended edit a label");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                var messageDialog =
                    PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                messageDialog.ShowMessageDialog(
                    "Cannot edit a label due to exception occured, please see the log file under the Logs for more information",
                    "Edit Label");
            }
        }

        /// <summary>
        ///     Get items for combobox depend on input key
        /// </summary>
        /// <param name="configKey">mapping with appconfig key</param>
        /// <returns></returns>
        private List<string> CreateComboboxItems(string configKey)
        {
            var sitems = Functions.GetConfig(configKey, "");
            return sitems.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        /// <summary>
        ///     Get items for combobox depend on input key
        /// </summary>
        /// <param name="configKey">mapping with appconfig key</param>
        /// <returns></returns>
        private List<string> CreateComboboxFromParameters(LabelParameters parameterType)
        {
            using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            {
                return sc.GetLabelParameters(parameterType).ToList();
            }
        }

        private void ReloadPage()
        {
            try
            {
                Logger.Info("Starting get label datas");
                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var request = EncryptionHelper.EncryptString("OK",
                        KeyEncryption);
                    var result = sc.GetAllLabel(request);
                    var resultDeserialize =
                        JsonConvert.DeserializeObject<LabelCriteriaEndpointList>(
                            EncryptionHelper.DecryptRijndael(result,
                                KeyEncryption));
                    if (resultDeserialize == null)
                    {
                        _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
                        {
                            var messageDialog =
                                PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                            messageDialog.TxtMessageText.Text = "Data is null";
                            messageDialog.Visibility = Visibility.Visible;
                        }));
                    }

                    BuildLabelPage(resultDeserialize);
                }
                Logger.Info("Ended get label datas");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
                {
                    var messageDialog =
                        PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.TxtMessageText.Text =
                        "Cannot get label due to exception occured, please see the log file under the Logs for more information";
                    messageDialog.Visibility = Visibility.Visible;
                }));
            }
        }

        #region Properties and Commands

        public int LastActivedExpander
        {
            get { return _lastActivedExpander; }
            set
            {
                _lastActivedExpander = value;
                OnPropertyChanged("LastActivedExpander");
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public ICommand AddLabelCommand { get; private set; }
        public ICommand SaveLabelCommand { get; private set; }
        public ICommand DeleteLabelCommand { get; private set; }
        public ICommand ResetLabelCommand { get; private set; }

        #endregion

        #region Private methods

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
                mainViewModel.IsBusy = false;
            //Releases all resources used by BackgroundWorker
            var worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= BackgroundWorker_RunWorkerCompleted;
            worker.DoWork -= BackgroundWorker_DoWork;
            worker.Dispose();
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ReloadPage();
        }


        /// <summary>
        ///     Build label page again or reset the label selected
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="isReset"></param>
        private void BuildLabelPage(LabelCriteriaEndpointList datas, bool isReset = false)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                var lbids = GetListLabelExpanded();

                //get all datas to rebuild page
                if (!isReset)
                {
                    #region Get all datas to rebuild page

                    _view.PnlLabelContainer.Children.Clear();
                    foreach (var ld in datas.Labels)
                    {
                        var labelCriteriaElement = new LabelCriteriaElement();
                        var labelCriteriaViewModel = labelCriteriaElement.DataContext as LabelCriteriaViewModel;
                        labelCriteriaViewModel.LabelId = ld.LabelId;
                        labelCriteriaViewModel.LabelName = ld.LabelName;
                        if (lbids.Contains(ld.LabelId))
                        {
                            labelCriteriaElement.Expander.IsExpanded = true;
                        }
                        var lbcris =
                            datas.LabelCriterias.Select(r => r)
                                .Where(r => r.LabelId == ld.LabelId)
                                .ToList();

                        foreach (var lbcri in lbcris)
                        {
                            SetLabelCriteriaViewModel(labelCriteriaElement, lbcri);
                        }

                        _view.PnlLabelContainer.Children.Add(labelCriteriaElement);
                    }

                    #endregion
                }
                else
                {
                    //reset labels that checked
                    var labelCriterias = _view.PnlLabelContainer.Children;

                    foreach (var ex in labelCriterias)
                    {
                        if (ex.GetType() == typeof (LabelCriteriaElement))
                        {
                            var labelCriElem = ex as LabelCriteriaElement;
                            var expander = labelCriElem.Expander;
                            var labelCriViewMoel = labelCriElem.DataContext as LabelCriteriaViewModel;
                            foreach (var lbcri in datas.LabelCriterias)
                            {
                                if (lbcri.LabelId == labelCriViewMoel.LabelId)
                                {
                                    SetLabelCriteriaViewModel(labelCriElem, lbcri);
                                }
                            }
                        }
                    }
                }
            }));
        }


        /// <summary>
        ///     Set value for label criteria view model
        /// </summary>
        /// <param name="labelCriteriaElement">the label criteria view model that want to set values</param>
        /// <param name="lbcri">data to set to label criteria view model</param>
        private void SetLabelCriteriaViewModel(LabelCriteriaElement labelCriteriaElement, LabelCriteria lbcri)
        {
            var labelCriteriaViewModel = labelCriteriaElement.DataContext as LabelCriteriaViewModel;
            switch (lbcri.FieldName)
            {
                case CommonConstants.EndpointComputerName:
                    var count = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count++;
                        var computerNameopeCri = new ComputerNameCriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count)
                        {
                            computerNameopeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            computerNameopeCri.Model.BtnDelVisible = false;
                        }
                        if (count == 1)
                        {
                            computerNameopeCri.Model.LabelOrVisible = false;
                        }
                        computerNameopeCri.Model.CbComputerOpeSelected = loc.Operator;
                        computerNameopeCri.Model.TxtComputerCriteria = loc.Value1;
                        computerNameopeCri.Model.CriteriaId = loc.CriteriaId;
                        labelCriteriaElement.PnlComputerNameCri.Children.Add(computerNameopeCri);
                    }
                    labelCriteriaViewModel.ChbComputerChecked = lbcri.IsAvailable;
                    break;
                case CommonConstants.EndpointVendor:
                    var count2 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count2++;
                        var vendorOpeCri = new VendorCriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count2)
                        {
                            vendorOpeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            vendorOpeCri.Model.BtnDelVisible = false;
                        }
                        if (count2 == 1)
                        {
                            vendorOpeCri.Model.LabelOrVisible = false;
                        }
                        vendorOpeCri.Model.CbVendorOpeSelected = loc.Operator;
                        vendorOpeCri.Model.CriteriaId = loc.CriteriaId;
                        vendorOpeCri.Model.TxtVendorCriteria = loc.Operator != ConstantHelper.IsOperator
                            ? loc.Value1
                            : "";
                        vendorOpeCri.Model.CbVendorCriteriaSelected = loc.Operator == ConstantHelper.IsOperator
                            ? loc.Value1
                            : "";
                        labelCriteriaElement.PnlVendorCri.Children.Add(vendorOpeCri);
                    }
                    labelCriteriaViewModel.ChbVendorChecked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointModel:
                    var count3 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count3++;
                        var opeCri = new ModelCriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count3)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count3 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbModelOpeSelected = loc.Operator;
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        opeCri.Model.TxtModelCriteria = loc.Operator != ConstantHelper.IsOperator ? loc.Value1 : "";
                        opeCri.Model.CbModelCriteriaSelected = loc.Operator == ConstantHelper.IsOperator
                            ? loc.Value1
                            : "";
                        labelCriteriaElement.PnlModelCri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbModelChecked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointOperatingSystem:
                    var count4 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count4++;
                        var opeCri = new OSCriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count4)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count4 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbOsOpeSelected = loc.Operator;
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        opeCri.Model.TxtOsCriteria = loc.Operator != ConstantHelper.IsOperator ? loc.Value1 : "";
                        opeCri.Model.CbOsCriteriaSelected = loc.Operator == ConstantHelper.IsOperator ? loc.Value1 : "";
                        labelCriteriaElement.PnlOsCri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbOsChecked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointPlatform:
                    var count5 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count5++;
                        var opeCri = new PlatformCriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count5)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count5 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbPlatformOpeSelected = string.IsNullOrWhiteSpace(loc.Value1)?0:int.Parse(loc.Value1);
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        labelCriteriaElement.PnlPlatformCri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbPlatformChecked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointComputerType:
                    var count6 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count6++;
                        var opeCri = new ComputerTypeCriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count6)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count6 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbComputerTypeOpeSelected = ConvertComType(loc.Value1);
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        labelCriteriaElement.PnlComputerTypeCri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbComputerTypeChecked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointDomain:
                    var count7 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count7++;
                        var opeCri = new DomainCriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count7)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count7 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbDomainOpeSelected = loc.Operator;
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        opeCri.Model.TxtDomainCriteria = loc.Operator != ConstantHelper.IsOperator ? loc.Value1 : "";
                        opeCri.Model.CbDomainCriteriaSelected = loc.Operator == ConstantHelper.IsOperator
                            ? loc.Value1
                            : "";
                        labelCriteriaElement.PnlDomainCri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbDomainChecked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointIPv4:
                    var count8 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count8++;
                        var opeCri = new IPv4CriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count8)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count8 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbIPv4OpeSelected = loc.Operator;
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        opeCri.Model.TxtIPv4Criteria1 = loc.Value1;
                        opeCri.Model.TxtIPv4Criteria2 = loc.Value2;

                        labelCriteriaElement.PnlIPv4Cri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbIPv4Checked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointIPv6:
                    var count9 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count9++;
                        var opeCri = new IPv6CriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count9)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count9 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbIPv6OpeSelected = loc.Operator;
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        opeCri.Model.TxtIPv6Criteria1 = loc.Value1;
                        opeCri.Model.TxtIPv6Criteria2 = loc.Value2;

                        labelCriteriaElement.PnlIPv6Cri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbIPv6Checked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointMemory:
                    var count10 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count10++;
                        var opeCri = new MemoryCriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count10)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count10 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbMemoryOpeSelected = loc.Operator;
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        opeCri.Model.TxtMemoryCriteria = loc.Value1;
                        labelCriteriaElement.PnlMemoryCri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbMemoryChecked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointHarddisk:
                    var count11 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count11++;
                        var opeCri = new HarddiskCriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count11)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count11 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbHarddiskOpeSelected = loc.Operator;
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        opeCri.Model.TxtHarddiskCriteria = loc.Value1;
                        labelCriteriaElement.PnlHarddiskCri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbHarddiskChecked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointLastSynchronization:
                    var count12 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count12++;
                        var opeCri = new LastSyncCriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count12)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count12 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbLastSyncOpeSelected = loc.Operator;
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        opeCri.Model.TxtLastSyncCriteria = loc.Value1;
                        labelCriteriaElement.PnlLastSyncCri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbLastSyncChecked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointColorCode:
                    var count13 = 0;
                    foreach (var loc in lbcri.LabelOperatorCriterias)
                    {
                        count13++;
                        var opeCri = new ColorCodeCriteriaElement();
                        if (lbcri.LabelOperatorCriterias.Count == count13)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.LabelOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count13 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbColorCodeOpeSelected = loc.Value1;
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        labelCriteriaElement.PnlColorCodeCri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbColorCodeChecked = lbcri.IsAvailable;

                    break;
            }
        }

        private readonly Dictionary<string, ComputerTypes> _dictMapComputerType =
            new Dictionary<string, ComputerTypes>(StringComparer.OrdinalIgnoreCase)
            {
                {"Server", ComputerTypes.Server},
                {"Desktop", ComputerTypes.Desktop},
                {"Notebook", ComputerTypes.Notebook}
            };

        private string ConvertComType(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var vlue = int.Parse(value);
                switch (vlue)
                {
                    case (int) ComputerTypes.Server:
                        return ComputerTypes.Server.ToString();
                    case (int) ComputerTypes.Desktop:
                        return ComputerTypes.Desktop.ToString();
                    case (int) ComputerTypes.Notebook:
                        return ComputerTypes.Notebook.ToString();
                    default:
                        return ComputerTypes.Desktop.ToString();
                }
            }

            return value;
        }

        private ComputerTypes ConvertComTypeFromString(string value)
        {
            if (_dictMapComputerType.ContainsKey(value))
                return _dictMapComputerType[value];
            return ComputerTypes.Desktop;
        }

        /// <summary>
        ///     Build xml data when edit label criteria
        /// </summary>
        /// <returns></returns>
        private string BuildXmlDataEdit()
        {
            var labelCris = _view.PnlLabelContainer.Children;
            var xmlDataBuilder = new StringBuilder();
            xmlDataBuilder.Append("<DataSet>");
            foreach (var ex in labelCris)
            {
                if (ex.GetType() == typeof(LabelCriteriaElement))
                {
                    var labelCriElement = ex as LabelCriteriaElement;
                    var labelCriteriaViewModel =
                        labelCriElement.DataContext as LabelCriteriaViewModel;

                    if (labelCriteriaViewModel != null)
                    {
                        //Build the xml data
                        //data for computer name
                        var computerOpes = labelCriElement.PnlComputerNameCri.Children;
                        foreach (var co in computerOpes)
                        {
                            if (co.GetType() == typeof(ComputerNameCriteriaElement))
                            {
                                var cvm = (co as ComputerNameCriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbComputerChecked == true)
                                {
                                    foreach (var child in labelCriElement.PnlComputerNameCri.Children)
                                    {
                                        if (child.GetType() == typeof(ComputerNameCriteriaElement))
                                        {
                                            var ldapElem = child as ComputerNameCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbComputerOpes");
                                            ldapElem.FindChild<ComboBox>("CbComputerOpes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbComputerOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtComputerCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtComputerCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("bntplus").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("bntminus").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("bntplus").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("bntminus").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlComputerNameCri.Children)
                                    {
                                        if (child.GetType() == typeof(ComputerNameCriteriaElement))
                                        {
                                            var ldapElem = child as ComputerNameCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbComputerOpes");
                                            ldapElem.FindChild<ComboBox>("CbComputerOpes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbComputerOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtComputerCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<TextBox>("TxtComputerCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("bntplus").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("bntminus").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("bntplus").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("bntminus").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            //ldapElem.Model.LabelOrVisible = false;
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlComputerNameCri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbComputerChecked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointComputerName +
                                                      "</FieldName>");
                                xmlDataBuilder.Append("<Operator>" + cvm.CbComputerOpeSelected +
                                                      "</Operator>");
                                xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtComputerCriteria) + "</Value1>");
                                xmlDataBuilder.Append("<Value2></Value2>");
                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for vendor
                        var vendorOpes = labelCriElement.PnlVendorCri.Children;
                        foreach (var co in vendorOpes)
                        {
                            if (co.GetType() == typeof(VendorCriteriaElement))
                            {
                                var cvm = (co as VendorCriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbVendorChecked == true)
                                {
                                    foreach (var child in labelCriElement.PnlVendorCri.Children)
                                    {
                                        if (child.GetType() == typeof(VendorCriteriaElement))
                                        {
                                            var ldapElem = child as VendorCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbVendorOpes");
                                            ldapElem.FindChild<ComboBox>("CbVendorOpes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbVendorOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtVendorCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtVendorCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<ComboBox>("CbVendorCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbVendorCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("bntplus_vendor").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("bntminus_vendor").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("bntplus_vendor").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("bntminus_vendor").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlVendorCri.Children)
                                    {
                                        if (child.GetType() == typeof(VendorCriteriaElement))
                                        {
                                            var ldapElem = child as VendorCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbVendorOpes");
                                            ldapElem.FindChild<ComboBox>("CbVendorOpes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbVendorOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8e8f93");
                                            ldapElem.FindChild<TextBox>("TxtVendorCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<TextBox>("TxtVendorCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8e8f93");
                                            ldapElem.FindChild<ComboBox>("CbVendorCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbVendorCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8e8f93");
                                            ldapElem.FindChild<Button>("bntplus_vendor").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("bntminus_vendor").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("bntplus_vendor").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("bntminus_vendor").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            //ldapElem.Model.LabelOrVisible = false;
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlComputerNameCri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbVendorChecked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointVendor + "</FieldName>");
                                xmlDataBuilder.Append("<Operator>" + cvm.CbVendorOpeSelected +
                                                      "</Operator>");
                                xmlDataBuilder.Append("<Value1>" +
                                                      (cvm.CbVendorOpeSelected == ConstantHelper.IsOperator
                                                          ? cvm.CbVendorCriteriaSelected
                                                          : ReplaceTag(cvm.TxtVendorCriteria ?? "")) + "</Value1>");
                                xmlDataBuilder.Append("<Value2></Value2>");
                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for model
                        var modelOpes = labelCriElement.PnlModelCri.Children;
                        foreach (var co in modelOpes)
                        {
                            if (co.GetType() == typeof(ModelCriteriaElement))
                            {
                                var cvm = (co as ModelCriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbModelChecked == true)
                                {
                                    foreach (var child in labelCriElement.PnlModelCri.Children)
                                    {
                                        if (child.GetType() == typeof(ModelCriteriaElement))
                                        {
                                            var ldapElem = child as ModelCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbModelOpes");
                                            ldapElem.FindChild<ComboBox>("CbModelOpes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbModelOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtModelCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtModelCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<ComboBox>("CbModelCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbModelCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("bntplus_model").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("bntminus_model").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("bntplus_model").Foreground = (Brush)new BrushConverter().ConvertFrom("#fFF");
                                            ldapElem.FindChild<Button>("bntminus_model").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlModelCri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlModelCri.Children)
                                    {
                                        if (child.GetType() == typeof(ModelCriteriaElement))
                                        {
                                            var ldapElem = child as ModelCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbModelOpes");
                                            ldapElem.FindChild<ComboBox>("CbModelOpes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbModelOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtModelCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<TextBox>("TxtModelCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<ComboBox>("CbModelCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbModelCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8F93");
                                            ldapElem.FindChild<Button>("bntplus_model").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("bntminus_model").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("bntplus_model").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("bntminus_model").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");//ldapElem.Model.LabelOrVisible = false;
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlModelCri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbModelChecked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointModel + "</FieldName>");
                                xmlDataBuilder.Append("<Operator>" + cvm.CbModelOpeSelected +
                                                      "</Operator>");
                                xmlDataBuilder.Append("<Value1>" +
                                                      (cvm.CbModelOpeSelected == ConstantHelper.IsOperator
                                                          ? cvm.CbModelCriteriaSelected
                                                          : ReplaceTag(cvm.TxtModelCriteria ?? "")) + "</Value1>");
                                xmlDataBuilder.Append("<Value2></Value2>");
                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for operating system
                        var osOpes = labelCriElement.PnlOsCri.Children;
                        foreach (var co in osOpes)
                        {
                            if (co.GetType() == typeof(OSCriteriaElement))
                            {
                                var cvm = (co as OSCriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbOsChecked == true)
                                {
                                    foreach (var child in labelCriElement.PnlOsCri.Children)
                                    {
                                        if (child.GetType() == typeof(OSCriteriaElement))
                                        {
                                            var ldapElem = child as OSCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbOsOpes");
                                            ldapElem.FindChild<ComboBox>("CbOsOpes").Background =
                                                (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbOsOpes").Foreground =
                                                (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtOsCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtOsCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<ComboBox>("CbOsCriteria").Background =
                                                (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbOsCriteria").Foreground =
                                                (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnplus_os").Background =
                                                (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnminus_os").Background =
                                                (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnplus_os").Foreground =
                                                (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnminus_os").Foreground =
                                                (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlOsCri.Children)
                                    {
                                        if (child.GetType() == typeof(OSCriteriaElement))
                                        {
                                            var ldapElem = child as OSCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbOsOpes");
                                            ldapElem.FindChild<ComboBox>("CbOsOpes").Background =
                                                (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbOsOpes").Foreground =
                                                (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtOsCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<TextBox>("TxtOsCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<ComboBox>("CbOsCriteria").Background =
                                                (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbOsCriteria").Foreground =
                                                (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnplus_os").Background =
                                                (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnminus_os").Background =
                                                (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnplus_os").Foreground =
                                                (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnminus_os").Foreground =
                                                (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            //ldapElem.Model.LabelOrVisible = false;
                                        }
                                    }
                                }

                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbOsChecked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointOperatingSystem +
                                                      "</FieldName>");
                                xmlDataBuilder.Append("<Operator>" + cvm.CbOsOpeSelected +
                                                      "</Operator>");
                                xmlDataBuilder.Append("<Value1>" +
                                                      (cvm.CbOsOpeSelected == ConstantHelper.IsOperator
                                                          ? cvm.CbOsCriteriaSelected
                                                          : ReplaceTag(cvm.TxtOsCriteria ?? "")) + "</Value1>");
                                xmlDataBuilder.Append("<Value2></Value2>");
                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for Platform
                        var platformOpes = labelCriElement.PnlPlatformCri.Children;
                        foreach (var co in platformOpes)
                        {
                            if (co.GetType() == typeof(PlatformCriteriaElement))
                            {
                                var cvm = (co as PlatformCriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbPlatformChecked == true)
                                {
                                    foreach (var child in labelCriElement.PnlPlatformCri.Children)
                                    {
                                        if (child.GetType() == typeof(PlatformCriteriaElement))
                                        {
                                            var ldapElem = child as PlatformCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbPlatformOpes");
                                            ldapElem.FindChild<ComboBox>("CbPlatformOpes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbPlatformOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            //ldapElem.FindChild<TextBox>("TxtVendorCriteria").Background = new SolidColorBrush(Colors.Yellow);
                                            ldapElem.FindChild<Button>("btnplus_platform").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnminus_platform").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnplus_platform").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnminus_platform").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlPlatformCri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlPlatformCri.Children)
                                    {
                                        if (child.GetType() == typeof(PlatformCriteriaElement))
                                        {
                                            var ldapElem = child as PlatformCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbPlatformOpes");
                                            ldapElem.FindChild<ComboBox>("CbPlatformOpes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbPlatformOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            //ldapElem.FindChild<TextBox>("TxtVendorCriteria").Background = new SolidColorBrush(Colors.Green);
                                            ldapElem.FindChild<Button>("btnplus_platform").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnminus_platform").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnplus_platform").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnminus_platform").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            //ldapElem.Model.LabelOrVisible = false;
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlPlatformCri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbPlatformChecked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointPlatform + "</FieldName>");
                                xmlDataBuilder.Append("<Operator>contains</Operator>");
                                xmlDataBuilder.Append("<Value1>" + cvm.CbPlatformOpeSelected + "</Value1>");
                                xmlDataBuilder.Append("<Value2></Value2>");
                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for computer type
                        var comTypeOpes = labelCriElement.PnlComputerTypeCri.Children;
                        foreach (var co in comTypeOpes)
                        {
                            if (co.GetType() == typeof(ComputerTypeCriteriaElement))
                            {
                                var cvm = (co as ComputerTypeCriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbComputerTypeChecked == true)
                                {
                                    foreach (var child in labelCriElement.PnlComputerTypeCri.Children)
                                    {
                                        if (child.GetType() == typeof(ComputerTypeCriteriaElement))
                                        {
                                            var ldapElem = child as ComputerTypeCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbComputerTypeOpes");
                                            ldapElem.FindChild<ComboBox>("CbComputerTypeOpes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbComputerTypeOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnplus_comtype").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnminus_comtype").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnplus_comtype").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnminus_comtype").Foreground = (Brush)new BrushConverter().ConvertFrom("#fff");
                                        }
                                    }
                                    //_view.PnlComputerTypeCri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlComputerTypeCri.Children)
                                    {
                                        if (child.GetType() == typeof(ComputerTypeCriteriaElement))
                                        {
                                            var ldapElem = child as ComputerTypeCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbComputerTypeOpes");
                                            ldapElem.FindChild<ComboBox>("CbComputerTypeOpes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbComputerTypeOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnplus_comtype").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnminus_comtype").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnplus_comtype").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnminus_comtype").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            //ldapElem.Model.LabelOrVisible = false;
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlComputerTypeCri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbComputerTypeChecked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointComputerType +
                                                      "</FieldName>");
                                xmlDataBuilder.Append("<Operator>is</Operator>");
                                xmlDataBuilder.Append("<Value1>" +
                                                      (int)ConvertComTypeFromString(cvm.CbComputerTypeOpeSelected) +
                                                      "</Value1>");
                                xmlDataBuilder.Append("<Value2></Value2>");
                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for domain
                        var domainOpes = labelCriElement.PnlDomainCri.Children;
                        foreach (var co in domainOpes)
                        {
                            if (co.GetType() == typeof(DomainCriteriaElement))
                            {
                                var cvm = (co as DomainCriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbDomainChecked == true)
                                {
                                    foreach (var child in labelCriElement.PnlDomainCri.Children)
                                    {
                                        if (child.GetType() == typeof(DomainCriteriaElement))
                                        {
                                            var ldapElem = child as DomainCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbDomainOpes");
                                            ldapElem.FindChild<ComboBox>("CbDomainOpes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbDomainOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtDomainCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtDomainCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<ComboBox>("CbDomainCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbDomainCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnolus_domain").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnminus_domain").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnolus_domain").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnminus_domain").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlDomainCri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlDomainCri.Children)
                                    {
                                        if (child.GetType() == typeof(DomainCriteriaElement))
                                        {
                                            var ldapElem = child as DomainCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbDomainOpes");
                                            ldapElem.FindChild<ComboBox>("CbDomainOpes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbDomainOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtDomainCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtDomainCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbDomainCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbDomainCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8F93");
                                            ldapElem.FindChild<Button>("btnolus_domain").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnminus_domain").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnolus_domain").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnminus_domain").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlDomainCri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbDomainChecked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointDomain + "</FieldName>");
                                xmlDataBuilder.Append("<Operator>" + cvm.CbDomainOpeSelected + "</Operator>");
                                xmlDataBuilder.Append("<Value1>" +
                                                      (cvm.CbDomainOpeSelected == ConstantHelper.IsOperator
                                                          ? cvm.CbDomainCriteriaSelected
                                                          : ReplaceTag(cvm.TxtDomainCriteria ?? "")) + "</Value1>");
                                xmlDataBuilder.Append("<Value2></Value2>");
                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for memory
                        var memoryOpes = labelCriElement.PnlMemoryCri.Children;
                        foreach (var co in memoryOpes)
                        {
                            if (co.GetType() == typeof(MemoryCriteriaElement))
                            {
                                var cvm = (co as MemoryCriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbMemoryChecked == true)
                                {
                                    foreach (var child in labelCriElement.PnlMemoryCri.Children)
                                    {
                                        if (child.GetType() == typeof(MemoryCriteriaElement))
                                        {
                                            var ldapElem = child as MemoryCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbMemoryOpes");
                                            ldapElem.FindChild<ComboBox>("CbMemoryOpes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbMemoryOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtMemoryCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtMemoryCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnplus_memory").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnminus_memory").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnplus_memory").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnminus_memory").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlMemoryCri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlMemoryCri.Children)
                                    {
                                        if (child.GetType() == typeof(MemoryCriteriaElement))
                                        {
                                            var ldapElem = child as MemoryCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbMemoryOpes");
                                            ldapElem.FindChild<ComboBox>("CbMemoryOpes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbMemoryOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtMemoryCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<TextBox>("TxtMemoryCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnplus_memory").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnminus_memory").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnplus_memory").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnminus_memory").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            //ldapElem.Model.LabelOrVisible = false;
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlMemoryCri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbMemoryChecked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointMemory + "</FieldName>");
                                xmlDataBuilder.Append("<Operator>" + cvm.CbMemoryOpeSelected + "</Operator>");
                                xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtMemoryCriteria) + "</Value1>");
                                xmlDataBuilder.Append("<Value2></Value2>");
                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for Harddisk
                        var harddiskOpes = labelCriElement.PnlHarddiskCri.Children;
                        foreach (var co in harddiskOpes)
                        {
                            if (co.GetType() == typeof(HarddiskCriteriaElement))
                            {
                                var cvm = (co as HarddiskCriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbHarddiskChecked == true)
                                {
                                    foreach (var child in labelCriElement.PnlHarddiskCri.Children)
                                    {
                                        if (child.GetType() == typeof(HarddiskCriteriaElement))
                                        {
                                            var ldapElem = child as HarddiskCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbHarddiskOpes");
                                            ldapElem.FindChild<ComboBox>("CbHarddiskOpes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbHarddiskOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtHarddiskCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtHarddiskCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnplus_harddisk").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnminus_harddisk").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnplus_harddisk").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnminus_harddisk").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlHarddiskCri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlHarddiskCri.Children)
                                    {
                                        if (child.GetType() == typeof(HarddiskCriteriaElement))
                                        {
                                            var ldapElem = child as HarddiskCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbHarddiskOpes");
                                            ldapElem.FindChild<ComboBox>("CbHarddiskOpes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbHarddiskOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtHarddiskCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<TextBox>("TxtHarddiskCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnplus_harddisk").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnplus_harddisk").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnminus_harddisk").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnplus_harddisk").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnminus_harddisk").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlVendorCri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbHarddiskChecked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointHarddisk + "</FieldName>");
                                xmlDataBuilder.Append("<Operator>" + cvm.CbHarddiskOpeSelected + "</Operator>");
                                xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtHarddiskCriteria) + "</Value1>");
                                xmlDataBuilder.Append("<Value2></Value2>");
                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for IPv4
                        var ipv4Opes = labelCriElement.PnlIPv4Cri.Children;
                        foreach (var co in ipv4Opes)
                        {
                            if (co.GetType() == typeof(IPv4CriteriaElement))
                            {
                                var cvm = (co as IPv4CriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbIPv4Checked == true)
                                {
                                    foreach (var child in labelCriElement.PnlIPv4Cri.Children)
                                    {
                                        if (child.GetType() == typeof(IPv4CriteriaElement))
                                        {
                                            var ldapElem = child as IPv4CriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbIPv4Opes");
                                            ldapElem.FindChild<ComboBox>("CbIPv4Opes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbIPv4Opes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtIPv4Criteria1").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtIPv4Criteria1").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtIPv4Criteria2").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtIPv4Criteria2").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnplus_ipv4").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnminus_ipv4").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnplus_ipv4").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnminus_ipv4").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlIPv4Cri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlIPv4Cri.Children)
                                    {
                                        if (child.GetType() == typeof(IPv4CriteriaElement))
                                        {
                                            var ldapElem = child as IPv4CriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbIPv4Opes");
                                            ldapElem.FindChild<ComboBox>("CbIPv4Opes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbIPv4Opes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtIPv4Criteria1").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<TextBox>("TxtIPv4Criteria1").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtIPv4Criteria2").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<TextBox>("TxtIPv4Criteria2").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnplus_ipv4").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnminus_ipv4").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnplus_ipv4").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnminus_ipv4").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            //ldapElem.Model.LabelOrVisible = false;
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlIPv4Cri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbIPv4Checked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointIPv4 + "</FieldName>");
                                xmlDataBuilder.Append("<Operator>" + cvm.CbIPv4OpeSelected + "</Operator>");
                                xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtIPv4Criteria1) + "</Value1>");
                                xmlDataBuilder.Append("<Value2>" + ReplaceTag(cvm.TxtIPv4Criteria2) + "</Value2>");

                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for IPv6
                        var ipv6Opes = labelCriElement.PnlIPv6Cri.Children;
                        foreach (var co in ipv6Opes)
                        {
                            if (co.GetType() == typeof(IPv6CriteriaElement))
                            {
                                var cvm = (co as IPv6CriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbIPv6Checked == true)
                                {
                                    foreach (var child in labelCriElement.PnlIPv6Cri.Children)
                                    {
                                        if (child.GetType() == typeof(IPv6CriteriaElement))
                                        {
                                            var ldapElem = child as IPv6CriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbIPv6Opes");
                                            ldapElem.FindChild<ComboBox>("CbIPv6Opes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbIPv6Opes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtIPv6Criteria1").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtIPv6Criteria1").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtIPv6Criteria2").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtIPv6Criteria2").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnplus_ipv6").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnminus_ipv6").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnplus_ipv6").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnminus_ipv6").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlIPv6Cri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlIPv6Cri.Children)
                                    {
                                        if (child.GetType() == typeof(IPv6CriteriaElement))
                                        {
                                            var ldapElem = child as IPv6CriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbIPv6Opes");
                                            ldapElem.FindChild<ComboBox>("CbIPv6Opes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbIPv6Opes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtIPv6Criteria1").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<TextBox>("TxtIPv6Criteria1").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtIPv6Criteria2").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<TextBox>("TxtIPv6Criteria2").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnplus_ipv6").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnminus_ipv6").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnplus_ipv6").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnminus_ipv6").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            //ldapElem.Model.LabelOrVisible = false;
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlIPv6Cri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbIPv6Checked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointIPv6 + "</FieldName>");
                                xmlDataBuilder.Append("<Operator>" + cvm.CbIPv6OpeSelected + "</Operator>");
                                xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtIPv6Criteria1) + "</Value1>");
                                xmlDataBuilder.Append("<Value2>" + ReplaceTag(cvm.TxtIPv6Criteria2) + "</Value2>");

                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for Last sync
                        var lastsyncOpes = labelCriElement.PnlLastSyncCri.Children;
                        foreach (var co in lastsyncOpes)
                        {
                            if (co.GetType() == typeof(LastSyncCriteriaElement))
                            {
                                var cvm = (co as LastSyncCriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbLastSyncChecked == true)
                                {
                                    foreach (var child in labelCriElement.PnlLastSyncCri.Children)
                                    {
                                        if (child.GetType() == typeof(LastSyncCriteriaElement))
                                        {
                                            var ldapElem = child as LastSyncCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbLastSyncOpes");
                                            ldapElem.FindChild<ComboBox>("CbLastSyncOpes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbLastSyncOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<TextBox>("TxtLastSyncCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<TextBox>("TxtLastSyncCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnplus_lastsync").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnminus_lastsync").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnplus_lastsync").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnminus_lastsync").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlLastSyncCri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlLastSyncCri.Children)
                                    {
                                        if (child.GetType() == typeof(LastSyncCriteriaElement))
                                        {
                                            var ldapElem = child as LastSyncCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbLastSyncOpes");
                                            ldapElem.FindChild<ComboBox>("CbLastSyncOpes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbLastSyncOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtLastSyncCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<TextBox>("TxtLastSyncCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnplus_lastsync").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnminus_lastsync").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnplus_lastsync").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnminus_lastsync").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            //ldapElem.Model.LabelOrVisible = false;
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlLastSyncCri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbLastSyncChecked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointLastSynchronization +
                                                      "</FieldName>");
                                xmlDataBuilder.Append("<Operator>" + cvm.CbLastSyncOpeSelected + "</Operator>");
                                xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtLastSyncCriteria) + "</Value1>");
                                xmlDataBuilder.Append("<Value2></Value2>");
                                xmlDataBuilder.Append("</Label>");
                            }
                        }

                        //data for Color code
                        var clOpes = labelCriElement.PnlColorCodeCri.Children;
                        foreach (var co in clOpes)
                        {
                            if (co.GetType() == typeof(ColorCodeCriteriaElement))
                            {
                                var cvm = (co as ColorCodeCriteriaElement).Model;
                                if (labelCriteriaViewModel.ChbColorCodeChecked == true)
                                {
                                    foreach (var child in labelCriElement.PnlColorCodeCri.Children)
                                    {
                                        if (child.GetType() == typeof(ColorCodeCriteriaElement))
                                        {
                                            var ldapElem = child as ColorCodeCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbColorCodeOpes");
                                            ldapElem.FindChild<ComboBox>("CbColorCodeOpes").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<ComboBox>("CbColorCodeOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnplus_colorcode").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnminus_colorcode").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                            ldapElem.FindChild<Button>("btnplus_colorcode").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                            ldapElem.FindChild<Button>("btnminus_colorcode").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                        }
                                    }
                                    //_view.PnlColorCodeCri.Opacity = 0.5;

                                }
                                else
                                {
                                    foreach (var child in labelCriElement.PnlColorCodeCri.Children)
                                    {
                                        if (child.GetType() == typeof(ColorCodeCriteriaElement))
                                        {
                                            var ldapElem = child as ColorCodeCriteriaElement;
                                            ldapElem.FindChild<ComboBox>("CbColorCodeOpes");
                                            ldapElem.FindChild<ComboBox>("CbColorCodeOpes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<ComboBox>("CbColorCodeOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnplus_colorcode").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnminus_colorcode").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                            ldapElem.FindChild<Button>("btnplus_colorcode").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                            ldapElem.FindChild<Button>("btnminus_colorcode").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                        }
                                    }
                                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                                    //_view.PnlColorCodeCri.Opacity = 0.5;
                                }
                                xmlDataBuilder.Append("<Label LabelId=\"" + labelCriteriaViewModel.LabelId + "\">");
                                xmlDataBuilder.Append("<IsAvailable>" + labelCriteriaViewModel.ChbColorCodeChecked +
                                                      "</IsAvailable>");
                                xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                                      "</CriteriaId>");
                                xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointColorCode + "</FieldName>");
                                xmlDataBuilder.Append("<Operator>is</Operator>");
                                xmlDataBuilder.Append("<Value1>" + cvm.CbColorCodeOpeSelected + "</Value1>");
                                xmlDataBuilder.Append("<Value2></Value2>");
                                xmlDataBuilder.Append("</Label>");
                            }
                        }
                    }
                }
            }

            xmlDataBuilder.Append("</DataSet>");
            return xmlDataBuilder.ToString();
        }

        private string BuildXmlDataAdd()
        {
            var xmlDataBuilder = new StringBuilder();
            xmlDataBuilder.Append("<DataSet>");

            //Build the xml data
            //data for computer name
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointComputerName + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</Label>");

            //data for vendor
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointVendor + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</Label>");

            //data for model
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointModel + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</Label>");

            //data for operating system
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointOperatingSystem + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</Label>");

            //data for Platform
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointPlatform + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1>86</Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</Label>");

            //data for computer type
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointComputerType + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1>0</Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.Int + "</Type>");
            xmlDataBuilder.Append("</Label>");
            //data for domain
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointDomain + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</Label>");

            //data for memory
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointMemory + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.GreaterThanOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.Real + "</Type>");
            xmlDataBuilder.Append("</Label>");

            //data for Harddisk
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointHarddisk + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.GreaterThanOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.Real + "</Type>");
            xmlDataBuilder.Append("</Label>");

            //data for IPv4
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointIPv4 + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsBetweenOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</Label>");

            //data for IPv6
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointIPv6 + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsBetweenOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</Label>");

            //data for Last sync
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointLastSynchronization +
                                  "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsWithinOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.Int + "</Type>");
            xmlDataBuilder.Append("</Label>");

            //data for Color code
            xmlDataBuilder.Append("<Label>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointColorCode + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1>Green</Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int) CriteriaType.Extend + "</Type>");
            xmlDataBuilder.Append("</Label>");


            xmlDataBuilder.Append("</DataSet>");
            return xmlDataBuilder.ToString();
        }

        private int FindLabelIndex()
        {
            var numberCounts = new List<int>();
            var labelControls = _view.PnlLabelContainer.Children;
            foreach (var lbel in labelControls)
            {
                if (lbel.GetType() == typeof(LabelCriteriaElement))
                {
                    var lcri = lbel as LabelCriteriaElement;
                    var header = lcri.Expander.Header.ToString();
                    if (header.IndexOf("New Label", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var headerUp = header.ToUpper();
                        string[] separatingChars = { "NEW LABEL" };
                        var nameSplit = headerUp.Split(separatingChars, StringSplitOptions.RemoveEmptyEntries);
                        if (nameSplit.Length >= 1)
                        {
                            var numberCountLocal = 0;
                            try
                            {
                                numberCountLocal =
                                    int.Parse(nameSplit[0].Replace("(", "").Replace(")", "").Trim());
                            }
                            catch
                            {
                                numberCountLocal = 0;
                            }
                            if (numberCountLocal > 1)
                            {
                                numberCounts.Add(numberCountLocal);
                            }
                        }
                        else
                        {
                            numberCounts.Add(1);
                        }
                    }
                }
            }
            var maxCount = 0;
            if (numberCounts.Count > 0)
            {
                numberCounts.Sort();
                foreach (var n in numberCounts)
                {
                    if (!numberCounts.Contains(n + 1))
                    {
                        maxCount = n + 1;
                        break;
                    }
                }
            }
            return maxCount;
        }

        private void OnAddLabel(object pars)
        {
            try
            {
                Logger.Info("Starting add a label");
                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var labelName = "New Label";
                    var maxCount = FindLabelIndex();
                    if (maxCount >= 2)
                    {
                        labelName += " (" + maxCount + ")";
                    }
                    var initdata = BuildXmlDataAdd();
                    var request = new StringAuthenticateObject {StringValue = labelName, StringAuth = initdata};
                    var req = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(request),
                        KeyEncryption);
                    var result = sc.AddLabel(req);
                    var resultDeserialize =
                        JsonConvert.DeserializeObject<LabelCriteriaEndpointList>(EncryptionHelper.DecryptRijndael(
                            result,
                            KeyEncryption));
                    if (resultDeserialize == null)
                    {
                        var messageDialog =
                            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                        messageDialog.ShowMessageDialog("Data is null", "Add Label");
                        return;
                    }
                    var loadLabelBk = new BackgroundWorker();
                    loadLabelBk.DoWork += LoadLabelBk_DoWork;
                    loadLabelBk.RunWorkerAsync();
                    var newLabel = new LabelCriteriaElement();
                    var labelViewModel = newLabel.DataContext as LabelCriteriaViewModel;
                    if (labelViewModel != null)
                    {
                        labelViewModel.LabelId = resultDeserialize.Labels[0].LabelId;
                        labelViewModel.LabelName = resultDeserialize.Labels[0].LabelName;
                        foreach (var lbcri in resultDeserialize.LabelCriterias)
                        {
                            SetLabelCriteriaViewModel(newLabel, lbcri);
                        }
                        labelViewModel.IsAddState = true;
                    }
                    _view.PnlLabelContainer.Children.Insert(resultDeserialize.Labels[0].LabelIndex-1,newLabel);
                    
                }
                Logger.Info("Ended add a label");
                
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                var messageDialog =
                    PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                messageDialog.ShowMessageDialog(
                    "Cannot add a label due to exception occured, please see the log file under the Logs for more information",
                    "Add Label");
            }
        }
        
        private void LoadLabelBk_DoWork(object sender, DoWorkEventArgs e)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
            {
                //Reload LabelFilterViewModel
                ApplicationContext.IsRebuildTree = true;
                RightTreeViewModel.LoadLabelView(true);
            }));
                
        }

        private void OnResetLabel()
        {
            try
            {
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
                {
                    var labelCris = _view.PnlLabelContainer.Children;
                    var lbids = new List<int>();
                    foreach (var ex in labelCris)
                    {
                        if (ex.GetType() == typeof (LabelCriteriaElement))
                        {
                            var expander = (ex as LabelCriteriaElement).Expander;
                            var chbheader = expander.FindChild<CheckBox>("CbExpandHeader");
                            if (chbheader != null && chbheader.IsChecked == true)
                            {
                                lbids.Add(((ex as LabelCriteriaElement).DataContext as LabelCriteriaViewModel).LabelId);
                            }
                        }
                    }

                    if (lbids.Count == 0)
                    {
                        var messageDialog =
                            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                        messageDialog.ShowMessageDialog("Please select at least a label to reset", "Reset Label");
                    }
                    else
                    {
                        Logger.Info("Starting reset label");
                        using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                        {
                            var request = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(lbids),
                                KeyEncryption);
                            var result = sc.ResetLabel(request);
                            var resultDeserialize =
                                JsonConvert.DeserializeObject<LabelCriteriaEndpointList>(
                                    EncryptionHelper.DecryptRijndael(result,
                                        KeyEncryption));
                            if (resultDeserialize == null)
                            {
                                var messageDialog =
                                    PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                                messageDialog.ShowMessageDialog("Data is null", "Get label data");
                            }

                            BuildLabelPage(resultDeserialize, true);
                            //Reload LabelFilterViewModel
                            ApplicationContext.IsRebuildTree = true;
                            RightTreeViewModel.LoadLabelView(true);
                        }
                        Logger.Info("Ended reset label");
                    }
                }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action) (() =>
                {
                    var messageDialog =
                        PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.ShowMessageDialog(
                        "Cannot reset label due to exception occured, please see the log file under the Logs for more information",
                        "Reset label");
                }));
            }
        }

        private void ResetCommandAction(object pars)
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
                mainViewModel.IsBusy = true;

            var backgroundWorker = new BackgroundWorker();
            backgroundWorker.DoWork += ReSetBackgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += ResetBackgroundWorker_RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        private void ResetBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
                mainViewModel.IsBusy = false;
            //Releases all resources used by BackgroundWorker
            var worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= ResetBackgroundWorker_RunWorkerCompleted;
            worker.DoWork -= ReSetBackgroundWorker_DoWork;
            worker.Dispose();
        }
        private void ReSetBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            OnResetLabel();
        }
        private void SaveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                    
                try
                {
                    Logger.Info("Starting save all label criteria datas");
                    using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                    {
                        var dataXml = BuildXmlDataEdit();
                        var request = new StringAuthenticateObject { StringValue = dataXml, StringAuth = "OK" };
                        var datareq = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(request),
                            KeyEncryption);
                        sc.EditLabelCriteria(datareq);
                    }

                    Logger.Info("Ended save label criteria datas");
                    var messageDialog =
                        PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                    //Reload LabelFilterViewModel
                    ApplicationContext.IsRebuildTree = true;
                    RightTreeViewModel.LoadLabelView(true);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);

                    var messageDialog =
                        PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;

                    messageDialog.ShowMessageDialog(
                        "Cannot save label criteria due to exception occured, please see the log file under the Logs for more information",
                        "Save label criteria");
                }
            }));
        }
        private void SaveBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            var worker = sender as BackgroundWorker;
            worker.RunWorkerCompleted -= SaveBackgroundWorker_RunWorkerCompleted;
            worker.DoWork -= SaveBackgroundWorker_DoWork;
            worker.Dispose();
        }

       

        public void OnSaveLabel(object pars)
        {
                var backgroundWorker = new BackgroundWorker();
                backgroundWorker.DoWork += SaveBackgroundWorker_DoWork;
                backgroundWorker.RunWorkerCompleted += SaveBackgroundWorker_RunWorkerCompleted;
                backgroundWorker.RunWorkerAsync();
        }
        //Make it public to access(09-04-2019)
        public void OnDeleteLabel(int id)
        {
            try
            {
                var labelCris = _view.PnlLabelContainer.Children;
                var lbids = new List<int>();
                lbids.Add(id);
                //foreach (var ex in labelCris)
                //{
                //    if (ex.GetType() == typeof (LabelCriteriaElement))
                //    {
                //        var expander = (ex as LabelCriteriaElement).Expander;
                //        var chbheader = expander.FindChild<CheckBox>("CbExpandHeader");
                //        if (chbheader != null && chbheader.IsChecked == true)
                //        {
                //            lbids.Add(((ex as LabelCriteriaElement).DataContext as LabelCriteriaViewModel).LabelId);
                //        }
                //    }
                //}

                if (lbids.Count == 0)
                {
                    var messageDialog =
                        PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.ShowMessageDialog("please select at least a label to delete", "Delete Label");
                }
                else
                {
                    var confirmdialog = new ConfirmDialog("Are you sure you want to delete selected labels?","CONFIRM DELETE");
                    confirmdialog.ConfirmText.Text = "Are you sure you want to delete selected labels?";
                    confirmdialog.BtnOk.Focus();
                    if (confirmdialog.ShowDialog() == true)
                    {
                        Logger.Info("Starting delete label");
                        using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                        {
                            var request = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(lbids),
                                KeyEncryption);
                            var result = sc.DeleteLabel(request);
                            var resultDeserialize =
                                JsonConvert.DeserializeObject<LabelCriteriaEndpointList>(
                                    EncryptionHelper.DecryptRijndael(result,
                                        KeyEncryption));
                            if (resultDeserialize == null)
                            {
                                var messageDialog =
                                    PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                                messageDialog.ShowMessageDialog("Data is null", "Get label data");
                            }

                            BuildLabelPage(resultDeserialize);
                            //Reload LabelFilterViewModel
                            foreach (var lid in lbids)
                            {
                                if (ApplicationContext.LabelNodesSelected.Select(l => l.NodeId).Contains(lid))
                                {
                                    ApplicationContext.LabelNodesSelected.Remove(
                                        ApplicationContext.LabelNodesSelected.Find(r => r.NodeId == lid));
                                }
                            }
                            ApplicationContext.IsRebuildTree = true;
                            RightTreeViewModel.LoadLabelView(true);
                        }
                        Logger.Info("Ended delete label");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                var messageDialog =
                    PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                messageDialog.ShowMessageDialog(
                    "Cannot delete label due to exception occured, please see the log file under the Logs for more information",
                    "Delete label");
            }
        }

        private bool CanExecuteCommand(object pars)
        {
            return !IsBusy;
        }
        private bool CanDeleteRule(int pars)
        {
            return true;
        }
        /// <summary>
        ///     Get all of expander id that expanded
        /// </summary>
        /// <returns></returns>
        private List<int> GetListLabelExpanded()
        {
            var labelCriterias = _view.PnlLabelContainer.Children;
            var lbids = new List<int>();
            foreach (var ex in labelCriterias)
            {
                if (ex.GetType() == typeof (LabelCriteriaElement))
                {
                    var expander = (ex as LabelCriteriaElement).Expander;
                    if (expander != null && expander.IsExpanded)
                    {
                        lbids.Add(((ex as LabelCriteriaElement).DataContext as LabelCriteriaViewModel).LabelId);
                    }
                }
            }
            return lbids;
        }

        private string ReplaceTag(string source)
        {
            if (!string.IsNullOrWhiteSpace(source))
                return source.Replace("<", "#open;").Replace(">", "#close;").Replace("&", "#and;");
            return source;
        }

        #endregion
    }
}