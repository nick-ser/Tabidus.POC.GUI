using System;
using System.Collections.Generic;
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
using Infragistics.Controls.Charts;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ViewModel.Task;

namespace Tabidus.POC.GUI.UserControls.Task
{
	/// <summary>
	/// Interaction logic for PieChartControl.xaml
	/// </summary>
	public partial class PieChartControl : UserControl
	{
		DispatcherTimer timer;
		public PieChartControl()
		{
			InitializeComponent();
			 timer = new DispatcherTimer();
		}

		private void TaskProgressPieChart_OnLoaded(object sender, RoutedEventArgs e)
		{
			TaskProgressPieChart.LabelsPosition = LabelsPosition.None;

			AutoRefresh(5000);
		}

		private void AutoRefresh(int interval)
		{
			timer.Interval = TimeSpan.FromMilliseconds(interval);
			timer.Tick += OnPieChartRefresh;
			timer.Start();
		}

		private void OnPieChartRefresh(object sender, EventArgs e)
		{
			try
			{
				timer.Stop();
				Model.Refresh();
			}
			catch (Exception ex)
			{
				Model.LogAutoRefreshError(ex);
			}
			finally
			{
				timer.Start();
			}
		}

		public PieChartViewModel Model
		{
			get
			{
				return DataContext as PieChartViewModel;
			}
		}
	}
}
