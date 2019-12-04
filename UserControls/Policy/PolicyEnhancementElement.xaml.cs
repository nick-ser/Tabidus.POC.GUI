using System.Windows.Controls;
using Tabidus.POC.GUI.ViewModel.Policy;

namespace Tabidus.POC.GUI.UserControls.Policy
{
    /// <summary>
    ///     Label criteria user control
    /// </summary>
    public partial class PolicyEnhancementElement : UserControl
    {
        public PolicyEnhancementElement()
        {
            InitializeComponent();
            Model = new PolicyEnhancementViewModel(this);
        }

        public PolicyEnhancementViewModel Model
        {
            get { return DataContext as PolicyEnhancementViewModel; }
            set { DataContext = value; }
        }
    }
}