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

namespace Tabidus.POC.GUI.UserControls.Task
{
	/// <summary>
	/// Interaction logic for InstallationPackagesControl.xaml
	/// </summary>
	public partial class InstallationPackagesControl : UserControl
	{
		public InstallationPackagesControl()
		{
			InitializeComponent();
		}


		public bool CanEdit
		{
			get { return (bool)GetValue(CanEditProperty); }
			set { SetValue(CanEditProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CanEdit.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CanEditProperty =
			DependencyProperty.Register("CanEdit", typeof(bool), typeof(InstallationPackagesControl), new PropertyMetadata(false));


	}
}
