using System.Windows.Controls;
using Tabidus.POC.GUI.ViewModel.Software;

namespace Tabidus.POC.GUI.UserControls.Software
{
    /// <summary>
    ///     Interaction logic for UpdateSourceElement.xaml
    /// </summary>
    public partial class UpdateSourceSchedulingElement : UserControl
    {
        public UpdateSourceSchedulingElement()
        {
            InitializeComponent();
            Model = new UpdateSourceSchedulingViewModel(this);
        }

        public UpdateSourceSchedulingViewModel Model
        {
            get { return DataContext as UpdateSourceSchedulingViewModel; }
            set { DataContext = value; }
        }
    }
}