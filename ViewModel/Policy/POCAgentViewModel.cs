using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.POCAgent;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.UserControls.Policy;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Policy
{
    public class POCAgentViewModel : PageViewModelBase
    {
        private static readonly List<string> PoliciesColor = GetPoliciesColor();
        private static List<string> PolicyColorExisted;
        private bool _activeTransfer;
        private bool _btnSaveVisible;
        private string _key;

        private readonly MessageDialog _messageDialog =
            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;

        private bool _neighborhoodWatch;
        private string _pOCServer;

        private string _policyColor;
        private int? _port;

        private int? _synchronizationInterval;
        private int? _transferInterval;
        private bool _updateSource;
        private readonly POCAgentPage _view;

        public POCAgentViewModel(POCAgentPage view)
        {
            _view = view;
            ApplicationContext.PoliciesList = new List<PolicyElementViewModel>();
            TabSelectedCommand = new RelayCommand<Button>(OnMenuSelected, CanMenuAction);
            SavePolicyCommand = new RelayCommand(OnSavePolicyCommand);
            BuildPage();
        }

        public ICommand TabSelectedCommand { get; private set; }
        public ICommand SavePolicyCommand { get; private set; }

        public bool BtnSaveVisible
        {
            get { return _btnSaveVisible; }
            set
            {
                _btnSaveVisible = value;
                OnPropertyChanged("BtnSaveVisible");
            }
        }

        public string Name { get; set; }

        public string PolicyColor
        {
            get { return _policyColor; }
            set
            {
                _policyColor = value;
                OnPropertyChanged("PolicyColor");
            }
        }

        public string POCServer
        {
            get { return _pOCServer; }
            set
            {
                _pOCServer = value;
                OnPropertyChanged("POCServer");
            }
        }

        public int? Port
        {
            get { return _port; }
            set
            {
                _port = value;
                OnPropertyChanged("Port");
            }
        }

        public string Key
        {
            get { return _key; }
            set
            {
                _key = value;
                OnPropertyChanged("Key");
            }
        }

        public int? SynchronizationInterval
        {
            get { return _synchronizationInterval; }
            set
            {
                _synchronizationInterval = value;
                OnPropertyChanged("SynchronizationInterval");
            }
        }

        public int? TransferInterval
        {
            get { return _transferInterval; }
            set
            {
                _transferInterval = value;
                OnPropertyChanged("TransferInterval");
            }
        }

        public bool NeighborhoodWatch
        {
            get { return _neighborhoodWatch; }
            set
            {
                _neighborhoodWatch = value;
                OnPropertyChanged("NeighborhoodWatch");
            }
        }

        public bool UpdateSource
        {
            get { return _updateSource; }
            set
            {
                _updateSource = value;
                OnPropertyChanged("UpdateSource");
            }
        }

        public bool ActiveTransfer
        {
            get { return _activeTransfer; }
            set
            {
                _activeTransfer = value;
                OnPropertyChanged("ActiveTransfer");
            }
        }

        public bool IsEditing { get; set; }
        public int Id { get; set; }
        public string pocServerOrigin { get; set; }
        public string keyOrigin { get; set; }
        public int? portOrigin { get; set; }

        private static List<string> GetPoliciesColor()
        {
            var colorConfig = Functions.GetConfig("POCAgent_Colors", "");
            var cc = colorConfig.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            return cc;
        }

        private void OnSavePolicyCommand(object arg)
        {
            if (string.IsNullOrWhiteSpace(POCServer) || string.IsNullOrWhiteSpace(Key) || Port == null)
            {
                _messageDialog.ShowMessageDialog("Server information (POCServer, Port, Key) cannot be null", "Message");
                return;
            }
            if (SynchronizationInterval == null || TransferInterval == null ||
                (SynchronizationInterval != null && SynchronizationInterval <= 0) ||
                (TransferInterval != null && TransferInterval <= 0))
            {
                _messageDialog.ShowMessageDialog("Time interval cannot be null or less than 0", "Message");
                return;
            }
            var pocAgent = new POCAgent
            {
                POCServer = POCServer,
                Key = Key,
                Port = Port.Value,
                Name = Name,
                NeighborhoodWatch = NeighborhoodWatch,
                ActiveTransfer = ActiveTransfer,
                UpdateSource = UpdateSource,
                SyncInterval = SynchronizationInterval.Value,
                TransferInterval = TransferInterval.Value,
                Color = PolicyColor,
                Id = Id
            };

            if (pocServerOrigin != POCServer || portOrigin != Port.Value || keyOrigin != Key)
            {
                var confirmDlg = new ConfirmDialog("Are you sure you want to save these informations?", "Save policy");
                if (confirmDlg.ShowDialog() == true)
                {
                    using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                    {
                        var requestData = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(pocAgent),
                            KeyEncryption);
                        sc.EditPolicy(requestData);
                        UpdatePocAgent(pocAgent);
                    }
                }
            }
            else
            {
                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var requestData = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(pocAgent),
                        KeyEncryption);
                    sc.EditPolicy(requestData);
                    UpdatePocAgent(pocAgent);
                }
            }
        }

        private void UpdatePocAgent(POCAgent pa)
        {
            var pocAgent = ApplicationContext.POCAgentList.Find(r => r.Id == pa.Id);
            if (pocAgent != null)
            {
                pocAgent.POCServer = pa.POCServer;
                pocAgent.Port = pa.Port;
                pocAgent.Key = pa.Key;
                pocAgent.NeighborhoodWatch = pa.NeighborhoodWatch;
                pocAgent.Name = pa.Name;
                pocAgent.UpdateSource = pa.UpdateSource;
                pocAgent.ActiveTransfer = pa.ActiveTransfer;
                pocAgent.SyncInterval = pa.SyncInterval;
                pocAgent.TransferInterval = pa.TransferInterval;
            }
        }

        public void AssignPolicy(List<PolicyAssign> lpa)
        {
            using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            {
                var requestData = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(lpa),
                    KeyEncryption);
                sc.AssignPolicy(requestData);
            }
        }

        public void DirtyCheckAcceptChanges()
        {
            _view.Tracker.AcceptChanges();
        }

        public override void Refresh()
        {
            Functions.GetAllPolicies();
            BuildPage();
        }

        private void BuildPage()
        {
            ApplicationContext.PoliciesList = new List<PolicyElementViewModel>();
            if (ApplicationContext.POCAgentList == null)
            {
                Functions.GetAllPolicies();
            }
            if (ApplicationContext.POCAgentList.Count > 0)
            {
                BtnSaveVisible = true;
            }
            else
            {
                BtnSaveVisible = false;
            }
            _view.PnlPolicyContainer.Children.Clear();
            PolicyColorExisted = new List<string>();
            var count = 0;
            foreach (var pocAgent in ApplicationContext.POCAgentList)
            {
                var policyElem = new PolicyElement();
                policyElem.Model.Name = pocAgent.Name;
                policyElem.Model.Id = pocAgent.Id;
                policyElem.Model.ExpanderBackgroundColor = pocAgent.Color;
                _view.PnlPolicyContainer.Children.Add(policyElem);
                PolicyColorExisted.Add(pocAgent.Color);


                if (count == 0 && Id == 0)
                {
                    policyElem.Model.IsActived = true;
                    POCServer = pocAgent.POCServer;
                    Key = pocAgent.Key;
                    Port = pocAgent.Port;
                    pocServerOrigin = pocAgent.POCServer;
                    portOrigin = pocAgent.Port;
                    keyOrigin = pocAgent.Key;
                    UpdateSource = pocAgent.UpdateSource;
                    ActiveTransfer = pocAgent.ActiveTransfer;
                    SynchronizationInterval = pocAgent.SyncInterval;
                    TransferInterval = pocAgent.TransferInterval;
                    NeighborhoodWatch = pocAgent.NeighborhoodWatch;
                    Id = pocAgent.Id;
                    Name = pocAgent.Name;
                    PolicyColor = pocAgent.Color;
                }
                else if (Id == pocAgent.Id)
                {
                    policyElem.Model.IsActived = true;
                    POCServer = pocAgent.POCServer;
                    Key = pocAgent.Key;
                    Port = pocAgent.Port;
                    pocServerOrigin = pocAgent.POCServer;
                    portOrigin = pocAgent.Port;
                    keyOrigin = pocAgent.Key;
                    UpdateSource = pocAgent.UpdateSource;
                    ActiveTransfer = pocAgent.ActiveTransfer;
                    SynchronizationInterval = pocAgent.SyncInterval;
                    TransferInterval = pocAgent.TransferInterval;
                    NeighborhoodWatch = pocAgent.NeighborhoodWatch;
                    Name = pocAgent.Name;
                    PolicyColor = pocAgent.Color;
                }

                count++;
            }
        }

        private bool CanMenuAction(Button btn)
        {
            if (btn == null)
                return false;
            switch (btn.Name)
            {
                case UIConstant.BtnPolicyDuplicate:
                case UIConstant.BtnPolicyAssign:
                    return ApplicationContext.PoliciesList != null && ApplicationContext.PoliciesList.Count == 1;
                case UIConstant.BtnPolicyDelete:
                    return ApplicationContext.PoliciesList.Count > 0;
            }
            return true;
        }

        private void OnMenuSelected(Button btn)
        {
            if (btn == null)
                return;

            switch (btn.Name)
            {
                case UIConstant.BtnPolicyAdd:
                    AddNewPolicy();
                    break;
                case UIConstant.BtnPolicyDuplicate:
                    DuplicateCurrentPolicy();
                    break;
                case UIConstant.BtnPolicyDelete:
                    DeletePolicy();
                    break;
                case UIConstant.BtnPolicyAssign:
                    ShowAssignPolicyDialog();
                    break;
            }
        }

        private void DuplicateCurrentPolicy()
        {
            try
            {
                ApplicationContext.IsBusy = true;
                var selectedElement = ApplicationContext.PoliciesList.FirstOrDefault();
                var selectedPocAgent = ApplicationContext.POCAgentList.Find(p => p.Id == selectedElement.Id);
                if (selectedPocAgent != null)
                {
                    var labelName = String.Format("{0} copy", selectedPocAgent.Name);
                    var maxCount = FindPolicyIndexByTitle(labelName, labelName.ToUpper());
                    if (maxCount >= 1)
                    {
                        labelName += " (" + maxCount + ")";
                    }
                    var policy = CreatePolicyElementByName(labelName);
                    _view.PnlPolicyContainer.Children.Add(policy);
                    PolicyColorExisted.Add(policy.Model.ExpanderBackgroundColor);

                    var pocAgent = new POCAgent
                    {
                        POCServer = selectedPocAgent.POCServer,
                        Key = selectedPocAgent.Key,
                        Port = selectedPocAgent.Port,
                        Name = labelName,
                        NeighborhoodWatch = selectedPocAgent.NeighborhoodWatch,
                        ActiveTransfer = selectedPocAgent.ActiveTransfer,
                        UpdateSource = selectedPocAgent.UpdateSource,
                        SyncInterval = selectedPocAgent.SyncInterval,
                        TransferInterval = selectedPocAgent.TransferInterval,
                        Color = policy.Model.ExpanderBackgroundColor
                    };

                    policy.Model.Id = Id = AddPocAgentToDb(pocAgent);
                    pocAgent.Id = Id;
                    ApplicationContext.POCAgentList.Add(pocAgent);
                    policy.ActiveCurrentExpander();
                    BtnSaveVisible = true;
                }
            }
            catch (Exception e)
            {
                ApplicationContext.IsBusy = false;
                Logger.Error(e.Message, e);
            }

        }

        private PolicyElement CreatePolicyElementByName(string labelName)
        {
            var policy = new PolicyElement();
            policy.Model.Name = labelName;
            var pcolor = "";
            foreach (var color in PoliciesColor)
            {
                if (!PolicyColorExisted.Contains(color))
                {
                    pcolor = color;
                    break;
                }
            }
            policy.Model.IsAddState = true;
            policy.Model.ExpanderBackgroundColor = pcolor;
            policy._editTimeStart = DateTime.Now;
            return policy;
        }
        private void AddNewPolicy()
        {
            try
            {
                ApplicationContext.IsBusy = true;
                var labelName = "New Policy";
                var maxCount = FindPolicyIndex();
                if (maxCount >= 2)
                {
                    labelName += " (" + maxCount + ")";
                }
                var policy = CreatePolicyElementByName(labelName);
                _view.PnlPolicyContainer.Children.Add(policy);
                PolicyColorExisted.Add(policy.Model.ExpanderBackgroundColor);

                Uri myUri = new Uri(ApplicationContext.ServerAddress);
                var pocAgent = new POCAgent
                {
                    POCServer = myUri.Host,
                    Key = KeyEncryption,
                    Port = myUri.Port,
                    Name = labelName,
                    NeighborhoodWatch = true,
                    ActiveTransfer = ActiveTransfer,
                    UpdateSource = UpdateSource,
                    SyncInterval = 60,
                    TransferInterval = 30,
                    Color = policy.Model.ExpanderBackgroundColor
                };

                policy.Model.Id = Id = AddPocAgentToDb(pocAgent);
                pocAgent.Id = Id;
                ApplicationContext.POCAgentList.Add(pocAgent);
                policy.ActiveCurrentExpander();
                BtnSaveVisible = true;
            }
            catch (Exception e)
            {
                ApplicationContext.IsBusy = false;
                Logger.Error(e.Message, e);
            }

        }

        private int AddPocAgentToDb(POCAgent pocAgent)
        {
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<DataResponse>(
                        sc.AddPolicy, pocAgent));
            return resultDeserialize.Result;
        }

        private void DeletePolicy()
        {
            var confirmDlg = new ConfirmDialog("Are you sure you want to delete these informations?", "Delete policy");
            if (confirmDlg.ShowDialog() == true)
            {
                if (ApplicationContext.PoliciesList != null)
                {
                    var listId = ApplicationContext.PoliciesList.Select(r => r.Id).ToList();

                    var requestObj = new StringAuthenticateObject
                    {
                        StringAuth = "OK",
                        StringValue = string.Join(",", listId)
                    };
                    using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                    {
                        var requestData = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(requestObj),
                            KeyEncryption);
                        sc.DeletePolicy(requestData);
                    }

                    foreach (var policyElementViewModel in ApplicationContext.PoliciesList)
                    {
                        //policyElementViewModel.IsActived = true;
                        _view.PnlPolicyContainer.Children.Remove(policyElementViewModel._view);
                    }
                    ApplicationContext.POCAgentList.RemoveAll(
                        r => ApplicationContext.PoliciesList.Select(r2 => r2.Id).Contains(r.Id));
                    ApplicationContext.PoliciesList.Clear();

                    if (_view.PnlPolicyContainer.Children.Count > 0 && ApplicationContext.POCAgentList.Count > 0)
                    {
                        var item = ApplicationContext.POCAgentList.FirstOrDefault(i => i.Id == Id);
                        if (item == null)
                        {
                            Id = ApplicationContext.POCAgentList.FirstOrDefault().Id;
                            BuildPage();
                        }
                    }
                    else
                    {
                        BtnSaveVisible = false;
                    }
                }
            }
        }

        private void ShowAssignPolicyDialog()
        {
            ApplicationContext.SelectedTargetNodes = new List<DirectoryNode>();
            var dlg = new EndpointDirectoryDialog();
            dlg.Model.MakeTree();
            PageNavigatorHelper._MainWindow.DynamicShowDialog(dlg, null, "Assign Policy");
        }
        private int FindPolicyIndex()
        {
            return FindPolicyIndexByTitle("New Policy", "NEW POLICY");
        }

        private int FindPolicyIndexByTitle(string policyTitle, string policyTitleUpper)
        {
            var numberCounts = new List<int>();
            var labelControls = _view.PnlPolicyContainer.Children;
            foreach (var lbel in labelControls)
            {
                if (lbel.GetType() == typeof(PolicyElement))
                {
                    var lcri = lbel as PolicyElement;
                    var header = lcri.Expander.Header.ToString();
                    if (header.IndexOf(policyTitle, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var headerUp = header.ToUpper();
                        string[] separatingChars = { policyTitleUpper };
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
    }
}