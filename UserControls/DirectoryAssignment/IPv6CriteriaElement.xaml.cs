using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.Model.DirectoryAssignment;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.DirectoryAssignment;

namespace Tabidus.POC.GUI.UserControls.DirectoryAssignment
{
    /// <summary>
    ///     Interaction logic for ComputerNameCriteriaElement.xaml
    /// </summary>
    public partial class IPv6CriteriaElement : UserControl
    {
        public IPv6CriteriaElement()
        {
            InitializeComponent();
            Model = new IPv6CriteriaViewModel(this);
        }

        public IPv6CriteriaElement(bool isAddBtnVisible) : this()
        {
            Model = new IPv6CriteriaViewModel(this, isAddBtnVisible);
        }

        public IPv6CriteriaViewModel Model
        {
            get { return DataContext as IPv6CriteriaViewModel; }
            set { DataContext = value; }
        }

        private void BtnSub_OnClick(object sender, RoutedEventArgs e)
        {
            var parent = Parent as StackPanel;
            if (parent != null && parent.Children.Count > 1)
            {
                
                var background = new BackgroundWorker();
                background.DoWork += Background_DoWork;
                background.RunWorkerAsync(Model.CriteriaId);
                parent.Children.Remove(this);
                var el = parent.Children[parent.Children.Count - 1] as IPv6CriteriaElement;
                if (el != null)
                {
                    var evm = el.Model;
                    evm.BtnAddVisible = true;
                    if (parent.Children.Count == 1)
                    {
                        evm.BtnDelVisible = false;
                    }
                }
                var elfirst = parent.Children[0] as IPv6CriteriaElement;
                if (elfirst != null)
                {
                    var evm = elfirst.Model;
                    evm.LabelOrVisible = false;
                }
            }
        }
        
        private void Background_DoWork(object sender, DoWorkEventArgs e)
        {
            var cid = (int)e.Argument;
            Functions.DeleteDirectoryAssignmentRuleCriteria(cid);
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            
            var parent = Parent as StackPanel;
            var labelElem = parent.TryFindParent<AssignmentCriterialElement>();
            if (labelElem != null)
            {
                var id = labelElem.Model.Id;
                var isChecked = labelElem.Model.ChbIPv6Checked;
                var fieldName = CommonConstants.EndpointIPv6;
                var type = CriteriaType.String;
                var lboc = new AssignmentOperatorCriteria();
                lboc.Operator = ConstantHelper.IsBetweenOperator;
                lboc.Value1 = string.Empty;
                lboc.Value2 = string.Empty;
                var lc = new AssignmentRuleCriteriaEnt
                {
                    Id = id,
                    FieldName = fieldName,
                    Type = (byte)type,
                    IsAvailable = isChecked,
                    Operator = lboc.Operator,
                    Value1 = lboc.Value1,
                    Value2 = lboc.Value2,
                    AssignmentRuleId = id
                };
                
                var addbackgrount = new BackgroundWorker();
                addbackgrount.DoWork += Addbackgrount_DoWork;
                addbackgrount.RunWorkerCompleted += Addbackgrount_RunWorkerCompleted;
                addbackgrount.RunWorkerAsync(lc);
            }
            
        }

        private int labelCriteriaId;
        private void Addbackgrount_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var parent = Parent as StackPanel;
            if (parent != null)
            {
                Model.BtnAddVisible = false;
                Model.BtnDelVisible = true;
                var el = new IPv6CriteriaElement(true);
                el.Model.CriteriaId = labelCriteriaId;
                parent.Children.Add(el);
            }
            
        }

        private void Addbackgrount_DoWork(object sender, DoWorkEventArgs e)
        {
            var lc = (AssignmentRuleCriteriaEnt)e.Argument;
            labelCriteriaId = Functions.AddDirectoryAssignmentRuleCriteria(lc);
        }

        private string currentText2 = "";
        private void TxtIPv6Criteria1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (currentText2 != TxtIPv6Criteria1.Text)
            {
                Model.TxtIPv6Criteria1 = TxtIPv6Criteria1.Text;
                var parent = Parent as StackPanel;
                var labelElem = parent.TryFindParent<AssignmentCriterialElement>();
                if (labelElem != null)
                {
                    labelElem.Model.OnSaveRuleCriteria(null);
                }
                currentText2 = TxtIPv6Criteria1.Text;
            }
        }

        private string currentText = "";
        private void TxtIPv6Criteria2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (currentText != TxtIPv6Criteria2.Text)
            {
                Model.TxtIPv6Criteria2 = TxtIPv6Criteria2.Text;
                var parent = Parent as StackPanel;
                var labelElem = parent.TryFindParent<AssignmentCriterialElement>();
                if (labelElem != null)
                {
                    labelElem.Model.OnSaveRuleCriteria(null);
                }
                currentText = TxtIPv6Criteria2.Text;
            }

        }

        private void CbIPv6Opes_DropDownClosed(object sender, System.EventArgs e)
        {
//            PageNavigatorHelper.GetMainContentViewModel<LabelViewModel>().OnSaveLabel(null);
        }
    }
}