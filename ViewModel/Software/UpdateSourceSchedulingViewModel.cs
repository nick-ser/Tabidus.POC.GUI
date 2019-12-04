using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.UserControls.Software;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Software
{
    public class UpdateSourceSchedulingViewModel : ViewModelBase
    {
        private bool _isSelected;

        private string _updateSourceName;
        private UpdateSourceSchedulingElement _view;

        public UpdateSourceSchedulingViewModel(UpdateSourceSchedulingElement view)
        {
            _view = view;
            ChangeUpdateSourceSchedulingCommand = new RelayCommand(OnChangeUpdateSourceSchedulingCommand);
        }

        public ICommand ChangeUpdateSourceSchedulingCommand { get; private set; }

        private void OnChangeUpdateSourceSchedulingCommand(object arg)
        {
            var changeUpdSourceBg = new BackgroundWorker();
            changeUpdSourceBg.DoWork += ChangeUpdSourceBg_DoWork;
            changeUpdSourceBg.RunWorkerAsync();
            var transferPage = PageNavigatorHelper.GetMainContent<TransferPage>();
            var mainScheduling = transferPage.MainSchedulingElement;
            var isNotSelectedUpdateSource = true;
            foreach (var updschedule in mainScheduling.PnlLeftUpdateSource.Children)
            {
                if (updschedule.GetType() == typeof (UpdateSourceSchedulingElement))
                {
                    var usse = updschedule as UpdateSourceSchedulingElement;
                    if (usse.Model.IsSelected)
                    {
                        isNotSelectedUpdateSource = false;
                        break;
                    }
                }
            }
            if (isNotSelectedUpdateSource)
            {
                foreach (var updschedule in mainScheduling.PnlRightUpdateSource.Children)
                {
                    if (updschedule.GetType() == typeof(UpdateSourceSchedulingElement))
                    {
                        var usse = updschedule as UpdateSourceSchedulingElement;
                        if (usse.Model.IsSelected)
                        {
                            isNotSelectedUpdateSource = false;
                            break;
                        }
                    }
                }
            }
            mainScheduling.Model.IsUpdateSourceNotChecked = isNotSelectedUpdateSource;

        }

        private void ChangeUpdSourceBg_DoWork(object sender, DoWorkEventArgs e)
        {
            
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (Action)(() =>
                {
                    var usschedule = new AddOrDeleteUpdateSourceScheduling
                    {
                        UpdateSourceId = Id,
                        SchedulingId = ApplicationContext.TransferScheduleId,
                        IsAdding = IsSelected
                    };
                    using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                    {
                        var data = JsonConvert.SerializeObject(usschedule);
                        sc.AddOrDeleteUpdateSourceScheduling(EncryptionHelper.EncryptString(data, KeyEncryption));
                    }
                }));
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public string UpdateSourceName
        {
            get { return _updateSourceName; }
            set
            {
                _updateSourceName = value;
                OnPropertyChanged("UpdateSourceName");
            }
        }

        public int Id { get; set; }
    }
}