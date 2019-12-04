using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Discovery;

namespace Tabidus.POC.GUI.UserControls.Discovery
{
    /// <summary>
    /// Interaction logic for GroupHeaderElement.xaml
    /// </summary>
    public partial class NeighborhoodHeaderElement : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Endpoint.GroupHeaderElement"/> class.
        /// </summary>
        public NeighborhoodHeaderElement()
        {
            InitializeComponent();
        }
        
        private void BtnNeighborMenu_OnClick(object sender, RoutedEventArgs e)
        {
            var pnlchr = PnlNeighborBar.Children;
            foreach (var ch in pnlchr)
            {
                if (ch.GetType() == typeof(Button))
                {
                    ((Button)ch).BorderThickness = new Thickness(0);
                    ((Button)ch).Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8e8f98"));
                }
            }
            ((Button)sender).Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF"));
            // ((Button)sender).BorderThickness = new Thickness(0, 0, 0, 2); // Removed since the blue indicator is not required now
            // ((Button)sender).BorderBrush = (Brush)(new BrushConverter().ConvertFrom("#1dabed")); Removed since the blue indicator is not required now
            ((Button)sender).Foreground = Brushes.White;
        
    }
    }
}