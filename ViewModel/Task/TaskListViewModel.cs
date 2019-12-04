using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.Common.Model.Task;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls.Task;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.MainWindowView;

namespace Tabidus.POC.GUI.ViewModel.Task
{
	public class TaskListViewModel : PageViewModelBase
	{
		#region Private Variable
		private TaskListPage _page;
		private BackgroundWorkerHelper _bgHelper;
		private List<PocTask> _pocTasks;
		//private readonly MainWindowViewModel _mainViewModel;
		#endregion
		#region Constructor
		public TaskListViewModel(TaskListPage page)
		{
			_page = page;
			TaskItemElements = new ObservableCollection<TaskItemElement>();
			//_mainViewModel = PageNavigatorHelper.GetMainModel();

			AddTaskCommand = new RelayCommand(ExecuteAddTask);

			Refresh();
		}
		#endregion
		#region Command
		public ICommand AddTaskCommand { get; private set; }
		private void ExecuteAddTask(object obj)
		{
			try
			{
				//Add task item
				var newTaskModel = new PocTask
				{
					Name = "New Task"
				};
				//var request = new DataRequest();
				var dataResponse = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<DataResponse>(sc.AddTask, newTaskModel));
				if (dataResponse != null && dataResponse.Result > 0)
				{
					newTaskModel.Id = dataResponse.Result;
					TaskItemElements.Insert(0, CreateNewTaskItem(newTaskModel));
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Add Task Error: " + ex.Message, ex);
			}
		}
		#endregion
		#region Properties
		private ObservableCollection<TaskItemElement> _taskItemsElements;
		public ObservableCollection<TaskItemElement> TaskItemElements
		{
			get { return _taskItemsElements; }
			set
			{
				_taskItemsElements = value;
				OnPropertyChanged("TaskItemElements");
			}
		}

		public Visibility IsGroupView
		{
			get { return ApplicationContext.NodesSelected[0].IsFolder ? Visibility.Visible : Visibility.Collapsed; }
		}

		public Visibility IsEndpointView
		{
			get { return IsGroupView == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed; }
		}
		#endregion
		#region Override
		public override void Refresh()
		{
			//_mainViewModel.IsBusy = true;
			OnPropertyChanged("IsGroupView");
			OnPropertyChanged("IsEndpointView");
			if (_bgHelper != null && _bgHelper.IsBusy())
			{
				_bgHelper.Cancel();
			}
			_bgHelper = new BackgroundWorkerHelper().SupportCancellation();
			_bgHelper.AddDoWork(OnLoadTask_Dowork).AddRunWorkerCompleted(OnLoadTask_Completed).DoWork();
		}
		#endregion
		#region TaskLoading Worker
		private void OnLoadTask_Dowork(object sender, DoWorkEventArgs e)
		{
			try
			{
				//_page.Dispatcher.Invoke(delegate { TaskItemElements.Clear(); }, DispatcherPriority.Render);
				if (ApplicationContext.TaskSoftwareList == null)
				{
					var softwareRequest = new StringAuthenticateObject
					{
						StringAuth = "OK"
					};
					ApplicationContext.TaskSoftwareList = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<SoftwareContent>>(
							sc.GetAllSoftwareForTask, softwareRequest
						));
				}
				var softwareSelectedRequest = new SoftwarePackageDataRequest
				{
					Id = ApplicationContext.NodesSelected[0].NodeId,
					IsFolder = ApplicationContext.NodesSelected[0].IsFolder
				};
				var taskDataResponse =
					ServiceManager.Invoke(
						sc => RequestResponseUtils.GetData<TaskDataResponse>(sc.GetTaskAndSoftware, softwareSelectedRequest));

				_pocTasks = taskDataResponse.Result.PocTasks;
				ApplicationContext.TaskSoftwareSelectedList = taskDataResponse.Result.SoftwareTasks;
				if (ApplicationContext.TaskProgressDictionary == null)
					RefreshTaskProgressDictionary(taskDataResponse.Result.TaskProgressList);
			}
			catch (Exception ex)
			{
				Logger.Error("Load Task Error: " + ex.Message, ex);
			}
		}

		private void OnLoadTask_Completed(object sender, RunWorkerCompletedEventArgs e)
		{
			if (_pocTasks != null)
			{
				UpdateHeader();
				foreach (var pocTask in _pocTasks)
				{
					var currentElement = TaskItemElements.FirstOrDefault(c => c.Model.Id == pocTask.Id);
					if (currentElement != null)
					{
						currentElement.Model.RefreshModel(pocTask);
					}
					else
					{
						var taskItemViewModel = new TaskItemViewModel(pocTask);
						currentElement = new TaskItemElement(taskItemViewModel);
						TaskItemElements.Add(currentElement);
					}
				}
			}
			RefreshAllPieChart();
			InitPieChartTimer(1000);
			//_mainViewModel.IsBusy = false;
		}
		#endregion
		#region PieChart Timer

		DateTime _lastSync = DateTime.MinValue;
		DispatcherTimer _timer;
		private void InitPieChartTimer(int interval)
		{
			_timer = new DispatcherTimer();
			_timer.Interval = TimeSpan.FromMilliseconds(interval);
			_timer.Tick += OnPieChartRefresh;
			_timer.Start();
		}
		private void OnPieChartRefresh(object sender, EventArgs e)
		{
			try
			{
				_timer.Stop();
				var dataRequest = new StringAuthenticateObject("OK");
				var dataResponse =
					ServiceManager.Invoke(sc => RequestResponseUtils.GetData<LastUpdated>(sc.GetLastUpdateTaskProgress, dataRequest));
				if (dataResponse != null && dataResponse.LastSync > _lastSync)
				{
					//First load
					if (_lastSync == DateTime.MinValue)
					{
						_lastSync = dataResponse.LastSync;
					}
					else
					{
						_lastSync = dataResponse.LastSync;
						RefreshAllPieChart();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("OnPieChartRefresh Error", ex);
			}
			finally
			{
				_timer.Start();
			}
		}
		#endregion
		#region Public Function
		public void RemoveTask(TaskItemViewModel taskItemViewModel)
		{
			for (int i = TaskItemElements.Count - 1; i >= 0; i--)
			{
				if (TaskItemElements[i].Model == taskItemViewModel)
				{
					TaskItemElements.RemoveAt(i);
					for (int j = ApplicationContext.TaskSoftwareSelectedList.Count - 1; j >= 0; j--)
					{
						if (ApplicationContext.TaskSoftwareSelectedList[j].TaskId == taskItemViewModel.Id)
						{
							ApplicationContext.TaskSoftwareSelectedList.RemoveAt(j);
							break;
						}
					}
				}
			}
		}

		public void RefreshAllPieChart()
		{
			if (ApplicationContext.NodesSelected.Count == 0)
				return;
			var dataRequest = new SoftwarePackageDataRequest
			{
				Id = ApplicationContext.NodesSelected[0].NodeId,
				IsFolder = ApplicationContext.NodesSelected[0].IsFolder
			};
			var dataResponse =
				ServiceManager.Invoke(
					sc => RequestResponseUtils.GetData<TaskProgressDataResponse>(sc.GetAllTaskProgress, dataRequest));

			if (dataResponse != null)
			{
				RefreshTaskProgressDictionary(dataResponse.Result);
				foreach (TaskItemElement taskElement in TaskItemElements)
				{
					taskElement.Model.PieChartViewModel.Refresh();
				}
			}
		}
		#endregion
		#region Private Function
		private void RefreshTaskProgressDictionary(List<TaskProgressItem> taskProgressList)
		{
			int taskId = 0;
			ApplicationContext.TaskProgressDictionary = new ConcurrentDictionary<int, TaskProgressResult>();
			foreach (var taskProgressItem in taskProgressList)
			{
				if (taskProgressItem.TaskId != taskId)
				{
					taskId = taskProgressItem.TaskId;
					var taskProgressResult = new TaskProgressResult();
					taskProgressResult.Init();
					ApplicationContext.TaskProgressDictionary.AddOrUpdate(taskId, taskProgressResult,
						(key, oldVal) => taskProgressResult);
				}
				ApplicationContext.TaskProgressDictionary[taskId][(int)taskProgressItem.Type] = taskProgressItem;
			}
		}
		private TaskItemElement CreateNewTaskItem(PocTask taskModel)
		{
			var taskItemViewModel = new TaskItemViewModel(taskModel);
			taskItemViewModel.IsAddState = true;
			return new TaskItemElement(taskItemViewModel);
		}
		private void UpdateHeader()
		{
			//var headerViewModel = _page.HeaderViewModel;
			//if (headerViewModel != null && ApplicationContext.NodesSelected.Count > 0)
			//{
			//	headerViewModel.UpdateDirectoryHeader(ApplicationContext.NodesSelected[0].NodeId);
			//}
			_page.UpdateHeader();
		}
		#endregion
	}
}
