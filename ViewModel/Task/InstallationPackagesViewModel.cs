using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.Common.Model.Task;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;

namespace Tabidus.POC.GUI.ViewModel.Task
{
	public class InstallationPackagesViewModel : ViewModelBase
	{
		private TaskItemViewModel _taskItemViewModel;
		public InstallationPackagesViewModel(TaskItemViewModel taskItemViewModel)
		{
			_taskItemViewModel = taskItemViewModel;
			SoftwareCollection = new ObservableCollection<SoftwareContentView>(GetSoftwareCollection());

			SoftwareWithTaskCommand = new RelayCommand(ExecuteSoftwareWithTask);
		}

		private void ExecuteSoftwareWithTask(object obj)
		{
			_taskItemViewModel.SaveCommand.Execute(null);
		}

		#region Command
		public ICommand SoftwareWithTaskCommand { get; private set; }
		#endregion
		#region Properties
		private ObservableCollection<SoftwareContentView> _softwareCollection;

		public ObservableCollection<SoftwareContentView> SoftwareCollection
		{
			get { return _softwareCollection; }
			set
			{
				_softwareCollection = value;
				OnPropertyChanged("SoftwareCollection");
			}
		}
		#endregion
		#region Private Function
		private IEnumerable<SoftwareContentView> GetSoftwareCollection()
		{
			var softwareSelected = ApplicationContext.TaskSoftwareSelectedList
									.Where(c => c.TaskId == _taskItemViewModel.Id)
									.Select(c=>c.SoftwareId);
			
			return ApplicationContext.TaskSoftwareList.Select(c =>
			{
				var softwareContent = new SoftwareContentView(c);
				if (softwareSelected.Contains(softwareContent.Id))
				{
					softwareContent.IsSelected = true;
				}
				return softwareContent;
			});
		}
		#endregion
		#region Public Function

		public void Refresh()
		{
			SoftwareCollection = new ObservableCollection<SoftwareContentView>(GetSoftwareCollection());
		}
		#endregion
	}
}
