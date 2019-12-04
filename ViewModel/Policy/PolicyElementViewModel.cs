using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
using Tabidus.POC.GUI.UserControls.Policy;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.DirectoryAssignment;

namespace Tabidus.POC.GUI.ViewModel.Policy
{
    public class PolicyElementViewModel : ViewModelBase
    {
        public readonly PolicyElement _view;

        public PolicyElementViewModel(PolicyElement view)
        {
            _view = view;
            EditPolicyCommand = new RelayCommand(ExecuteEditPolicy);
            ChangeSelectedCommand = new RelayCommand(OnChangeSelectedCommand);
        }

        public ICommand EditPolicyCommand { get; private set; }
        public ICommand ChangeSelectedCommand { get; private set; }
        private void ExecuteEditPolicy(object arg)
        {
            //            EditAssigmentRule(er);
        }

        public void OnActiveChanged()
        {
            var policyVm = PageNavigatorHelper.GetMainContentViewModel<POCAgentViewModel>();
            if (policyVm != null)
            {
                policyVm.Id = Id;
                policyVm.Name = Name;
                policyVm.PolicyColor = ExpanderBackgroundColor;

                var pocAgent = ApplicationContext.POCAgentList.Find(r => r.Id == Id);
                if (pocAgent != null)
                {
                    policyVm.POCServer = pocAgent.POCServer;
                    policyVm.Key = pocAgent.Key;
                    policyVm.Port = pocAgent.Port;
                    policyVm.NeighborhoodWatch = pocAgent.NeighborhoodWatch;
                    policyVm.TransferInterval = pocAgent.TransferInterval;
                    policyVm.SynchronizationInterval = pocAgent.SyncInterval;
                    policyVm.ActiveTransfer = pocAgent.ActiveTransfer;
                    policyVm.UpdateSource = pocAgent.UpdateSource;
                    policyVm.pocServerOrigin = pocAgent.POCServer;
                    policyVm.keyOrigin = pocAgent.Key;
                    policyVm.portOrigin = pocAgent.Port;
                }
            }
        }

        private void OnChangeSelectedCommand(object arg)
        {
            if (IsSelected)
            {
                if (!ApplicationContext.PoliciesList.Select(r => r.Id).Contains(Id))
                {
                    ApplicationContext.PoliciesList.Add(this);
                }
            }
            else
            {
                ApplicationContext.PoliciesList.RemoveAll(r => r.Id == Id);
            }
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

        #region Properties

        #region Checkbox

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
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

        private string _color;
        public string ExpanderBackgroundColor
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged("ExpanderBackgroundColor");
            }
        }

        public string TextColor
        {
            get { return "#D2D2D3"; }
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
            }
        }

        #endregion
    }
}