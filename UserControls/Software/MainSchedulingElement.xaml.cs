using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.GUI.ViewModel.Software;

namespace Tabidus.POC.GUI.UserControls.Software
{
    /// <summary>
    ///     Interaction logic for MainSchedulingElement.xaml
    /// </summary>
    public partial class MainSchedulingElement : UserControl
    {
        public MainSchedulingElement()
        {
            InitializeComponent();
            Model = new MainSchedulingViewModel(this);
        }

        public MainSchedulingViewModel Model
        {
            get { return DataContext as MainSchedulingViewModel; }
            set { DataContext = value; }
        }

        private void TxtTransferEveryHour_OnLostFocus(object sender, RoutedEventArgs e)
        {
            Model.ChangeScheduleCommand.Execute(null);
        }

        /// <summary>
        ///     Validation TextBox that only accept numberic input data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        /// <summary>
        ///     validation TextBox when we're pasting text into TextBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var txt = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(txt))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        /// <summary>
        ///     Using regular expression to validate input data
        /// </summary>
        /// <param name="text">Input data</param>
        /// <returns></returns>
        private static bool IsTextAllowed(string text)
        {
            var regex = new Regex("[^0-9]+");
            return !regex.IsMatch(text);
        }
    }
}