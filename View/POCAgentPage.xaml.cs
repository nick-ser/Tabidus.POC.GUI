using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Infragistics.DragDrop;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.LDAP;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls.Discovery;
using Tabidus.POC.GUI.ViewModel.Discovery;
using Tabidus.POC.GUI.ViewModel.Policy;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for EndPointListPage.xaml
    /// </summary>
    public partial class POCAgentPage : Page
    {
        // The CheckBoxes in the XamDataGrid execute this command when clicked.
        public static readonly string KeyEncryption = Functions.GetConfig("MESSAGE_KEY", "");

        public POCAgentPage()
        {
            InitializeComponent();
            Model = new POCAgentViewModel(this);
        }

        public POCAgentViewModel Model
        {
            get { return DataContext as POCAgentViewModel; }
            set { DataContext = value; }
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

        //private void Button_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    ((Button)sender).Background = (Brush)(new BrushConverter().ConvertFrom("#F00"));
        //}

        //private void Button_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    ((Button)sender).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0AFFFFFF"));
        //}
    }
}