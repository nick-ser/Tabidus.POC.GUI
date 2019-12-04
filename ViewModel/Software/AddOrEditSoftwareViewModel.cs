using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Software
{
    public class AddOrEditSoftwareViewModel : ViewModelBase
    {
        private AddOrEditSoftwareDialog _view;
        public string _oriFileName;
        public string _oriVersion;
        private MessageDialog _messageDialog = PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
        public AddOrEditSoftwareViewModel(AddOrEditSoftwareDialog view)
        {
            _view = view;
            SaveCommand = new RelayCommand(OnSaveCommand, CanSaveCommand);
            BrowseFileCommand = new RelayCommand(OnBrowseFileCommand);
        }

        public ICommand SaveCommand { get; private set; }
        public ICommand BrowseFileCommand { get; private set; }

        private bool CanSaveCommand(object arg)
        {
            return ValidateData();
        }
        private string _addOrEditText;

        public string AddOrEditText
        {
            get { return _addOrEditText; }
            set
            {
                _addOrEditText = value;
                OnPropertyChanged("AddOrEditText");
            }
        }

        private void UploadSoftware()
        {
            var mainVm = PageNavigatorHelper.GetMainModel();
            mainVm.ShowMessage("Uploading...");
            var uploadBg = new BackgroundWorkerHelper();
            uploadBg.AddDoWork(SaveBackgroundWorker_DoWork).AddRunWorkerCompleted(OnSaveData_RunWorkerCompleted).DoWork();
        }
        private string _filePath;

        private void OnSaveData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
			var mainVm = PageNavigatorHelper.GetMainModel();
			if (e.Result != null && (bool) e.Result == false)
			{
				mainVm.HideMessage();
				return;
			}
            _view.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action)(() =>
            {
                var smv = PageNavigatorHelper.GetMainContentViewModel<SoftwareViewModel>();
                if (smv != null)
                {
                    smv.Refresh();
                }
                _view.Close();
                
                mainVm.HideMessage();
					//Auto transfer
					AutoTransferNewContent();
				}));
            
        }

		private void AutoTransferNewContent()
		{
			var bgHelper = new BackgroundWorkerHelper();
			bgHelper.AddDoWork((o, args) =>
			{
				var dataRequest = new NewSoftwareTransferRequest
				{
					Id = Id,
					Name = Name,
					FileName = Executable,
					Params = Parameters,
					Checksum = Checksum
				};

				ServiceManager.Invoke(
					sc => RequestResponseUtils.GetData<DataResponse>(sc.NewSoftwareTransfer, dataRequest));
			}).DoWork();
		}
        private void OnSaveCommand(object arg)
        {
            if (ValidateData())
            {
                var regex = new Regex("^([^/\\\\|*<>:\"?]*)$");
                if (!regex.IsMatch(Name))
                {
                    _messageDialog.ShowMessageDialog("The Name of Software can't contain any of the following characters: (/\\|*<>:\"?)", "Message");
                    return;
                }
                if (IsAdding)
                {
                    if (ApplicationContext.SoftwareList.Select(r => r.Name).Contains(Name))
                    {
                        _messageDialog.ShowMessageDialog("The Name of Software is existed", "Message");
                        return;
                    }
                }
                UploadSoftware();
            }
            else
            {
                _messageDialog.ShowMessageDialog("Please enter required values", "Message");
            }
        }
        private void SaveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _view.Dispatcher.Invoke(DispatcherPriority.Render, (Action)(() =>
            {
					var dataResponse = new DataResponse(true, 1);
                if (IsAdding)
                {
					Checksum = FileUtils.GetChecksum(_filePath);
					dataResponse = ServiceManager.Upload(new FileUploadRequest
                    {
                        Name = Name,
                        SecurityKey = RequestResponseUtils.EncryptString(
                            AppSettings.GetConfig<string>(CommonConstants.MESSAGE_KEY)),
                        VirtualPath = _filePath
                    }, Checksum);
				}
                else
                {
                    if (_oriFileName != Executable)
                    {
						Checksum = FileUtils.GetChecksum(_filePath);
						//upload another file
						ServiceManager.DeleteSoftwareFile(new List<string> { Name });
							   dataResponse = ServiceManager.Upload(new FileUploadRequest
                        {
                            Name = Name,
                            SecurityKey = RequestResponseUtils.EncryptString(
                            AppSettings.GetConfig<string>(CommonConstants.MESSAGE_KEY)),
                            VirtualPath = _filePath
                        }, Checksum);
                    }
                }

	            if (!dataResponse.Status)
	            {
		            DialogHelper.Error(dataResponse.Message, "Error on Upload file");
		            e.Result = false;
						return;
	            }

                var scontent = new SoftwareContent
                {
                    Id = Id,
                    Comment = Comment,
                    FileName = Executable,
                    LastUpdated = DateTime.Now,
                    Name = Name,
                    Parameters = Parameters,
                    Size = Size,
                    Version = Version,
						  Checksum = Checksum
					 };
                var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<DataResponse>(
                    sc.SoftwareInsertOrUpdate,
                    scontent));
	            Id = resultDeserialize.Result;
	            e.Result = resultDeserialize.Status;

            }));
        }
        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Version) ||
                string.IsNullOrWhiteSpace(Executable) || string.IsNullOrWhiteSpace(Parameters))
            {
                return false;
            }
            return true;
        }
        private void OnBrowseFileCommand(object arg)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Applications (*.exe;*.msi)|*.exe;*.msi";
            if (openFileDialog.ShowDialog() == true)
            {
                _filePath = openFileDialog.FileName;
                FileInfo f = new FileInfo(openFileDialog.FileName);
                Executable = f.Name;
                Parameters = f.Name;
                Size = Math.Round((double)f.Length/(1024*1024), 2);
            }
        }
        public bool IsAdding { get; set; }
        public int Id { get; set; }
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

        private string _version;

        public string Version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged("Version");
            }
        }

        private string _comment;

        public string Comment
        {
            get { return _comment; }
            set
            {
                _comment = value;
                OnPropertyChanged("Comment");
            }
        }

        private string _parameters;

        public string Parameters
        {
            get { return _parameters; }
            set
            {
                _parameters = value;
                OnPropertyChanged("Parameters");
            }
        }

        private string _executable;

        public string Executable
        {
            get { return _executable; }
            set
            {
                _executable = value;
                OnPropertyChanged("Executable");
            }
        }

        private double _size;

        public double Size
        {
            get { return _size; }
            set
            {
                _size = value;
                OnPropertyChanged("Size");
            }
        }

	     public string Checksum { get; set; }
    }
}
