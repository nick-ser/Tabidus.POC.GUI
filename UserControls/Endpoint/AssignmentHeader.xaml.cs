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

namespace Tabidus.POC.GUI.UserControls.Endpoint
{
    /// <summary>
    /// Interaction logic for AssignmentHeader.xaml
    /// </summary>
    public partial class AssignmentHeader : UserControl
    {
        public AssignmentHeader()
        {
            InitializeComponent();
        }
        private void OnDirectoryAssignmentClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
        }
    }
}
