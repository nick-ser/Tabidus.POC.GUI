using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tabidus.POC.GUI.ViewModel.Endpoint;
using Tabidus.POC.GUI.ViewModel.Task;

namespace Tabidus.POC.GUI.View
{
	/// <summary>
	/// Interaction logic for TaskListPage.xaml
	/// </summary>
	public partial class TaskListPage : Page
	{
		public TaskListPage()
		{
			InitializeComponent();
			TaskItemControl.Items.SortDescriptions.Add(
				new SortDescription("DataContext.Name", ListSortDirection.Ascending));
			Model = new TaskListViewModel(this);
			//GroupHeaderElement.UpdateHeader(2, ApplicationContext.FolderPathName, ApplicationContext.TotalEndpoint);
		}

		public TaskListViewModel Model
		{
			set { DataContext = value; }
		}
		//public GroupHeaderViewModel HeaderViewModel
		//{
		//	get { return GroupHeaderElement.Model; }
		//}

		public void UpdateHeader()
		{
			if(ApplicationContext.NodesSelected.Count == 0)
				return;
			//if (GroupHeaderElement.Visibility == Visibility.Visible)
			//{
			//	GroupHeaderElement.Model.UpdateDirectoryHeader(ApplicationContext.NodesSelected[0].NodeId, 2);
			//}
			//else
			//{
			//	EndpointHeaderElement.Dispatcher.BeginInvoke(DispatcherPriority.Render, (Action) (() =>
			//	{
			//		EndpointHeaderElement.Model.UpdateHeader(2);
			//	}));
			//}
		}
	}
}
