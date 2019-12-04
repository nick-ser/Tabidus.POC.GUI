using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.UserControls.Software;

namespace Tabidus.POC.GUI.ViewModel.Software
{
    public class MainSchedulingViewModel : ViewModelBase
    {
        private bool _canTransferAfterNewContent;
        private bool _canTransferEveryHours;

        private int? _transferEveryHours;
        private readonly MainSchedulingElement _view;

        public MainSchedulingViewModel(MainSchedulingElement view)
        {
            _view = view;
            ChangeScheduleCommand = new RelayCommand(OnChangeScheduleCommand);
            Build();
        }

        public ICommand ChangeScheduleCommand { get; private set; }

        public bool CanTransferEveryHours
        {
            get { return _canTransferEveryHours; }
            set
            {
                _canTransferEveryHours = value;
                OnPropertyChanged("CanTransferEveryHours");
            }
        }

        public bool CanTransferAfterNewContent
        {
            get { return _canTransferAfterNewContent; }
            set
            {
                _canTransferAfterNewContent = value;
                OnPropertyChanged("CanTransferAfterNewContent");
            }
        }

        public int? TransferEveryHours
        {
            get { return _transferEveryHours; }
            set
            {
                _transferEveryHours = value;
                OnPropertyChanged("TransferEveryHours");
            }
        }

        public int TransferScheduleId { get; set; }

        private bool _isUpdateSourceNotChecked = true;

        public bool IsUpdateSourceNotChecked
        {
            get { return _isUpdateSourceNotChecked; }
            set
            {
                _isUpdateSourceNotChecked = value;
                OnPropertyChanged("IsUpdateSourceNotChecked");
            }
        }

        private void OnChangeScheduleCommand(object arg)
        {
            var saveScheduleBg = new BackgroundWorker();
            saveScheduleBg.DoWork += SaveScheduleBg_DoWork;
            saveScheduleBg.RunWorkerAsync();
        }

        private void SaveScheduleBg_DoWork(object sender, DoWorkEventArgs e)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() =>
                {
                    var tscheduling = new TransferSchedule
                    {
                        Id = TransferScheduleId,
                        TransferEveryHours = TransferEveryHours,
                        CanTransferAfterNewContent = CanTransferAfterNewContent,
                        CanTransferEveryHours = CanTransferEveryHours
                    };
                    using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                    {
                        var data = JsonConvert.SerializeObject(tscheduling);
                        sc.EditTransferScheduling(EncryptionHelper.EncryptString(data, KeyEncryption));
                    }
                }));
        }

        private void OnGetTransferScheduling(object sender, DoWorkEventArgs args)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action) (() =>
                {
                    var transferSchedule = GetTransferSchedule();
                    if (transferSchedule != null)
                    {
                        CanTransferEveryHours = transferSchedule.CanTransferEveryHours;
                        CanTransferAfterNewContent = transferSchedule.CanTransferAfterNewContent;
                        TransferEveryHours = transferSchedule.TransferEveryHours;
                        TransferScheduleId = transferSchedule.Id;
                        ApplicationContext.TransferScheduleId = transferSchedule.Id;
                    }
                }));
        }

        private void Build()
        {
            var transferScheduleBg = new BackgroundWorkerHelper();
            transferScheduleBg.AddDoWork(OnGetTransferScheduling).DoWork();
            var count = 0;
            _view.PnlLeftUpdateSource.Children.Clear();
            _view.PnlRightUpdateSource.Children.Clear();
            var agentUpdataSource =
                ApplicationContext.UpdateSourceList.Where(
                    r => r.Id != ApplicationContext.UpdateSourceList.Select(u => u.Id).Min());
            foreach (var updateSource in agentUpdataSource)
            {
                var updSourceEle = new UpdateSourceSchedulingElement();
                updSourceEle.Model.Id = updateSource.Id;
                updSourceEle.Model.UpdateSourceName = string.IsNullOrWhiteSpace(updateSource.SourceName)
                    ? updateSource.SystemName
                    : updateSource.SourceName + " (" + updateSource.SystemName + ")";
                if (count%2 == 0)
                {
                    _view.PnlLeftUpdateSource.Children.Add(updSourceEle);
                }
                else
                {
                    _view.PnlRightUpdateSource.Children.Add(updSourceEle);
                }
                count++;
            }
            var updateSourceScheduleBg = new BackgroundWorkerHelper();
            updateSourceScheduleBg.AddDoWork(OnGetUpdateSourceScheduling).DoWork();
        }

        private void OnGetUpdateSourceScheduling(object sender, DoWorkEventArgs args)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action) (() =>
                {
                    var isSelected = false;
                    var updateSourceSchedule = GetUpdateSourceSchedule();
                    if (updateSourceSchedule != null)
                    {
                        var leftEle = _view.PnlLeftUpdateSource.Children;
                        var rightEle = _view.PnlRightUpdateSource.Children;
                       
                        foreach (var lelem in leftEle)
                        {
                            if (lelem.GetType() == typeof (UpdateSourceSchedulingElement))
                            {

                                var ussevm = (lelem as UpdateSourceSchedulingElement).Model;
                                var uss = updateSourceSchedule.Find(r => r.UpdateSourceId == ussevm.Id);
                                if (uss != null)
                                {
                                    ussevm.IsSelected = true;
                                    isSelected = true;
                                }
                            }
                        }
                        foreach (var relem in rightEle)
                        {
                            if (relem.GetType() == typeof (UpdateSourceSchedulingElement))
                            {
                                var ussevm = (relem as UpdateSourceSchedulingElement).Model;
                                var uss = updateSourceSchedule.Find(r => r.UpdateSourceId == ussevm.Id);
                                if (uss != null)
                                {
                                    ussevm.IsSelected = true;
                                    isSelected = true;
                                }
                            }
                        }
                    }
                    IsUpdateSourceNotChecked = !isSelected;
                }));
        }

        private TransferSchedule GetTransferSchedule()
        {
            try
            {
                var requestObj = new StringAuthenticateObject
                {
                    StringAuth = "OK"
                };
                var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<TransferSchedule>(
                    sc.GetTransferScheduling,
                    requestObj));
                return resultDeserialize;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return null;
            }
        }

        private List<UpdateSourceScheduling> GetUpdateSourceSchedule()
        {
            try
            {
                var requestObj = new StringAuthenticateObject
                {
                    StringAuth = "OK"
                };
                var resultDeserialize =
                    ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<UpdateSourceScheduling>>(
                        sc.GetAllUpdateSourceScheduling,
                        requestObj));
                return resultDeserialize;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return null;
            }
        }
    }
}