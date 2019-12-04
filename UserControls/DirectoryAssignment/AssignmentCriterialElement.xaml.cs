using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.Model.DirectoryAssignment;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls.Discovery;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.DirectoryAssignment;

namespace Tabidus.POC.GUI.UserControls.DirectoryAssignment
{
    /// <summary>
    ///     Interaction logic for AssignmentCriterialElement.xaml
    /// </summary>
    public partial class AssignmentCriterialElement : UserControl
    {
        private string _headerExpanderOrg;

        public AssignmentCriterialElement()
        {
            InitializeComponent();
            //DataContext = new AssignmentCriteriaViewModel(this);
            Model = new AssignmentCriteriaViewModel(this);
            Loaded += DirAssignmentCriteriaElement_Loaded;
        }

        public AssignmentCriteriaViewModel Model
        {
            get { return DataContext as AssignmentCriteriaViewModel; }
            set { DataContext = value; }
            //get { return DataContext as AssignmentCriteriaViewModel; }
        }

        private void DirAssignmentCriteriaElement_Loaded(object sender, RoutedEventArgs e)
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
                var tb = (TextBox) sender;
                var btnHeader = Expander.FindChild<Button>("BtnExpandHeader");
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = _headerExpanderOrg;
                    var messageDialog =
                        PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.ShowMessageDialog("Rule name cannot be empty", "Message");
                }
                else
                {
                    UpdateRule(tb.Text);
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
            var tb = (TextBox) sender;
            var btnHeader = Expander.FindChild<Button>("BtnExpandHeader");
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = _headerExpanderOrg;
                var messageDialog =
                    PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                messageDialog.ShowMessageDialog("Rule name cannot be empty", "Message");
            }
            else
            {
                UpdateRule(tb.Text);
            }

            tb.Visibility = Visibility.Hidden;

            if (btnHeader != null)
            {
                btnHeader.Content = _headerExpanderOrg;
                btnHeader.Visibility = Visibility.Visible;
            }
        }

        private void UpdateRule(string name)
        {
            if (name.Length > 200)
            {
                var messageDialog =
                    PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                messageDialog.ShowMessageDialog("Rule name cannot more than 200 letters", "Message");
                return;
            }

            var rule = new EditRuleDataRequest(name, Model.Id, Model.IsEnable);
            //Edit assignment rule
            Model.EditAssigmentRule(rule);
            _headerExpanderOrg = name;
        }

        private void BtnHeader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((Button) sender).Visibility = Visibility.Hidden;
            var tbHeader = Expander.FindChild<TextBox>("TbExpandHeader");
            if (tbHeader != null)
            {
                tbHeader.Visibility = Visibility.Visible;
                tbHeader.Focus();
            }
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
                    if (child.GetType() == typeof(AssignmentCriterialElement))
                    {
                        var ldapElem = child as AssignmentCriterialElement;
                        ldapElem.Model.IsActived = false;
                    }
                }
            }
            Model.IsActived = true;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var parent = Parent as StackPanel;
            var labelElem = parent.TryFindParent<AssignmentCriterialElement>();
            if (labelElem != null)
            {
                labelElem.Model.OnSaveRuleCriteria(null);
            }
        }

    }
}