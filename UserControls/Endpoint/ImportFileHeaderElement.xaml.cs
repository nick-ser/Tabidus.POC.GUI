using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tabidus.POC.GUI.UserControls.Endpoint
{
    /// <summary>
    /// Interaction logic for ImportFileHeaderElement.xaml
    /// </summary>
    public partial class ImportFileHeaderElement : UserControl
    {
        public ImportFileHeaderElement()
        {
            InitializeComponent();
            btnsimulate.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF"));
            btnsimulate.BorderThickness = new Thickness(0, 0, 0, 3);
            btnsimulate.BorderBrush = (Brush)(new BrushConverter().ConvertFrom("#1dabed"));
        }
        private void BtnBase_OnClick(object sender, RoutedEventArgs e)
        {
            var pnlchr = PnlMenuBar.Children;
            foreach (var ch in pnlchr)
            {
                if (ch.GetType() == typeof(Button))
                {
                    ((Button)ch).BorderThickness = new Thickness(0);
                    ((Button)ch).Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8e8f98"));
                }
            }
            ((Button)sender).Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF"));
            ((Button)sender).BorderThickness = new Thickness(0, 0, 0, 4);
            ((Button)sender).BorderBrush = (Brush)(new BrushConverter().ConvertFrom("#1dabed"));
            ((Button)sender).Foreground = Brushes.White;
        }
    }
}
