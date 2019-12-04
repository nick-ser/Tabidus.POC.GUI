using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Tabidus.POC.Common.Model.Task;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ViewModel.Task;

namespace Tabidus.POC.GUI.UserControls.Task
{
	/// <summary>
	/// Interaction logic for TaskItemElement.xaml
	/// </summary>
	public partial class TaskItemElement : UserControl
	{
		private readonly Regex _regex = new Regex("[^0-9]+");
		private string _headerExpanderOrg;
		public TaskItemElement()
		{
			InitializeComponent();
			Loaded += TaskItemElement_Loaded;
		}
		
		public TaskItemElement(TaskItemViewModel model) : this()
		{
			Model = model;
		}
		public TaskItemViewModel Model
		{
			get { return DataContext as TaskItemViewModel; }
			set { DataContext = value; }
		}
		#region Header Event
		private void TaskItemElement_Loaded(object sender, RoutedEventArgs e)
		{
			var btnHeader = Expander.FindChild<Button>("BtnExpandHeader");
			var tbHeader = Expander.FindChild<TextBox>("TbExpandHeader");
			if (btnHeader != null)
			{
				btnHeader.MouseDoubleClick += BtnHeader_MouseDoubleClick;
			}
			if (tbHeader != null)
			{
				_headerExpanderOrg = tbHeader.Text;
				tbHeader.LostFocus += TbHeader_LostFocus;
				tbHeader.KeyDown += TbHeader_KeyDown;
				if (tbHeader.IsVisible)
				{
					tbHeader.Focus();
					tbHeader.SelectAll();
				}
			}
		}
		private void BtnHeader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			((Button)sender).Visibility = Visibility.Hidden;
			var tbHeader = Expander.FindChild<TextBox>("TbExpandHeader");
			if (tbHeader != null)
			{
				tbHeader.Visibility = Visibility.Visible;
				tbHeader.Focus();
			}
		}
		private void TbHeader_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				UpdateTask(sender as TextBox);
			}
		}

		private void TbHeader_LostFocus(object sender, RoutedEventArgs e)
		{
			UpdateTask(sender as TextBox);
		}

		private void UpdateTask(TextBox txtHeader)
		{
			string headerText = txtHeader.Text;
			if (string.IsNullOrWhiteSpace(headerText))
			{
				Model.Name = _headerExpanderOrg;
				DialogHelper.Error("Task name cannot be empty", "Message");
			}
			else if (headerText.Length > 200)
			{
				Model.Name = _headerExpanderOrg;
				DialogHelper.Error("Task name cannot more than 200 letters", "Message");
			}
			else
			{
				Model.Name = _headerExpanderOrg = headerText;
				//Edit Task
				Model.EditTaskCommand.Execute(null);

				txtHeader.Visibility = Visibility.Hidden;
				var btnHeader = Expander.FindChild<Button>("BtnExpandHeader");
				if (btnHeader != null)
				{
					btnHeader.Content = _headerExpanderOrg;
					btnHeader.Visibility = Visibility.Visible;
				}
				var viewItemSource = this.TryFindParent<ItemsControl>().ItemsSource;
				CollectionViewSource.GetDefaultView(viewItemSource).Refresh();
				BringIntoView();
			}
		}
		private void Expander_OnExpanded(object sender, RoutedEventArgs e)
		{
			ActiveCurrentExpander();
		}

		private void ActiveCurrentExpander()
		{
			var taskListViewModel = PageNavigatorHelper.GetMainContentViewModel<TaskListViewModel>();
			if (taskListViewModel != null)
			{
				foreach (var taskItemElement in taskListViewModel.TaskItemElements)
				{
					taskItemElement.Model.IsActived = false;
				}

			}
			Model.IsActived = true;
		}
		#endregion

		private void RunEveryMinutes_OnLostFocus(object sender, RoutedEventArgs e)
		{
			Model.SaveCommand.Execute(null);
		}

		private void RunEveryMinutes_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
		{
			e.Handled = _regex.IsMatch(e.Text);
		}

		private void UIElement_OnPreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Space)
			{
				e.Handled = true;
			}
		}
	}
}
