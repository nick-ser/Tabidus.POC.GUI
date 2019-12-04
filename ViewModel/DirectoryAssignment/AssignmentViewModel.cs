using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Threading;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.DirectoryAssignment;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.LDAP;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.UserControls.DirectoryAssignment;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class AssignmentViewModel : ViewModelBase
    {
        #region Constructor

        public AssignmentViewModel(DirectoryAssignmentPage page)
        {
            _bgWorkerHelper = new BackgroundWorkerHelper();
            _page = page;
            AddRuleCommand = new RelayCommand(ExecuteAddRule, CanAddRule);
            DeleteRuleCommand = new RelayCommand<int>(ExecuteDeleteRule, CanDeleteRule);
            AutoSaveCommand = new RelayCommand(ExecuteAutoSave, CanAutoSave);
            var ldaps = GetLDAPs();
            ApplicationContext.CbLDAPItems = new List<ComboboxItem>();
            if (ldaps != null)
            {
                foreach (var ldap in ldaps)
                {
                    var cbi = new ComboboxItem
                    {
                        Text = ldap.Domain,
                        Value = ldap.Id
                    };
                    ApplicationContext.CbLDAPItems.Add(cbi);
                }
            }
            if (ApplicationContext.CbComputerOpeItems == null ||
                (ApplicationContext.CbComputerOpeItems != null && ApplicationContext.CbComputerOpeItems.Count <= 0))
            {
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
        }

        #endregion

        #region Private variable

        private readonly DirectoryAssignmentPage _page;
        private readonly BackgroundWorkerHelper _bgWorkerHelper;

        private readonly MessageDialog _messageDialog =
            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;

        #endregion

        #region BackgroundWorker

        #endregion

        #region Commands

        public ICommand AddRuleCommand { get; private set; }
        public ICommand DeleteRuleCommand { get; private set; }
        public ICommand AutoSaveCommand { get; private set; }
        //AddRuleCommand
        private void ExecuteAddRule(object obj)
        {
            try
            {
                var labelName = "New Rule";
                var maxCount = FindRuleIndex();
                if (maxCount >= 2)
                {
                    labelName += " (" + maxCount + ")";
                }
                var initdata = BuildXmlDataAdd();
                var ldapXmlInit = BuildXmlLDAPDataAdd();
                var request = new AddRuleDataRequest(labelName, ApplicationContext.NodesSelected[0].NodeId, initdata);
                request.LdapXmlData = ldapXmlInit;
                request.IsActive = false;
                var resultDeserialize =
                    ServiceManager.Invoke(
                        sc => RequestResponseUtils.GetData<GetRulesDataResponse>(sc.AddRule, request));
                if (resultDeserialize == null)
                {
                    _messageDialog.ShowMessageDialog("Data is null", "Add Rule");
                    return;
                }

                var newRule = new AssignmentCriterialElement();
                var ruleViewModel = newRule.Model;
                if (ruleViewModel != null)
                {
                    ruleViewModel.Id = resultDeserialize.Result.AssignmentRules[0].Id;
                    ruleViewModel.Name = resultDeserialize.Result.AssignmentRules[0].Name;
                    foreach (var lbcri in resultDeserialize.Result.AssignmentRuleCriterias)
                    {
                        SetRuleCriteriaViewModel(newRule, lbcri);
                    }
                    var ldapAssignment = new LDAPAssignment();
                    ldapAssignment.Model.Id = resultDeserialize.Result.LdapRuleCriterias[0].Id;
                    ldapAssignment.CbLDAP.Tag = true;
                    newRule.PnlLDAPCri.Children.Add(ldapAssignment);
                    ruleViewModel.IsAddState = true;
                    ruleViewModel.FolderId = ApplicationContext.NodesSelected[0].NodeId;
                }
                _page.PnlDirAssignmentContainer.Children.Insert(resultDeserialize.Result.AssignmentRules[0].Index - 1,
                    newRule);
                ApplicationContext.AssignmentRulesData.Result.AssignmentRules.Add(resultDeserialize.Result.AssignmentRules[0]);
                ApplicationContext.AssignmentRulesData.Result.AssignmentRuleCriterias.AddRange(resultDeserialize.Result.AssignmentRuleCriterias);
                ApplicationContext.AssignmentRulesData.Result.LdapRuleCriterias.Add(resultDeserialize.Result.LdapRuleCriterias[0]);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                //_messageDialog.ShowMessageDialog(
                //    "Cannot add a directory assignment rule due to exception occured, please see the log file under the Logs for more information",
                //    "Add directory assignment rule");
            }
        }


        private List<LDAP> GetLDAPs()
        {
            var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK"
            };
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<LDAP>>(
                sc.GetAllLDAP,
                requestObj));

            if (resultDeserialize == null)
            {
                _messageDialog.ShowMessageDialog("Data is null", "Message");

                return new List<LDAP>();
            }

            return resultDeserialize;
        }

        /// <summary>
        ///     Build label page
        /// </summary>
        public void BuildPage()
        {
            ReloadPage();
        }

        public void GetRuleByFolder()
        {
            var bgWorkerHelper = new BackgroundWorkerHelper();
            bgWorkerHelper.AddDoWork(RebuildPage)
                .DoWork();
        }

        private void ReloadPage()
        {
            try
            {
                if (ApplicationContext.AssignmentRulesData == null)
                {
                    var requestObj = new GetRuleDataRequest { DirectoryId = ApplicationContext.NodesSelected[0].NodeId };
                    var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<GetRulesDataResponse>(
                        sc.GetRules,
                        requestObj));

                    if (resultDeserialize == null)
                    {
                        _page.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            (Action)(() => { _messageDialog.ShowMessageDialog("Data is null", "Message"); }));
                    }
                    ApplicationContext.AssignmentRulesData = resultDeserialize;
                    BuildPage(resultDeserialize);
                }
                else
                {
                    BuildPage(ApplicationContext.AssignmentRulesData,true);
                    GetRuleByFolder();
                }
               
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                _page.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)
                        (() =>
                        {
                            _messageDialog.ShowMessageDialog(
                                "Cannot get assigment rules due to exception occured, please see the log file under the Logs for more information",
                                "Message");
                        }));
            }
        }

        public void UpdateHeader()
        {
            var headerViewModel = _page.HeaderViewModel;
            if (headerViewModel != null && ApplicationContext.NodesSelected.Count > 0)
            {
                headerViewModel.UpdateDirectoryHeader(ApplicationContext.NodesSelected[0].NodeId);
            }
        }

        private void RebuildPage(object sender, DoWorkEventArgs args)
        {
            try
            {
                _page.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)(() =>
                   {
                       UpdateHeader();

                       var requestObj = new GetRuleDataRequest
                       {
                           DirectoryId = ApplicationContext.NodesSelected[0].NodeId
                       };
                       var resultDeserialize =
                           ServiceManager.Invoke(sc => RequestResponseUtils.GetData<GetRulesDataResponse>(
                               sc.GetActivedRuleByDirectoryId,
                               requestObj));

                       if (resultDeserialize == null)
                       {
                           _messageDialog.ShowMessageDialog("Data is null", "Message");
                       }

                       SetCriteriaViewModelValue(resultDeserialize);
                   }));
            }
            catch (Exception ex)
            {
                Logger.Error(ex);

                _page.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (Action)
                        (() =>
                        {
                            _messageDialog.ShowMessageDialog(
                                "Cannot get assigment rules due to exception occured, please see the log file under the Logs for more information",
                                "Message");
                        }));
            }
        }

        /// <summary>
        ///     Build label page again or reset the label selected
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="isReset"></param>
        private void BuildPage(GetRulesDataResponse datas, bool isReset = false)
        {
            _page.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
           {
               var lbids = GetListLabelExpanded();
               var assignmentRules = datas.Result.AssignmentRules;
               var assignmentRuleCriterias = datas.Result.AssignmentRuleCriterias;
               var ldapRules = datas.Result.LdapRuleCriterias;
                //get all datas to rebuild page
                
                    #region Get all datas to rebuild page

                    _page.PnlDirAssignmentContainer.Children.Clear();
                   foreach (var ld in assignmentRules)
                   {
                       var labelCriteriaElement = new AssignmentCriterialElement();
                       var labelCriteriaViewModel = labelCriteriaElement.Model;
                       labelCriteriaViewModel.Id = ld.Id;
                       labelCriteriaViewModel.FolderId = ApplicationContext.NodesSelected[0].NodeId;
                       labelCriteriaViewModel.Name = ld.Name;
                       labelCriteriaViewModel.IsEnable = !isReset && (ld.IsActive ?? false);
                       labelCriteriaElement.Expander.Header = ld.Name;

                       if (lbids.Contains(ld.Id))
                       {
                           labelCriteriaElement.Expander.IsExpanded = true;
                       }
                       var lbcris =
                           assignmentRuleCriterias.Select(r => r)
                               .Where(r => r.AssignmentRuleId == ld.Id)
                               .ToList();
                       var ldapcris = ldapRules.Select(r => r).Where(r => r.AssignmentRuleId == ld.Id).ToList();
                       foreach (var lbcri in lbcris)
                       {
                           SetRuleCriteriaViewModel(labelCriteriaElement, lbcri);
                       }
                       char[] spliter = { ';' };

                       foreach (var ldapcri in ldapcris)
                       {
                           var ldapAssigment = new LDAPAssignment();
                           ldapAssigment.CbLDAP.Tag = true;
                           var ldapAssignmentViewModel = ldapAssigment.Model;
                           ldapAssignmentViewModel.CbLDAPSelected = ldapcri.DomainId;
                           ldapAssignmentViewModel.ChbLDAPChecked = ldapcri.IsActive ?? false;
                           ldapAssignmentViewModel.ChbLDAPFolderChecked = ldapcri.IsFolderActive ?? false;
                           ldapAssignmentViewModel.ChbAddOnlyComputerChecked = ldapcri.IsAddOnlyComputer ?? false;
                           ldapAssignmentViewModel.ChbExcludeComputerChecked = ldapcri.IsExcludeComputerActive ?? false;
                           ldapAssignmentViewModel.ChbExcludeFolderChecked = ldapcri.IsExcludeSubFolderActive ?? false;
                           ldapAssignmentViewModel.ChbExcludeEmptyFolderChecked = ldapcri.IsExcludeEmptyFolder ?? false;
                           ldapAssignmentViewModel.TxtExcludeFolder = ldapcri.ExcludeSubFolderPath;
                           ldapAssignmentViewModel.TxtExcludeComputer = ldapcri.ExcludeComputerPath;
                           ldapAssignmentViewModel.TxtLDAPFolder = ldapcri.FolderPath;
                           var excludeSubFolders =
                               ldapcri.ExcludeSubFolder != null
                                   ? ldapcri.ExcludeSubFolder.Split(spliter, StringSplitOptions.RemoveEmptyEntries)
                                       .ToList()
                                   : new List<string>();
                           var excludeComputers =
                               ldapcri.ExcludeComputer != null
                                   ? ldapcri.ExcludeComputer.Split(spliter, StringSplitOptions.RemoveEmptyEntries)
                                       .ToList()
                                   : new List<string>();
                           ldapAssignmentViewModel.ExcludeFolderIds = excludeSubFolders;
                           ldapAssignmentViewModel.ExcludeComputerIds = excludeComputers;
                           ldapAssignmentViewModel.Id = ldapcri.Id;
                           labelCriteriaElement.PnlLDAPCri.Children.Add(ldapAssigment);
                           ldapAssignmentViewModel.LDAPFolderId = ldapcri.FolderId;
                       }
                       _page.PnlDirAssignmentContainer.Children.Add(labelCriteriaElement);
                   }

                    #endregion
                
           }));
        }

        private void SetCriteriaViewModelValue(GetRulesDataResponse datas)
        {
            var assignmentRules = datas.Result.AssignmentRules;
            var labelCriterias = _page.PnlDirAssignmentContainer.Children;

            foreach (var ex in labelCriterias)
            {
                if (ex.GetType() == typeof(AssignmentCriterialElement))
                {
                    var labelCriElem = ex as AssignmentCriterialElement;
                    var labelCriViewMoel = labelCriElem.Model;
                    labelCriViewMoel.IsEnable = false;
                    foreach (var ar in assignmentRules)
                    {
                        if (ar.Id == labelCriViewMoel.Id)
                        {
                            labelCriViewMoel.IsEnable = true;
                            break;
                        }
                    }
                }
            }
        }

        private void GetPathNode(List<string> listNode, LDAPDirectory dir, List<LDAPDirectory> ldapDirectories)
        {
            if (dir == null) return;
            listNode.Add(dir.FolderName);
            foreach (var ep in ldapDirectories)
            {
                if (dir.ParentId == ep.Id)
                {
                    GetPathNode(listNode, ep, ldapDirectories);
                    break;
                }
            }
        }


        /// <summary>
        ///     Get all of expander id that expanded
        /// </summary>
        /// <returns></returns>
        private List<int> GetListLabelExpanded()
        {
            var labelCriterias = _page.PnlDirAssignmentContainer.Children;
            var lbids = new List<int>();
            foreach (var ex in labelCriterias)
            {
                if (ex.GetType() == typeof(AssignmentCriterialElement))
                {
                    var expander = (ex as AssignmentCriterialElement).Expander;
                    if (expander != null && expander.IsExpanded)
                    {
                        lbids.Add((ex as AssignmentCriterialElement).Model.Id);
                    }
                }
            }
            return lbids;
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
                mainViewModel.IsBusy = false;
        }

        private bool CanAddRule(object pars)
        {
            return true;
        }

        //DeleteRuleCommand
        private void ExecuteDeleteRule(int id)
        {
            var children = _page.PnlDirAssignmentContainer.Children;

            for (var i = children.Count - 1; i >= 0; i--)
            {
                if (children[i].GetType() == typeof(AssignmentCriterialElement))
                {
                    var expander = children[i] as AssignmentCriterialElement;
                    if (expander.Model.Id == id)
                    {
                        var confirmdialog = new ConfirmDialog("Are you sure you want to delete selected rule?","CONFIRM DELETE");
                        confirmdialog.ConfirmText.Text = "Are you sure you want to delete selected rule?";
                        confirmdialog.BtnOk.Focus();
                        if (confirmdialog.ShowDialog() == true)
                        {
                            var request = new DeleteRuleDataRequest();
                            request.Ids = expander.Model.Id.ToString();
                            _bgWorkerHelper.AddDoWork(OnDeleteRuleDoWork).DoWork(request);

                            _page.PnlDirAssignmentContainer.Children.RemoveAt(i);
                            ApplicationContext.AssignmentRulesData.Result.AssignmentRules.RemoveAll(r => r.Id == id);
                        }

                        break;
                    }
                }
            }
        }

        private bool CanDeleteRule(int pars)
        {
            return true;
        }

        //AutoSaveCommand
        private void ExecuteAutoSave(object obj)
        {
        }

        private bool CanAutoSave(object pars)
        {
            return true;
        }

        #endregion

        #region Background Workers

        //DeleteRule Worker
        private void OnDeleteRuleDoWork(object sender, DoWorkEventArgs args)
        {
            var dataRequest = args.Argument as DeleteRuleDataRequest;
            ServiceManager.Invoke(
                sc => RequestResponseUtils.GetData<RemoveRuleDataResponse>(sc.DeleteRule, dataRequest));
        }

        private void OnDeleteRuleRunWorkCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
        }

        /// <summary>
        ///     Get items for combobox depend on input key
        /// </summary>
        /// <param name="configKey">mapping with appconfig key</param>
        /// <returns></returns>
        private List<string> CreateComboboxItems(string configKey)
        {
            var sitems = Functions.GetConfig(configKey, "");
            return sitems.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
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

        private int FindRuleIndex()
        {
            var numberCounts = new List<int>();
            var labelControls = _page.PnlDirAssignmentContainer.Children;
            foreach (var lbel in labelControls)
            {
                if (lbel.GetType() == typeof(AssignmentCriterialElement))
                {
                    var lcri = lbel as AssignmentCriterialElement;
                    var header = lcri.Expander.Header.ToString();
                    if (header.IndexOf("New Rule", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var headerUp = header.ToUpper();
                        string[] separatingChars = { "NEW RULE" };
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

        private string BuildXmlDataAdd()
        {
            var xmlDataBuilder = new StringBuilder();
            xmlDataBuilder.Append("<DataSet>");

            //Build the xml data
            //data for unmanaged endpoints
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.UnmanagedEndpoints + "</FieldName>");
            xmlDataBuilder.Append("<Operator></Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.Extend + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for managed endpoints
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.ManagedEndpoints + "</FieldName>");
            xmlDataBuilder.Append("<Operator></Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.Extend + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for computer name
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointComputerName + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for vendor
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointVendor + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for model
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointModel + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for operating system
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointOperatingSystem + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for Platform
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointPlatform + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1>86</Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for computer type
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointComputerType + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1>0</Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.Int + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");
            //data for domain
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointDomain + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for memory
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointMemory + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.GreaterThanOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.Real + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for Harddisk
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointHarddisk + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.GreaterThanOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.Real + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for IPv4
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointIPv4 + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsBetweenOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for IPv6
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointIPv6 + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsBetweenOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.String + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for Last sync
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointLastSynchronization +
                                  "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsWithinOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.Int + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");

            //data for Color code
            xmlDataBuilder.Append("<AssignmentRule>");
            xmlDataBuilder.Append("<IsAvailable>0</IsAvailable>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointColorCode + "</FieldName>");
            xmlDataBuilder.Append("<Operator>" + ConstantHelper.IsOperator + "</Operator>");
            xmlDataBuilder.Append("<Value1>Green</Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("<Type>" + (int)CriteriaType.Extend + "</Type>");
            xmlDataBuilder.Append("</AssignmentRule>");


            xmlDataBuilder.Append("</DataSet>");
            return xmlDataBuilder.ToString();
        }

        private string BuildXmlLDAPDataAdd()
        {
            var xmlDataBuilder = new StringBuilder();
            xmlDataBuilder.Append("<DataSet>");

            xmlDataBuilder.Append("<LDAPRuleCriteria>");
            xmlDataBuilder.Append("<DomainId>" +
                                  (ApplicationContext.CbLDAPItems != null && ApplicationContext.CbLDAPItems.Count > 0
                                      ? ApplicationContext.CbLDAPItems[0].Value
                                      : 0) +
                                  "</DomainId>");
            xmlDataBuilder.Append("<FolderId></FolderId>");
            xmlDataBuilder.Append("<ExcludeSubFolder></ExcludeSubFolder>");
            xmlDataBuilder.Append("<ExcludeComputer></ExcludeComputer>");
            xmlDataBuilder.Append("<IsExcludeEmptyFolder>0</IsExcludeEmptyFolder>");
            xmlDataBuilder.Append("<IsAddOnlyComputer>0</IsAddOnlyComputer>");
            xmlDataBuilder.Append("<IsFolderActive>0</IsFolderActive>");
            xmlDataBuilder.Append("<IsExcludeSubFolderActive>0</IsExcludeSubFolderActive>");
            xmlDataBuilder.Append("<IsExcludeComputerActive>0</IsExcludeComputerActive>");
            xmlDataBuilder.Append("<IsActive>0</IsActive>");
            xmlDataBuilder.Append("<FolderPath></FolderPath>");
            xmlDataBuilder.Append("<ExcludeSubFolderPath></ExcludeSubFolderPath>");
            xmlDataBuilder.Append("<ExcludeComputerPath></ExcludeComputerPath>");
            xmlDataBuilder.Append("</LDAPRuleCriteria>");

            xmlDataBuilder.Append("</DataSet>");
            return xmlDataBuilder.ToString();
        }

        /// <summary>
        ///     Set value for label criteria view model
        /// </summary>
        /// <param name="labelCriteriaElement">the label criteria view model that want to set values</param>
        /// <param name="lbcri">data to set to label criteria view model</param>
        private void SetRuleCriteriaViewModel(AssignmentCriterialElement labelCriteriaElement,
            AssignmentRuleCriteria lbcri)
        {
            var labelCriteriaViewModel = labelCriteriaElement.Model;
            switch (lbcri.FieldName)
            {
                case CommonConstants.UnmanagedEndpoints:
                    labelCriteriaViewModel.ChbUnmangedChecked = lbcri.IsAvailable;
                    labelCriteriaViewModel.UnmanagedCriteriaId = lbcri.AssignmentOperatorCriterias[0].CriteriaId;
                    break;
                case CommonConstants.ManagedEndpoints:
                    labelCriteriaViewModel.ChbMangedChecked = lbcri.IsAvailable;
                    labelCriteriaViewModel.ManagedCriteriaId = lbcri.AssignmentOperatorCriterias[0].CriteriaId;
                    break;
                case CommonConstants.EndpointComputerName:
                    var count = 0;
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count++;
                        var computerNameopeCri = new ComputerNameCriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count)
                        {
                            computerNameopeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count2++;
                        var vendorOpeCri = new VendorCriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count2)
                        {
                            vendorOpeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count3++;
                        var opeCri = new ModelCriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count3)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count4++;
                        var opeCri = new OSCriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count4)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count5++;
                        var opeCri = new PlatformCriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count5)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
                        {
                            opeCri.Model.BtnDelVisible = false;
                        }
                        if (count5 == 1)
                        {
                            opeCri.Model.LabelOrVisible = false;
                        }
                        opeCri.Model.CbPlatformOpeSelected = string.IsNullOrWhiteSpace(loc.Value1)
                            ? 0
                            : int.Parse(loc.Value1);
                        opeCri.Model.CriteriaId = loc.CriteriaId;
                        labelCriteriaElement.PnlPlatformCri.Children.Add(opeCri);
                    }
                    labelCriteriaViewModel.ChbPlatformChecked = lbcri.IsAvailable;

                    break;
                case CommonConstants.EndpointComputerType:
                    var count6 = 0;
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count6++;
                        var opeCri = new ComputerTypeCriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count6)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count7++;
                        var opeCri = new DomainCriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count7)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count8++;
                        var opeCri = new IPv4CriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count8)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count9++;
                        var opeCri = new IPv6CriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count9)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count10++;
                        var opeCri = new MemoryCriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count10)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count11++;
                        var opeCri = new HarddiskCriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count11)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count12++;
                        var opeCri = new LastSyncCriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count12)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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
                    foreach (var loc in lbcri.AssignmentOperatorCriterias)
                    {
                        count13++;
                        var opeCri = new ColorCodeCriteriaElement();
                        if (lbcri.AssignmentOperatorCriterias.Count == count13)
                        {
                            opeCri.Model.BtnAddVisible = true;
                        }
                        if (lbcri.AssignmentOperatorCriterias.Count == 1)
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

        private string ConvertComType(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var vlue = int.Parse(value);
                switch (vlue)
                {
                    case (int)ComputerTypes.Server:
                        return ComputerTypes.Server.ToString();
                    case (int)ComputerTypes.Desktop:
                        return ComputerTypes.Desktop.ToString();
                    case (int)ComputerTypes.Notebook:
                        return ComputerTypes.Notebook.ToString();
                    default:
                        return ComputerTypes.Desktop.ToString();
                }
            }

            return value;
        }

        #endregion
    }
}