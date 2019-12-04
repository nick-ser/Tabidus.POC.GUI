using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model.Task;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls.Task;

namespace Tabidus.POC.GUI.ViewModel.Task
{
	public class TaskItemViewModel : ViewModelBase
	{
		private bool _isInheritClick = false;
		public TaskItemViewModel(PocTask taskModel)
		{
			UpdateModel(taskModel);
			InitChildViewModel(this);
			InitCommand();
		}
		#region Command
		public ICommand EditTaskCommand { get; private set; }
		public ICommand DeleteTaskCommand { get; private set; }
		public ICommand InheritFromCommand { get; private set; }
		public ICommand EnabledCommand { get; private set; }
		public ICommand SaveCommand { get; private set; }
        public ICommand RunOnceCommand { get; private set; }
        public ICommand RunEveryMinsCommand { get; private set; }        

		private void InitCommand()
		{
			EditTaskCommand = new RelayCommand(ExecuteEditTask);
			DeleteTaskCommand = new RelayCommand(ExecuteDelete);
			InheritFromCommand = new RelayCommand(ExecuteInheritFrom);
			EnabledCommand = new RelayCommand(ExecuteEnabled);
            SaveCommand = new RelayCommand(ExecuteSave);
            RunOnceCommand = new RelayCommand(ExecuteSave);
            RunEveryMinsCommand = new RelayCommand(ExecuteSave);
		}

		private void ExecuteEditTask(object obj)
		{
			try
			{
				ServiceManager.Invoke(sc => RequestResponseUtils.GetData<DataResponse>(
				sc.AddTask,
				new PocTask
				{
					Id = Id,
					Name = Name
				}));
			}
			catch (Exception ex)
			{
				Logger.Error("Edit Task Error: " + ex.Message, ex);
			}
		}

		private void ExecuteDelete(object obj)
		{
			try
			{
				if (DialogHelper.Confirm("Are you sure you want to delete selected task?", "Confirm Delete Task"))
				{
					var dataRequest = new DataRequest(Id);
					ServiceManager.Invoke(sc => sc.DeleteTask(RequestResponseUtils.SendData(dataRequest)));

					var taskListViewModel = PageNavigatorHelper.GetMainContentViewModel<TaskListViewModel>();
					if (taskListViewModel != null)
					{
						taskListViewModel.RemoveTask(this);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Delete Task Error: " + ex.Message, ex);
			}
		}

		private void ExecuteInheritFrom(object obj)
		{
			_isInheritClick = Convert.ToBoolean(obj);
			SaveCommand.Execute(null);
		}

		private void ExecuteEnabled(object obj)
		{
			SaveCommand.Execute(null);
		}
		private void ExecuteSave(object obj)
		{
			//DialogHelper.ShowLoading();
			var taskModel = new PocTaskSyncData
			{
				Task = new PocTask
				{
					Id = Id,
					CanInheritFrom = CanInheritFrom,
					InheritFrom = InheritFrom,
					IsEnabled = IsEnabled,
					CanRunEveryMinutes = CanRunEveryMinutes,
					RunEveryMinutes = RunEveryMinutes ?? 0,
					CanRunOnce = CanRunOnce
				},
				SoftwareIds = GetSoftwareIds()
			};

			var backgroundWorker = new BackgroundWorkerHelper();
			backgroundWorker.AddDoWork(OnSave_Dowork).AddRunWorkerCompleted(OnSave_Completed).DoWork(taskModel);
		}

		private void OnSave_Completed(object sender, RunWorkerCompletedEventArgs e)
		{
			//DialogHelper.HideLoading();
			if (_isInheritClick)
			{
				_isInheritClick = false;
				var dataRequest = new TaskProgressRequest
				{
					TaskId = Id,
					DirectoryEndpointId = ApplicationContext.NodesSelected[0].NodeId,
					IsFolder = ApplicationContext.NodesSelected[0].IsFolder
				};
				var taskSoftwareInfo =
					ServiceManager.Invoke(sc => RequestResponseUtils.GetData<TaskDataResponse>(sc.GetTaskAndSoftwareInfo, dataRequest));
				if (taskSoftwareInfo != null)
				{
					//Update task model
					UpdateModel(taskSoftwareInfo.Result.PocTasks.FirstOrDefault());
					//Remove old software selected stated
					for (int i = ApplicationContext.TaskSoftwareSelectedList.Count - 1; i >= 0; i --)
					{
						var currentTask = ApplicationContext.TaskSoftwareSelectedList[i];
						if (currentTask.TaskId == Id)
						{
							ApplicationContext.TaskSoftwareSelectedList.RemoveAt(i);
						}
					}
					ApplicationContext.TaskSoftwareSelectedList.AddRange(taskSoftwareInfo.Result.SoftwareTasks);
					//Refresh software package list
					InstallationPackagesViewModel.Refresh();
				}
			}
		}

		private void OnSave_Dowork(object sender, DoWorkEventArgs e)
		{
			//Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)DialogHelper.ShowLoading);
			var taskModel = e.Argument as PocTaskSyncData;
			if (taskModel != null)
			{
				if (ApplicationContext.NodesSelected[0].IsFolder)
				{
					taskModel.DirectoryId = ApplicationContext.NodesSelected[0].NodeId;
					ServiceManager.Invoke(sc => sc.AssignTaskToDirectory(RequestResponseUtils.SendData(taskModel)));
				}
				else
				{
                    taskModel.EndPointId = ApplicationContext.NodesSelected[0].NodeId;
					ServiceManager.Invoke(sc => sc.AssignTaskToEndpoint(RequestResponseUtils.SendData(taskModel)));
				}
			}
		}

        #endregion
        #region Properties
        public string ExpanderBackgroundColor
        {
            //get { return IsActived ? "#284B51" : "#C6CCD8"; }
            get { return IsActived ? "#331dabed" : "#08FFFFFF"; }
        }

        public string TextColor
        {
            get { return IsActived ? "#FFFFFF" : "#8e8f93"; }
            //get { return IsActived ? "#D2D2D3" : "#525963"; }
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
		#endregion
		#region Properties Model
		private int _id;
		public int Id
		{
			get { return _id; }
			set { _id = value; OnPropertyChanged("Id"); }
		}
		private string _name;
		public string Name
		{
			get { return _name; }
			set { _name = value; OnPropertyChanged("Name"); }
		}
		private bool _canInheritFrom;
		public bool CanInheritFrom
		{
			get { return _canInheritFrom; }
			set { _canInheritFrom = value; OnPropertyChanged("CanInheritFrom"); }
		}
		private int _inheritFrom;
		public int InheritFrom
		{
			get { return _inheritFrom; }
			set { _inheritFrom = value; OnPropertyChanged("InheritFrom"); }
		}
		private string _inheritFromName;

		public string InheritFromName
		{
			get { return _inheritFromName; }
			set { _inheritFromName = value; OnPropertyChanged("InheritFromName"); }
		}

		private bool _isEnabled;
		public bool IsEnabled
		{
			get { return _isEnabled; }
			set { _isEnabled = value; OnPropertyChanged("IsEnabled"); }
		}
		private int? _runEveryMinutes;
		public int? RunEveryMinutes
		{
			get { return _runEveryMinutes; }
			set
			{
				_runEveryMinutes = value;
				OnPropertyChanged("RunEveryMinutes");
			}
		}
		private bool _canRunEveryMinutes;
		public bool CanRunEveryMinutes
		{
			get { return _canRunEveryMinutes; }
			set { _canRunEveryMinutes = value; OnPropertyChanged("CanRunEveryMinutes"); }
		}
		private bool _canRunOnce;
		public bool CanRunOnce
		{
			get { return _canRunOnce; }
			set { _canRunOnce = value; OnPropertyChanged("CanRunOnce"); }
		}
		#endregion
		#region Child ViewModel
		private InstallationPackagesViewModel _installationPackagesViewModel;

		public InstallationPackagesViewModel InstallationPackagesViewModel
		{
			get { return _installationPackagesViewModel; }
			set
			{
				_installationPackagesViewModel = value;
				OnPropertyChanged("InstallationPackagesViewModel");
			}
		}

		private PieChartViewModel _pieChartViewModel;

		public PieChartViewModel PieChartViewModel
		{
			get { return _pieChartViewModel; }
			set
			{
				_pieChartViewModel = value;
				OnPropertyChanged("PieChartViewModel");
			}
		}
		#endregion
		#region Private Function
		private void InitChildViewModel(TaskItemViewModel taskItemViewModel)
		{
			InstallationPackagesViewModel = new InstallationPackagesViewModel(taskItemViewModel);
			PieChartViewModel = new PieChartViewModel(taskItemViewModel);
		}
		private void UpdateModel(PocTask taskModel)
		{
			if(taskModel == null)
				return;
			Id = taskModel.Id;
			Name = taskModel.Name;
			CanInheritFrom = taskModel.CanInheritFrom;
			InheritFrom = taskModel.InheritFrom;
			InheritFromName = GetInheritFormName(InheritFrom);
			IsEnabled = taskModel.IsEnabled;
			RunEveryMinutes = taskModel.RunEveryMinutes;
			CanRunEveryMinutes = taskModel.CanRunEveryMinutes;
			CanRunOnce = taskModel.CanRunOnce;
		}
		private string GetInheritFormName(int inheritFrom)
		{
			var currentInheritForm = ApplicationContext.FolderListTree.FirstOrDefault(c => c.FolderId == inheritFrom);
			if (currentInheritForm != null)
				return currentInheritForm.FolderName;
			return string.Empty;
		}

		private string GetSoftwareIds()
		{
			var selectedSoftware = InstallationPackagesViewModel.SoftwareCollection.Where(c => c.IsSelected).Select(c => c.Id);
			return string.Join(",", selectedSoftware);
		}
		#endregion
		#region Public Function

		public void RefreshModel(PocTask model)
		{
			UpdateModel(model);
			InstallationPackagesViewModel.Refresh();
			PieChartViewModel.Refresh();
		}
		#endregion
	}
}
