using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Label;
using Label = Tabidus.POC.Common.Model.Endpoint.Label;

namespace Tabidus.POC.GUI.UserControls.Label
{
    /// <summary>
    ///     Label criteria user control
    /// </summary>
    public partial class LabelCriteriaElement : UserControl
    {
        private string _headerExpanderOrg;
        public LabelCriteriaElement()
        {
            InitializeComponent();
            //DataContext = new LabelCriteriaViewModel(this);
            Model = new LabelCriteriaViewModel(this);
            Loaded += LabelCriteriaElement_Loaded;
        }

        public LabelCriteriaViewModel Model
        {
            get { return DataContext as LabelCriteriaViewModel; }
            set { DataContext = value; }
        }

        private void LabelCriteriaElement_Loaded(object sender, RoutedEventArgs e)
        {
            var btnHeader = Expander.FindChild<Button>("BtnExpandHeader");

            var tbHeader = Expander.FindChild<TextBox>("TbExpandHeader");
            if (btnHeader != null)
            {
                btnHeader.MouseDoubleClick += BtnHeader_MouseDoubleClick;
            }
            if (tbHeader != null)
            {
                _headerExpanderOrg = tbHeader.Text;
                tbHeader.LostFocus += TbHeader_LostFocus;
                tbHeader.KeyDown += TbHeader_KeyDown;
                if (tbHeader.IsVisible)
                {
                    tbHeader.Focus();
                    tbHeader.SelectAll();
                }
            }
        }

        private void TbHeader_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var tb = (TextBox)sender;
                var btnHeader = Expander.FindChild<Button>("BtnExpandHeader");
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = _headerExpanderOrg;
                    var messageDialog =
                        PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.ShowMessageDialog("Label name cannot be empty", "Message");
                }
                else
                {
                    UpdateLabel(tb.Text);
                }
                tb.Visibility = Visibility.Hidden;

                if (btnHeader != null)
                {
                    btnHeader.Content = _headerExpanderOrg;
                    btnHeader.Visibility = Visibility.Visible;
                }

            }
        }

        private void TbHeader_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = (TextBox)sender;
            var btnHeader = Expander.FindChild<Button>("BtnExpandHeader");
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = _headerExpanderOrg;
                var messageDialog =
                    PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                messageDialog.ShowMessageDialog("Label name cannot be empty", "Message");
            }
            else
            {
                UpdateLabel(tb.Text);
            }

            tb.Visibility = Visibility.Hidden;

            if (btnHeader != null)
            {
                btnHeader.Content = _headerExpanderOrg;
                btnHeader.Visibility = Visibility.Visible;
            }
        }

        private void UpdateLabel(string name)
        {
            if (name.Length > 200)
            {
                var messageDialog =
                    PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                messageDialog.ShowMessageDialog("Label name cannot more than 200 letters", "Message");
                return;
            }
            var viewModel = DataContext as LabelCriteriaViewModel;
            var label = new POC.Common.Model.Endpoint.Label
            {
                LabelId = viewModel.LabelId,
                LabelName = name
            };
            var labelsPage = PageNavigatorHelper.GetMainContent<LabelsPage>();
            if (labelsPage != null)
            {
                var labelViewModel = labelsPage.DataContext as LabelViewModel;
                if (labelViewModel != null)
                    labelViewModel.EditLabel(label);
                _headerExpanderOrg = name;
                var rightViewModel = PageNavigatorHelper.GetRightElementViewModel();
                ApplicationContext.IsRebuildTree = true;
                rightViewModel.LoadLabelView();
            }
        }

        private void BtnHeader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((Button)sender).Visibility = Visibility.Hidden;
            var tbHeader = Expander.FindChild<TextBox>("TbExpandHeader");
            if (tbHeader != null)
            {
                tbHeader.Visibility = Visibility.Visible;
                tbHeader.Focus();
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

        private void ChbOs_OnChecked(object sender, RoutedEventArgs e)
        {

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
                    if (child.GetType() == typeof(LabelCriteriaElement))
                    {
                        var ldapElem = child as LabelCriteriaElement;
                        ldapElem.Model.IsActived = false;
                    }
                }
            }
            Model.IsActived = true;
        }
    }
}