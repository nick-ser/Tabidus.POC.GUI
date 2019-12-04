using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel;

namespace Tabidus.POC.GUI.UserControls
{
    /// <summary>
    ///     Interaction logic for PaginationElement.xaml
    /// </summary>
    public partial class PaginationElement : UserControl
    {
        public PaginationElement()
        {
            InitializeComponent();
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
            if (e.DataObject.GetDataPresent(typeof (string)))
            {
                var txt = (string) e.DataObject.GetData(typeof (string));
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

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
//            if (e.Key == Key.Enter)
//            {
//                var textBox = (TextBox) sender;
//                var page = Convert.ToInt32(string.IsNullOrEmpty(textBox.Text)?"1": textBox.Text);
//                var parentPage = this.TryFindParent<EndPointListPage>();
//                if (parentPage != null)
//                {
//                    var mainWindowViewModel = parentPage.DataContext as GroupViewModel;
//                    mainWindowViewModel.ShowSpecifyPage(page);
//                }
//            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
//            var cbbox = (ComboBox) sender;
//            var parentPage = this.TryFindParent<EndPointListPage>();
//            if (parentPage != null)
//            {
//                var mainWindowViewModel = parentPage.DataContext as GroupViewModel;
//                mainWindowViewModel.ShowDataWithRows((int)cbbox.SelectedItem);
//            }
        }
    }
}