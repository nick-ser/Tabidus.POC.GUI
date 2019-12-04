using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Newtonsoft.Json;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.UserControls.DirectoryAssignment;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class AssignmentCriteriaViewModel : ViewModelBase
    {
        private readonly Dictionary<string, ComputerTypes> _dictMapComputerType =
            new Dictionary<string, ComputerTypes>(StringComparer.OrdinalIgnoreCase)
            {
                {"Server", ComputerTypes.Server},
                {"Desktop", ComputerTypes.Desktop},
                {"Notebook", ComputerTypes.Notebook}
            };

        private readonly AssignmentCriterialElement _view;

        public AssignmentCriteriaViewModel(AssignmentCriterialElement view)
        {
            _view = view;
            EditRuleCommand = new RelayCommand(ExecuteEditRule);
            EditRuleCriteriaCommand = new RelayCommand(OnSaveRuleCriteria);
            DeleteRuleCommand = new RelayCommand(OnDeleteRuleCommand);
        }

        public ICommand EditRuleCommand { get; private set; }
        public ICommand EditRuleCriteriaCommand { get; private set; }
        public ICommand DeleteRuleCommand { get; private set; }

        public void OnSaveRuleCriteria(object pars)
        {
            var backgroundHelper = new BackgroundWorkerHelper();
            backgroundHelper.AddDoWork(SaveBackgroundWorker_DoWork).DoWork();
        }

        private void OnDeleteRuleCommand(object arg)
        {
            var assignmentViewModel = PageNavigatorHelper.GetMainContentViewModel<AssignmentViewModel>();
            assignmentViewModel.DeleteRuleCommand.Execute(Id);
        }

        private void SaveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                try
                {
                    using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                    {
                        var dataXml = BuildXmlDataEdit();
                        var ldapXml = BuildLDAPXmlCriteria();
                        var request = new EditAssignmentRuleCriteriaDataRequest { XmlData = dataXml, DirectoryId = FolderId, LdapXmlData = ldapXml };
                        var datareq = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(request),
                            KeyEncryption);
                        sc.EditAssignmentRuleCriteria(datareq);
                        var requestObj = new GetRuleDataRequest { DirectoryId = 1 };
                        var data = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(requestObj), KeyEncryption);
                        var result = sc.GetRules(data);
                        var resultDeserialize = JsonConvert.DeserializeObject<GetRulesDataResponse>(EncryptionHelper.DecryptRijndael(result, KeyEncryption));
                        if (resultDeserialize != null)
                        {
                            ApplicationContext.AssignmentRulesData = resultDeserialize;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);

                    var messageDialog =
                        PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;

                    messageDialog.ShowMessageDialog(
                        "Cannot save directory assignment criteria due to exception occured, please see the log file under the Logs for more information",
                        "Save directory assignment criteria");
                }
            }));
        }

        private string ReplaceTag(string source)
        {
            if (!string.IsNullOrWhiteSpace(source))
                return source.Replace("<", "#open;").Replace(">", "#close;").Replace("&", "#and;");
            return source;
        }

        private ComputerTypes ConvertComTypeFromString(string value)
        {
            if (_dictMapComputerType.ContainsKey(value))
                return _dictMapComputerType[value];
            return ComputerTypes.Desktop;
        }

        private string BuildLDAPXmlCriteria()
        {
            var xmlDataBuilder = new StringBuilder();
            xmlDataBuilder.Append("<DataSet>");
            //Build the LDAP xml data
            var ldapcri = _view.PnlLDAPCri.Children;
            foreach (var ldap in ldapcri)
            {
                if (ldap.GetType() == typeof(LDAPAssignment))
                {
                    var cvm = (ldap as LDAPAssignment).Model;
                    if (cvm.ChbLDAPChecked == true)
                    {
                        foreach (var child in _view.PnlLDAPCri.Children)
                        {
                            if (child.GetType() == typeof(LDAPAssignment))
                            {
                                var ldapElem = child as LDAPAssignment;
                                ldapElem.FindChild<ComboBox>("CbLDAP");
                                ldapElem.FindChild<ComboBox>("CbLDAP").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                ldapElem.FindChild<ComboBox>("CbLDAP").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                ldapElem.FindChild<TextBox>("TxtLDAPFolder").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                ldapElem.FindChild<TextBox>("TxtLDAPFolder").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                ldapElem.FindChild<TextBox>("TxtExcludeFolder").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                ldapElem.FindChild<TextBox>("TxtExcludeFolder").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                ldapElem.FindChild<TextBox>("TxtExcludeComputer").Background = (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                ldapElem.FindChild<TextBox>("TxtExcludeComputer").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                            }
                        }
                    }
                    else
                    {
                        foreach (var child in _view.PnlLDAPCri.Children)
                        {
                            if (child.GetType() == typeof(LDAPAssignment))
                            {
                                var ldapElem = child as LDAPAssignment;
                                ldapElem.FindChild<ComboBox>("CbLDAP");
                                ldapElem.FindChild<ComboBox>("CbLDAP").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<ComboBox>("CbLDAP").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                ldapElem.FindChild<TextBox>("TxtLDAPFolder").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<TextBox>("TxtLDAPFolder").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                ldapElem.FindChild<TextBox>("TxtExcludeFolder").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<TextBox>("TxtExcludeFolder").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                ldapElem.FindChild<TextBox>("TxtExcludeComputer").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<TextBox>("TxtExcludeComputer").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                            }
                        }
                    }
                    xmlDataBuilder.Append("<LDAPRuleCriteria Id=\"" + cvm.Id + "\" AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<DomainId>" + cvm.CbLDAPSelected +
                                          "</DomainId>");
                    xmlDataBuilder.Append("<FolderId>" + cvm.LDAPFolderId +
                                          "</FolderId>");
                    xmlDataBuilder.Append("<ExcludeSubFolder>" + string.Join(";", cvm.ExcludeFolderIds) +
                                          "</ExcludeSubFolder>");
                    xmlDataBuilder.Append("<ExcludeComputer>" + string.Join(";", cvm.ExcludeComputerIds) +
                                          "</ExcludeComputer>");
                    xmlDataBuilder.Append("<IsExcludeEmptyFolder>" + cvm.ChbExcludeEmptyFolderChecked +
                                          "</IsExcludeEmptyFolder>");
                    xmlDataBuilder.Append("<IsAddOnlyComputer>" + cvm.ChbAddOnlyComputerChecked + "</IsAddOnlyComputer>");
                    xmlDataBuilder.Append("<IsFolderActive>" + cvm.ChbLDAPFolderChecked + "</IsFolderActive>");
                    xmlDataBuilder.Append("<IsExcludeSubFolderActive>" + cvm.ChbExcludeFolderChecked +
                                          "</IsExcludeSubFolderActive>");
                    xmlDataBuilder.Append("<IsExcludeComputerActive>" + cvm.ChbExcludeComputerChecked +
                                          "</IsExcludeComputerActive>");
                    xmlDataBuilder.Append("<IsActive>" + cvm.ChbLDAPChecked + "</IsActive>");
                    xmlDataBuilder.Append("<FolderPath>" + cvm.TxtLDAPFolder + "</FolderPath>");
                    xmlDataBuilder.Append("<ExcludeSubFolderPath>" + cvm.TxtExcludeFolder + "</ExcludeSubFolderPath>");
                    xmlDataBuilder.Append("<ExcludeComputerPath>" + cvm.TxtExcludeComputer + "</ExcludeComputerPath>");
                    xmlDataBuilder.Append("</LDAPRuleCriteria>");
                }
            }
            xmlDataBuilder.Append("</DataSet>");
            return xmlDataBuilder.ToString();
        }

        private string BuildXmlDataEdit()
        {
            var xmlDataBuilder = new StringBuilder();
            xmlDataBuilder.Append("<DataSet>");

            //Build the xml data
            //data for Unmanaged endpoints
            xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
            xmlDataBuilder.Append("<IsAvailable>" + ChbUnmangedChecked +
                                  "</IsAvailable>");
            xmlDataBuilder.Append("<CriteriaId>" + UnmanagedCriteriaId +
                                  "</CriteriaId>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.UnmanagedEndpoints +
                                  "</FieldName>");
            xmlDataBuilder.Append("<Operator></Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("</AssignmentRule>");
            //data for Managed endpoints
            xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
            xmlDataBuilder.Append("<IsAvailable>" + ChbMangedChecked +
                                  "</IsAvailable>");
            xmlDataBuilder.Append("<CriteriaId>" + ManagedCriteriaId +
                                  "</CriteriaId>");
            xmlDataBuilder.Append("<FieldName>" + CommonConstants.ManagedEndpoints +
                                  "</FieldName>");
            xmlDataBuilder.Append("<Operator></Operator>");
            xmlDataBuilder.Append("<Value1></Value1>");
            xmlDataBuilder.Append("<Value2></Value2>");
            xmlDataBuilder.Append("</AssignmentRule>");
            //data for computer name
            #region Computer name criteria old code
            var computerOpes = _view.PnlComputerNameCri.Children;
            foreach (var co in computerOpes)
            {
                if (co.GetType() == typeof(ComputerNameCriteriaElement))
                {
                    var cvm = (co as ComputerNameCriteriaElement).Model;
                    //Added effect for combobox and textbox while checkbox is checked/unchecked 13-04-2019
                    if (ChbComputerChecked == true)
                    {
                        foreach (var child in _view.PnlComputerNameCri.Children)
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
                        foreach (var child in _view.PnlComputerNameCri.Children)
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
                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbComputerChecked +
                                          "</IsAvailable>");
                    xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                          "</CriteriaId>");
                    xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointComputerName +
                                          "</FieldName>");
                    xmlDataBuilder.Append("<Operator>" + cvm.CbComputerOpeSelected +
                                          "</Operator>");
                    xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtComputerCriteria) + "</Value1>");
                    xmlDataBuilder.Append("<Value2></Value2>");
                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }
            #endregion

            //data for vendor
            var vendorOpes = _view.PnlVendorCri.Children;
            foreach (var co in vendorOpes)
            {
                if (co.GetType() == typeof(VendorCriteriaElement))
                {
                    var cvm = (co as VendorCriteriaElement).Model;
                    if (ChbVendorChecked == true)
                    {
                        foreach (var child in _view.PnlVendorCri.Children)
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
                        //_view.PnlVendorCri.Opacity = 0.5;

                    }
                    else
                    {
                        foreach (var child in _view.PnlVendorCri.Children)
                        {
                            if (child.GetType() == typeof(VendorCriteriaElement))
                            {
                                var ldapElem = child as VendorCriteriaElement;
                                ldapElem.FindChild<ComboBox>("CbVendorOpes");
                                ldapElem.FindChild<ComboBox>("CbVendorOpes").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<ComboBox>("CbVendorOpes").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                ldapElem.FindChild<TextBox>("TxtVendorCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#FFF");
                                ldapElem.FindChild<TextBox>("TxtVendorCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                ldapElem.FindChild<ComboBox>("CbVendorCriteria").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<ComboBox>("CbVendorCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                ldapElem.FindChild<Button>("bntplus_vendor").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<Button>("bntminus_vendor").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<Button>("bntplus_vendor").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                ldapElem.FindChild<Button>("bntminus_vendor").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                //ldapElem.Model.LabelOrVisible = false;
                            }
                        }
                        //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                        //_view.PnlVendorCri.Opacity = 0.5;
                    }

                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbVendorChecked +
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
                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }

            //data for model
            var modelOpes = _view.PnlModelCri.Children;
            foreach (var co in modelOpes)
            {
                if (co.GetType() == typeof(ModelCriteriaElement))
                {
                    var cvm = (co as ModelCriteriaElement).Model;
                    if (ChbModelChecked == true)
                    {
                        foreach (var child in _view.PnlModelCri.Children)
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
                        foreach (var child in _view.PnlModelCri.Children)
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
                                ldapElem.FindChild<ComboBox>("CbModelCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                ldapElem.FindChild<Button>("bntplus_model").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<Button>("bntminus_model").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<Button>("bntplus_model").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                ldapElem.FindChild<Button>("bntminus_model").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");//ldapElem.Model.LabelOrVisible = false;
                            }
                        }
                        //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                        //_view.PnlModelCri.Opacity = 0.5;
                    }
                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbModelChecked +
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
                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }

            //data for operating system
            var osOpes = _view.PnlOsCri.Children;
            foreach (var co in osOpes)
            {
                if (co.GetType() == typeof(OSCriteriaElement))
                {
                    var cvm = (co as OSCriteriaElement).Model;
                    if (ChbOsChecked == true)
                    {
                        foreach (var child in _view.PnlOsCri.Children)
                        {
                            if (child.GetType() == typeof(OSCriteriaElement))
                            {
                                var ldapElem = child as OSCriteriaElement;
                                ldapElem.FindChild<ComboBox>("CbOsOpes");
                                ldapElem.FindChild<ComboBox>("CbOsOpes").Background =
                                    (Brush)new BrushConverter().ConvertFrom("#4d000000");
                                ldapElem.FindChild<ComboBox>("CbOsOpes").Foreground =
                                    (Brush)new BrushConverter().ConvertFrom("#FFF");
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
                        foreach (var child in _view.PnlOsCri.Children)
                        {
                            if (child.GetType() == typeof(OSCriteriaElement))
                            {
                                var ldapElem = child as OSCriteriaElement;
                                ldapElem.FindChild<ComboBox>("CbOsOpes");
                                ldapElem.FindChild<ComboBox>("CbOsOpes").Background =
                                    (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<ComboBox>("CbOsOpes").Foreground =
                                    (Brush)new BrushConverter().ConvertFrom("#8E8f93");
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
                    //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);

                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                        xmlDataBuilder.Append("<IsAvailable>" + ChbOsChecked +
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
                        xmlDataBuilder.Append("</AssignmentRule>");
                    
                }
            }

            //data for Platform
            var platformOpes = _view.PnlPlatformCri.Children;
            foreach (var co in platformOpes)
            {
                if (co.GetType() == typeof(PlatformCriteriaElement))
                {
                    var cvm = (co as PlatformCriteriaElement).Model;
                    if (ChbPlatformChecked == true)
                    {
                        foreach (var child in _view.PnlPlatformCri.Children)
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
                        foreach (var child in _view.PnlPlatformCri.Children)
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
                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbPlatformChecked +
                                          "</IsAvailable>");
                    xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                          "</CriteriaId>");
                    xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointPlatform + "</FieldName>");
                    xmlDataBuilder.Append("<Operator>contains</Operator>");
                    xmlDataBuilder.Append("<Value1>" + cvm.CbPlatformOpeSelected + "</Value1>");
                    xmlDataBuilder.Append("<Value2></Value2>");
                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }

            //data for computer type
            var comTypeOpes = _view.PnlComputerTypeCri.Children;
            foreach (var co in comTypeOpes)
            {
                if (co.GetType() == typeof(ComputerTypeCriteriaElement))
                {
                    var cvm = (co as ComputerTypeCriteriaElement).Model;
                    if (ChbComputerTypeChecked == true)
                    {
                        foreach (var child in _view.PnlComputerTypeCri.Children)
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
                        foreach (var child in _view.PnlComputerTypeCri.Children)
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
                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbComputerTypeChecked +
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
                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }

            //data for domain
            var domainOpes = _view.PnlDomainCri.Children;
            foreach (var co in domainOpes)
            {
                if (co.GetType() == typeof(DomainCriteriaElement))
                {
                    var cvm = (co as DomainCriteriaElement).Model;
                    if (ChbDomainChecked == true)
                    {
                        foreach (var child in _view.PnlDomainCri.Children)
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
                        foreach (var child in _view.PnlDomainCri.Children)
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
                                ldapElem.FindChild<ComboBox>("CbDomainCriteria").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                ldapElem.FindChild<Button>("btnolus_domain").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<Button>("btnminus_domain").Background = (Brush)new BrushConverter().ConvertFrom("#33000000");
                                ldapElem.FindChild<Button>("btnolus_domain").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                                ldapElem.FindChild<Button>("btnminus_domain").Foreground = (Brush)new BrushConverter().ConvertFrom("#8E8f93");
                            }
                        }
                        //_view.PnlComputerNameCri.Background = new SolidColorBrush(Colors.Yellow);
                        //_view.PnlDomainCri.Opacity = 0.5;
                    }
                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbDomainChecked +
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
                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }

            //data for memory
            var memoryOpes = _view.PnlMemoryCri.Children;
            foreach (var co in memoryOpes)
            {
                if (co.GetType() == typeof(MemoryCriteriaElement))
                {
                    var cvm = (co as MemoryCriteriaElement).Model;
                    if (ChbMemoryChecked == true)
                    {
                        foreach (var child in _view.PnlMemoryCri.Children)
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
                        foreach (var child in _view.PnlMemoryCri.Children)
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
                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbMemoryChecked +
                                          "</IsAvailable>");
                    xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                          "</CriteriaId>");
                    xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointMemory + "</FieldName>");
                    xmlDataBuilder.Append("<Operator>" + cvm.CbMemoryOpeSelected + "</Operator>");
                    xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtMemoryCriteria) + "</Value1>");
                    xmlDataBuilder.Append("<Value2></Value2>");
                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }

            //data for Harddisk
            var harddiskOpes = _view.PnlHarddiskCri.Children;
            foreach (var co in harddiskOpes)
            {
                if (co.GetType() == typeof(HarddiskCriteriaElement))
                {
                    var cvm = (co as HarddiskCriteriaElement).Model;
                    if (ChbHarddiskChecked == true)
                    {
                        foreach (var child in _view.PnlHarddiskCri.Children)
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
                        foreach (var child in _view.PnlHarddiskCri.Children)
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
                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbHarddiskChecked +
                                          "</IsAvailable>");
                    xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                          "</CriteriaId>");
                    xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointHarddisk + "</FieldName>");
                    xmlDataBuilder.Append("<Operator>" + cvm.CbHarddiskOpeSelected + "</Operator>");
                    xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtHarddiskCriteria) + "</Value1>");
                    xmlDataBuilder.Append("<Value2></Value2>");
                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }

            //data for IPv4
            var ipv4Opes = _view.PnlIPv4Cri.Children;
            foreach (var co in ipv4Opes)
            {
                if (co.GetType() == typeof(IPv4CriteriaElement))
                {
                    var cvm = (co as IPv4CriteriaElement).Model;
                    if (ChbIPv4Checked == true)
                    {
                        foreach (var child in _view.PnlIPv4Cri.Children)
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
                        foreach (var child in _view.PnlIPv4Cri.Children)
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
                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbIPv4Checked +
                                          "</IsAvailable>");
                    xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                          "</CriteriaId>");
                    xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointIPv4 + "</FieldName>");
                    xmlDataBuilder.Append("<Operator>" + cvm.CbIPv4OpeSelected + "</Operator>");
                    xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtIPv4Criteria1) + "</Value1>");
                    xmlDataBuilder.Append("<Value2>" + ReplaceTag(cvm.TxtIPv4Criteria2) + "</Value2>");

                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }

            //data for IPv6
            var ipv6Opes = _view.PnlIPv6Cri.Children;
            foreach (var co in ipv6Opes)
            {
                if (co.GetType() == typeof(IPv6CriteriaElement))
                {
                    var cvm = (co as IPv6CriteriaElement).Model;
                    if (ChbIPv6Checked == true)
                    {
                        foreach (var child in _view.PnlIPv6Cri.Children)
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
                        foreach (var child in _view.PnlIPv6Cri.Children)
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
                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbIPv6Checked +
                                          "</IsAvailable>");
                    xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                          "</CriteriaId>");
                    xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointIPv6 + "</FieldName>");
                    xmlDataBuilder.Append("<Operator>" + cvm.CbIPv6OpeSelected + "</Operator>");
                    xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtIPv6Criteria1) + "</Value1>");
                    xmlDataBuilder.Append("<Value2>" + ReplaceTag(cvm.TxtIPv6Criteria2) + "</Value2>");

                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }

            //data for Last sync
            var lastsyncOpes = _view.PnlLastSyncCri.Children;
            foreach (var co in lastsyncOpes)
            {
                if (co.GetType() == typeof(LastSyncCriteriaElement))
                {
                    var cvm = (co as LastSyncCriteriaElement).Model;
                    if (ChbLastSyncChecked == true)
                    {
                        foreach (var child in _view.PnlLastSyncCri.Children)
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
                        foreach (var child in _view.PnlLastSyncCri.Children)
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
                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbLastSyncChecked +
                                          "</IsAvailable>");
                    xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                          "</CriteriaId>");
                    xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointLastSynchronization +
                                          "</FieldName>");
                    xmlDataBuilder.Append("<Operator>" + cvm.CbLastSyncOpeSelected + "</Operator>");
                    xmlDataBuilder.Append("<Value1>" + ReplaceTag(cvm.TxtLastSyncCriteria) + "</Value1>");
                    xmlDataBuilder.Append("<Value2></Value2>");
                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }

            //data for Color code
            var clOpes = _view.PnlColorCodeCri.Children;
            foreach (var co in clOpes)
            {
                if (co.GetType() == typeof(ColorCodeCriteriaElement))
                {
                    var cvm = (co as ColorCodeCriteriaElement).Model;
                    if (ChbColorCodeChecked == true)
                    {
                        foreach (var child in _view.PnlColorCodeCri.Children)
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
                        foreach (var child in _view.PnlColorCodeCri.Children)
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
                    xmlDataBuilder.Append("<AssignmentRule AssignmentRuleId=\"" + Id + "\">");
                    xmlDataBuilder.Append("<IsAvailable>" + ChbColorCodeChecked +
                                          "</IsAvailable>");
                    xmlDataBuilder.Append("<CriteriaId>" + cvm.CriteriaId +
                                          "</CriteriaId>");
                    xmlDataBuilder.Append("<FieldName>" + CommonConstants.EndpointColorCode + "</FieldName>");
                    xmlDataBuilder.Append("<Operator>is</Operator>");
                    xmlDataBuilder.Append("<Value1>" + cvm.CbColorCodeOpeSelected + "</Value1>");
                    xmlDataBuilder.Append("<Value2></Value2>");
                    xmlDataBuilder.Append("</AssignmentRule>");
                }
            }

            xmlDataBuilder.Append("</DataSet>");
            return xmlDataBuilder.ToString();
        }

        private void ExecuteEditRule(object arg)
        {
            int selectedFolderId = ApplicationContext.NodesSelected[0].NodeId;
            var er = new EditRuleDataRequest(Name, Id, selectedFolderId, IsEnable);
            EditAssigmentRule(er);
        }

        public void EditAssigmentRule(EditRuleDataRequest ar)
        {
            var _bgWorkerHelper = new BackgroundWorkerHelper();
            _bgWorkerHelper.AddDoWork(OnEditRuleDoWork)
                .DoWork(ar);
        }

        private void OnEditRuleDoWork(object sender, DoWorkEventArgs e)
        {
            var ar = e.Argument as EditRuleDataRequest;
            if (ar.Name != Name)
            {
                ServiceManager.Invoke(sc => RequestResponseUtils.GetData<DataResponse>(sc.EditRule, ar));

                var cri = ApplicationContext.AssignmentRulesData.Result.AssignmentRules.Find(r => r.Id == Id);
                if (cri != null)
                {
                    cri.Name = ar.Name;
                }
                Name = ar.Name;
            }
            else
            {
                var cri = ApplicationContext.AssignmentRulesData.Result.AssignmentRules.Find(r => r.Id == Id);
                ar.Name = cri.Name;
                ServiceManager.Invoke(sc => RequestResponseUtils.GetData<DataResponse>(sc.EditRule, ar));
            }

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

        public int FolderId { get; set; }

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

        public int ManagedCriteriaId { get; set; }

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

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
            }
        }

        private int _id;

        public int Id
        {
            get { return _id; }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
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

        public bool IsNotAddState
        {
            get { return !_isAddState; }
        }

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
    }
}