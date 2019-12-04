using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls.Software;
using Tabidus.POC.GUI.View;
using Timer = System.Timers.Timer;

namespace Tabidus.POC.GUI.ViewModel.Software
{
    public class TransferViewModel : PageViewModelBase
    {
        private readonly TransferPage _view;

        private string _sourceName;

        private List<UpdateSource> _viewList;
	    private List<BackgroundWorker> _backgroundWorkers;

        public TransferViewModel(TransferPage view)
        {
            _view = view;
            ApplicationContext.UpdateSourceElementViewModelsSelected = new List<UpdateSourceElementViewModel>();
            ViewList = new List<UpdateSource>();
            TabSelectedCommand = new RelayCommand<Button>(OnMenuSelected, CanMenuAction);
            BuidPage();
        }

        public ICommand TabSelectedCommand { get; private set; }

        public List<UpdateSource> ViewList
        {
            get { return _viewList; }
            set
            {
                _viewList = value;
                OnPropertyChanged("ViewList");
            }
        }

        public string SourceName
        {
            get { return _sourceName; }
            set
            {
                _sourceName = value;
                OnPropertyChanged("SourceName");
            }
        }

        public override void Refresh()
        {
            BuidPage();
        }

        public void BuidPage()
        {
            _view.PnlUpdateSourceContainer.Children.Clear();
            foreach (
                var uds in
                    ApplicationContext.UpdateSourceList.Where(
                        r => r.ParentId == ApplicationContext.UpdateSourceList.Select(u => u.Id).Min()))
            {
                var usele = new UpdateSourceElement();
                usele.Model.Id = uds.Id;
                usele.Model.UpdateSourceUrl = uds.SourceUrl;
                usele.Model.UpdateSourceName = string.IsNullOrWhiteSpace(uds.SourceName)
                    ? uds.SystemName
                    : uds.SourceName + " (" + uds.SystemName + ")";
                _view.PnlUpdateSourceContainer.Children.Add(usele);
                var clist = ApplicationContext.UpdateSourceList.Where(r => r.ParentId == uds.Id).ToList();
                BuidUpdateSource(clist, usele);
            }
        }

        private void BuidUpdateSource(List<UpdateSource> ulist, UpdateSourceElement updateSourceElement)
        {
            updateSourceElement.PnlChildren.Children.Clear();
            foreach (var uds in ulist)
            {
                var usele = new UpdateSourceElement();
                usele.Model.Id = uds.Id;
                usele.Model.UpdateSourceUrl = uds.SourceUrl;
                usele.Model.UpdateSourceName = string.IsNullOrWhiteSpace(uds.SourceName)
                    ? uds.SystemName
                    : uds.SourceName + " (" + uds.SystemName + ")";
                updateSourceElement.PnlChildren.Children.Add(usele);
                var clist = ApplicationContext.UpdateSourceList.Where(r => r.ParentId == uds.Id).ToList();
                BuidUpdateSource(clist, usele);
            }
        }

        private void OnMenuSelected(Button btn)
        {
            if (btn == null)
                return;

            switch (btn.Name)
            {
                case UIConstant.BtnSoftwareTransferNow:
                    TransferNow();
                    break;
            }
        }

        private bool CanMenuAction(Button btn)
        {
            if (btn == null)
                return false;
            switch (btn.Name)
            {
                case UIConstant.BtnSoftwareTransferNow:
                    return ApplicationContext.UpdateSourceElementViewModelsSelected.Any(r => r.IsSelected);
            }
            return true;
        }

        private void TransferNow()
        {
			ErrorMessageList = new List<string>();
			//Hide all progress bars
			foreach (var usvm in ApplicationContext.UpdateSourceElementViewModelsSelected)
            {
                usvm.ProgressVisible = false;
            }
            //transfer for all selected Agent Update Source
            var usvmSelectedList =
                ApplicationContext.UpdateSourceElementViewModelsSelected.Where(r => r.IsSelected).ToList();
			_backgroundWorkers = new List<BackgroundWorker>();

			foreach (var usvm in usvmSelectedList)
            {
                var transferBg = new BackgroundWorker();
                transferBg.DoWork += TransferNow_OnDoWork;
                transferBg.RunWorkerCompleted += TransferNow_Completed;
				_backgroundWorkers.Add(transferBg);
				transferBg.RunWorkerAsync(usvm);
            }
        }

	    private void TransferNow_Completed(object sender, RunWorkerCompletedEventArgs e)
	    {
		    var usvm = e.Result as UpdateSourceElementViewModel;
		    if (usvm != null)
		    {
			    usvm.CurrentSize = usvm.TotalSize;
				usvm.ProgressVisible = false;
			}
			var bgCurrent = sender as BackgroundWorker;
			if (bgCurrent != null)
			{
				bgCurrent.DoWork -= TransferNow_OnDoWork;
				bgCurrent.RunWorkerCompleted -= TransferNow_Completed;
				_backgroundWorkers.Remove(bgCurrent);
				bgCurrent.Dispose();
				if (_backgroundWorkers.Count == 0)
				{
					//Show error message
					if (ErrorMessageList.Count > 0)
					{
						var errors = string.Join(Environment.NewLine, ErrorMessageList);
						DialogHelper.Error(errors, "Transfer Now Error");
					}
				}
			}
		}

	    private void TransferNow_OnDoWork(object sender, DoWorkEventArgs e)
        {
			var usvm = e.Argument as UpdateSourceElementViewModel;
            if (usvm != null)
            {
                usvm.CurrentSize = 0;
                var totalSize =
                    ServiceManager.TransferToServerAgent(new TransferToAgentDataRequest
                    {
                        SourceUrl = usvm.UpdateSourceUrl,
                        UpdateSourceId = usvm.Id
                    });
                usvm.TotalSize = (int) totalSize;
                usvm.ProgressVisible = true;
                var aTimer = new Timer();
                aTimer.Elapsed += delegate
                {
                    if (usvm.CurrentSize < usvm.TotalSize - CalculateSizeOnTenPercent(usvm.TotalSize))
                        usvm.CurrentSize += 1;
                };
                aTimer.Interval = 1000;
                aTimer.Enabled = true;

                var timeOut = 60*10000; //10 minutes
                while (true)
                {
                    var dataRequest = new StringAuthenticateObject
                    {
                        StringAuth = "OK",
                        StringValue = usvm.Id.ToString()
                    };
                    var dataResponse =
                        ServiceManager.Invoke(
                            sc => RequestResponseUtils.GetData<TransferDataResponse>(sc.GetTransferStatus, dataRequest));
					//Break when unable execute transfer to Agent
	                if (dataResponse.FileSize < 0)
	                {
		                ErrorMessageList.AddRange(dataResponse.ErrorMessage.Select(c=>string.Format("[{0}]: {1}", usvm.UpdateSourceName, c)));

		                usvm.CurrentSize = 0;
		                usvm.ProgressVisible = false;
						aTimer.Enabled = false;
						aTimer.Stop();
		                e.Result = null;
						break;
					}
                    if (timeOut == 0 || totalSize == dataResponse.FileSize)
                    {
                        //Timeout or transfer complete
                        aTimer.Enabled = false;
                        aTimer.Stop();
	                    e.Result = usvm;
						break;
                    }
					Thread.Sleep(1000);
					timeOut -= 1000;
				}
			}
        }

        private int CalculateSizeOnTenPercent(int totalSize)
        {
            return totalSize/10 < 1 ? 1 : totalSize/10;
        }

		private List<string> _errorMessageList;
		public List<string> ErrorMessageList
		{
			get { return _errorMessageList; }
			set
			{
				_errorMessageList = value;
				OnPropertyChanged("ErrorMessageList");
			}
		}
	}
}