using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Endpoint;

namespace Tabidus.POC.GUI.UserControls.Endpoint
{
    /// <summary>
    /// Interaction logic for ColorCodeControl.xaml
    /// </summary>
    public partial class ColorCodeControl : UserControl
    {
        private readonly Regex _regex = new Regex("[^0-9]+");
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorCodeControl"/> class.
        /// </summary>
        public ColorCodeControl(ColorModel colorModel)
        {
            InitializeComponent();
            Model = new ColorCodeViewModel(this, colorModel);
        }
        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <value>The model.</value>
        public ColorCodeViewModel Model
        {
            set
            {
                DataContext = value;
            }
        }

        /// <summary>
        /// Numbers the validation text box.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextCompositionEventArgs"/> instance containing the event data.</param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }
    }
}
