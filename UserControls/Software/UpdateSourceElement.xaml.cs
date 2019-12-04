using System.Windows.Controls;
using Tabidus.POC.GUI.ViewModel.Software;

namespace Tabidus.POC.GUI.UserControls.Software
{
    /// <summary>
    ///     Interaction logic for UpdateSourceElement.xaml
    /// </summary>
    public partial class UpdateSourceElement : UserControl
    {
        public UpdateSourceElement()
        {
            InitializeComponent();
            Model = new UpdateSourceElementViewModel(this);
        }

        public UpdateSourceElementViewModel Model
        {
            get { return DataContext as UpdateSourceElementViewModel; }
            set { DataContext = value; }
        }
    }
}