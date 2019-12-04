using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Infragistics;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model.Task;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Common;

namespace Tabidus.POC.GUI.ViewModel.Task
{
	public class PieChartViewModel : ViewModelBase
	{
		private Dictionary<TaskProgressType, Color> _dictColorMap = new Dictionary<TaskProgressType, Color>
		{
			{TaskProgressType.TaskStarted, Color.FromRgb(0, 76, 156) },
			{TaskProgressType.Downloading, Color.FromRgb(24, 123, 15) },
			{TaskProgressType.Installing, Color.FromRgb(0, 126, 128) },
			{TaskProgressType.TaskSuccessful, Color.FromRgb(64, 56, 147) },
			{TaskProgressType.Error, Color.FromRgb(132, 15, 11) },
		};
		private TaskItemViewModel _taskItemViewModel;
		public PieChartViewModel(TaskItemViewModel taskItemViewModel)
		{
			_taskItemViewModel = taskItemViewModel;
			InitData();
		}

		private ObservableCollection<TaskProgressItemView> _taskProgressCollection;

		public ObservableCollection<TaskProgressItemView> TaskProgressCollection
		{
			get { return _taskProgressCollection; }
			set
			{
				_taskProgressCollection = value;
				OnPropertyChanged("TaskProgressCollection");
			}
		}

		private BrushCollection _colorBrushCollection;
		public BrushCollection ColorBrushCollection
		{
			get
			{
				return _colorBrushCollection;
			}
			set
			{
				_colorBrushCollection = value;
				OnPropertyChanged("ColorBrushCollection");
			}
		}

		public void Refresh()
		{
			InitData();
		}

		private void InitData()
		{
			TaskProgressResult currentTaskProgress;
			if (ApplicationContext.TaskProgressDictionary.ContainsKey(_taskItemViewModel.Id))
			{
				currentTaskProgress = ApplicationContext.TaskProgressDictionary[_taskItemViewModel.Id];
			}
			else
			{
				currentTaskProgress = new TaskProgressResult();
				currentTaskProgress.Init();
			}
			if (TaskProgressCollection != null)
				TaskProgressCollection = null;
			TaskProgressCollection = new ObservableCollection<TaskProgressItemView>();
			ColorBrushCollection = new BrushCollection();

			foreach (var task in currentTaskProgress)
			{
				var solidColor = new SolidColorBrush(_dictColorMap[task.Type]);
				var taskView = new TaskProgressItemView(task)
				{
					SolidColor = solidColor
				};
				if (task.TaskCount > 0)
					ColorBrushCollection.Add(solidColor);
				TaskProgressCollection.Add(taskView);
			}
		}

		public void LogAutoRefreshError(Exception ex)
		{
			Logger.Error("LogAutoRefresh Error: " + ex.Message, ex);
		}
	}

	public class TaskProgressItemView
	{
		public TaskProgressItemView(TaskProgressItem taskItem)
		{
			Type = taskItem.Type;
			TaskCount = taskItem.TaskCount;
		}
		public TaskProgressType Type { get; set; }
		public int TaskCount { get; set; }
		public string TaskName
		{
			get { return string.Format("{0} ({1})", CommonConstants.DictTaskTypeNameMapping[Type], TaskCount); }
		}
		public SolidColorBrush SolidColor { get; set; }
	}
}
