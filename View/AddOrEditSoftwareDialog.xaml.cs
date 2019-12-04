using System.Windows;
using Tabidus.POC.GUI.ViewModel.Software;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Infragistics.Windows;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for MoveLdapDialog.xaml
    /// </summary>
    public partial class AddOrEditSoftwareDialog
    {
        public AddOrEditSoftwareDialog()
        {
            InitializeComponent();
            Model = new AddOrEditSoftwareViewModel(this);
        }

        public AddOrEditSoftwareViewModel Model
        {
            get { return DataContext as AddOrEditSoftwareViewModel; }
            set { DataContext = value; }
        }

        /// <summary>
        ///     Closes the move dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonClose_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
          }
        public bool? DialogResult { get; set; }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AddOrEditSoftwareDialog_OnLoaded(object sender, RoutedEventArgs e)
            {
            Border header = Utilities.GetDescendantFromName(sender as DependencyObject, "HeaderArea") as Border;
            header.Visibility = System.Windows.Visibility.Visible;
            Grid borderGrid = Utilities.GetDescendantFromName(sender as DependencyObject, "BorderGrid") as Grid;
            borderGrid.Visibility = Visibility.Hidden;
            Grid parent = header.Parent as Grid;
            //parent.RowDefinitions[0].MaxHeight = 0;
            parent.RowDefinitions[0].MinHeight = 8;
            header.Opacity = 0;
            parent.RowDefinitions[0].Style = null;
            parent.RowDefinitions[0].OverridesDefaultStyle = true;
            foreach (var iChild in parent.Children)
            {
                if (iChild is Grid)
                    (iChild as Grid).Visibility = Visibility.Collapsed;
            }
            //Use the below loop if you wish to remove the small border that appears
            Grid content = Utilities.GetDescendantFromName(sender as DependencyObject, "ContentGrid") as Grid;
            foreach (var item in content.Children)
            {
                if (item is Border)
                    (item as Border).BorderThickness = new Thickness(0);
            }
        }
    }
}