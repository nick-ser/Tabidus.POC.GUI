using System.Windows.Controls;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Label;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for LabelsPage.xaml
    /// </summary>
    public partial class LabelsPage : Page
    {
        public LabelsPage()
        {
            InitializeComponent();
            DataContext = new LabelViewModel(this);
        }
        
    }
}