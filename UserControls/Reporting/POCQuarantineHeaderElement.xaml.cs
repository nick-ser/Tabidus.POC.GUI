using System.Windows.Controls;
using Tabidus.POC.GUI.ViewModel.Reporting;

namespace Tabidus.POC.GUI.UserControls.Reporting
{
    /// <summary>
    /// Interaction logic for POCQuarantineHeaderElement.xaml
    /// </summary>
    public partial class POCQuarantineHeaderElement : UserControl
    {
        private POCQuarantineHederViewModel _model;

        public POCQuarantineHeaderElement()
        {
            InitializeComponent();
            Model = new POCQuarantineHederViewModel(this);
        }

        public POCQuarantineHederViewModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                DataContext = _model;
            }
        }
    }
}
