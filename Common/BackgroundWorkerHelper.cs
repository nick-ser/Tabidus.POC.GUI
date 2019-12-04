using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tabidus.POC.GUI.Common
{
    public class BackgroundWorkerHelper
    {
        #region Private variable
        private bool _addedEventDoWork;
        private bool _addedEventRunWorkerCompleted;
        private readonly BackgroundWorker _worker;

        private DoWorkEventHandler _onDoWorkEventHandler;
        private ProgressChangedEventHandler _onReportProgressChanged;
        private RunWorkerCompletedEventHandler _onCompletedEventHandler;
        #endregion

        #region Constructors
        public BackgroundWorkerHelper() : this(new BackgroundWorker())
        {

        }
        public BackgroundWorkerHelper(BackgroundWorker worker)
        {
            _worker = worker;
        }
        #endregion

        #region Public functions
        public BackgroundWorkerHelper AddDoWork(DoWorkEventHandler onDoWorkEvent)
        {
            _addedEventDoWork = true;
            _onDoWorkEventHandler = onDoWorkEvent;
            _worker.DoWork += OnDoWork;
            return this;
        }

        public BackgroundWorkerHelper AddReportProgressChanged(ProgressChangedEventHandler onReportProgressChanged)
        {
            _worker.WorkerReportsProgress = true;
            _onReportProgressChanged = onReportProgressChanged;
            _worker.ProgressChanged += OnProgressChanged;
            return this;
        }

        public BackgroundWorkerHelper AddRunWorkerCompleted(RunWorkerCompletedEventHandler onRunWorkerCompleted)
        {
            _addedEventRunWorkerCompleted = true;
            _onCompletedEventHandler = onRunWorkerCompleted;
            return this;
        }

        public BackgroundWorkerHelper SupportCancellation()
        {
            _worker.WorkerSupportsCancellation = true;
            return this;
        }

        public void DoWork(object obj = null)
        {
            if (_addedEventDoWork)
            {
                _worker.RunWorkerCompleted += OnRunWorkerCompleted;
                _worker.RunWorkerAsync(obj);
            }
        }

        public void Cancel()
        {
            if (_worker.WorkerSupportsCancellation)
            {
                _worker.CancelAsync();
                //Release all event handler when cancel
                ReleaseEventHandler();
            }
        }

	    public bool IsBusy()
	    {
		    return _worker != null && _worker.IsBusy;
	    }
        #endregion

        #region Private functions
        private void OnDoWork(object sender, DoWorkEventArgs args)
        {
            _onDoWorkEventHandler(sender, args);
        }
        private void OnProgressChanged(object sender, ProgressChangedEventArgs args)
        {
            _onReportProgressChanged(sender, args);
        }
        private void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs args)
        {
            if (_onCompletedEventHandler != null)
            {
                _onCompletedEventHandler(sender, args);
            }
            //Release all registered event handler when completed
            ReleaseEventHandler();
        }
        private void ReleaseEventHandler()
        {
            if (_addedEventDoWork)
            {
                _worker.DoWork -= OnDoWork;
                _onDoWorkEventHandler = null;
                _addedEventDoWork = false;
            }
            if (_worker.WorkerReportsProgress)
            {
                _worker.ProgressChanged -= OnProgressChanged;
                _onReportProgressChanged = null;
                _worker.WorkerReportsProgress = false;
            }
            if (_addedEventRunWorkerCompleted)
            {
                _worker.RunWorkerCompleted -= OnRunWorkerCompleted;
                _onCompletedEventHandler = null;
                _addedEventRunWorkerCompleted = false;
            }
        }
        #endregion
    }
}