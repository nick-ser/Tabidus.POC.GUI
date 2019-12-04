using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    public partial class MemoryCriteriaElement : UserControl
    {
        public MemoryCriteriaElement()
        {
            InitializeComponent();
            Model = new MemoryCriteriaViewModel(this);
        }

        public MemoryCriteriaElement(bool isAddBtnVisible) : this()
        {
            Model = new MemoryCriteriaViewModel(this, isAddBtnVisible);
        }

        public MemoryCriteriaViewModel Model
        {
            get { return DataContext as MemoryCriteriaViewModel; }
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

        private void BtnSub_OnClick(object sender, RoutedEventArgs e)
        {
            var parent = Parent as StackPanel;
            if (parent != null && parent.Children.Count > 1)
            {
                
                var background = new BackgroundWorker();
                background.DoWork += Background_DoWork;
                background.RunWorkerAsync(Model.CriteriaId);
                parent.Children.Remove(this);
                var el = parent.Children[parent.Children.Count - 1] as MemoryCriteriaElement;
                if (el != null)
                {
                    var evm = el.Model;
                    evm.BtnAddVisible = true;
                    if (parent.Children.Count == 1)
                    {
                        evm.BtnDelVisible = false;
                    }
                }
                var elfirst = parent.Children[0] as MemoryCriteriaElement;
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
                var isChecked = labelElem.Model.ChbMemoryChecked;
                var fieldName = CommonConstants.EndpointMemory;
                var type = CriteriaType.Real;
                var lboc = new AssignmentOperatorCriteria();
                lboc.Operator = ConstantHelper.GreaterThanOperator;
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
                var el = new MemoryCriteriaElement(true);
                el.Model.CriteriaId = labelCriteriaId;
                parent.Children.Add(el);
            }
            
        }

        private void Addbackgrount_DoWork(object sender, DoWorkEventArgs e)
        {
            var lc = (AssignmentRuleCriteriaEnt)e.Argument;
            labelCriteriaId = Functions.AddDirectoryAssignmentRuleCriteria(lc);
        }

        private void CbMemoryOpes_DropDownClosed(object sender, System.EventArgs e)
        {
            var parent = Parent as StackPanel;
            var labelElem = parent.TryFindParent<AssignmentCriterialElement>();
            if (labelElem != null)
            {
                labelElem.Model.OnSaveRuleCriteria(null);
            }
        }

        private string currentText = "";
        private void TxtMemoryCriteria_LostFocus(object sender, RoutedEventArgs e)
        {
            if (currentText != TxtMemoryCriteria.Text)
            {
                var parent = Parent as StackPanel;
                var labelElem = parent.TryFindParent<AssignmentCriterialElement>();
                if (labelElem != null)
                {
                    labelElem.Model.OnSaveRuleCriteria(null);
                }
                currentText = TxtMemoryCriteria.Text;
            }

        }
    }
}