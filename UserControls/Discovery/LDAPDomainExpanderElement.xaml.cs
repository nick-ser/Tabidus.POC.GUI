using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ViewModel.Discovery;

namespace Tabidus.POC.GUI.UserControls.Discovery
{
    /// <summary>
    ///     Interaction logic for LDAPDomainExpanderElement.xaml
    /// </summary>
    public partial class LDAPDomainExpanderElement : UserControl
    {
        private string _initPassword;
        public LDAPDomainExpanderElement()
        {
            InitializeComponent();
            Model = new LDAPDomainExpanderElementViewModel(this);
            _initPassword = Model.Password;
        }

        public LDAPDomainExpanderElementViewModel Model
        {
            get { return DataContext as LDAPDomainExpanderElementViewModel; }
            set { DataContext = value; }
        }

        private void BtnExpandHeader_OnClick(object sender, RoutedEventArgs e)
        {
            ActiveCurrentExpander();
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

        private void Expander_OnExpanded(object sender, RoutedEventArgs e)
        {
            ActiveCurrentExpander();
        }

        private void ActiveCurrentExpander()
        {
            var container = Parent as StackPanel;
            if (container != null)
            {
                foreach (var child in container.Children)
                {
                    if (child.GetType() == typeof(LDAPDomainExpanderElement))
                    {
                        var ldapElem = child as LDAPDomainExpanderElement;
                        ldapElem.Model.IsActived = false;
                    }
                }
            }
            Model.IsActived = true;
            Model.ShowLDAPDirectory.Execute(null);
        }
        
    }
}