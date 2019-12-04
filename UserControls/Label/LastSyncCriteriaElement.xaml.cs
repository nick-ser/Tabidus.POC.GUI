using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Label;

namespace Tabidus.POC.GUI.UserControls.Label
{
    /// <summary>
    ///     Interaction logic for ComputerNameCriteriaElement.xaml
    /// </summary>
    public partial class LastSyncCriteriaElement : UserControl
    {
        public LastSyncCriteriaElement()
        {
            InitializeComponent();
            Model = new LastSyncCriteriaViewModel(this);
        }

        public LastSyncCriteriaElement(bool isVisibleAddBtn) : this()
        {
            Model = new LastSyncCriteriaViewModel(this, isVisibleAddBtn);
        }

        public LastSyncCriteriaViewModel Model
        {
            get { return DataContext as LastSyncCriteriaViewModel; }
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
                background.RunWorkerCompleted += Background_RunWorkerCompleted;
                background.RunWorkerAsync(Model.CriteriaId);
                parent.Children.Remove(this);
                var el = parent.Children[parent.Children.Count - 1] as LastSyncCriteriaElement;
                if (el != null)
                {
                    var evm = el.Model;
                    evm.BtnAddVisible = true;
                    if (parent.Children.Count == 1)
                    {
                        evm.BtnDelVisible = false;
                    }
                }
                var elfirst = parent.Children[0] as LastSyncCriteriaElement;
                if (elfirst != null)
                {
                    var evm = elfirst.Model;
                    evm.LabelOrVisible = false;
                }
            }
        }

        private void Background_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Reload LabelFilterViewModel
            var rvm = PageNavigatorHelper.GetRightElementViewModel();
            rvm.LoadLabelView(true);
            
        }

        private void Background_DoWork(object sender, DoWorkEventArgs e)
        {
            var cid = (int)e.Argument;
            Functions.DeleteLabelCriteria(cid);
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            
            var parent = Parent as StackPanel;
            var labelElem = parent.TryFindParent<LabelCriteriaElement>();
            if (labelElem != null)
            {
                var labelId = labelElem.Model.LabelId;
                var isChecked = labelElem.Model.ChbLastSyncChecked;
                var fieldName = CommonConstants.EndpointLastSynchronization;
                var type = CriteriaType.Int;
                var lboc = new LabelOperatorCriteria();
                lboc.Operator = ConstantHelper.IsWithinOperator;
                lboc.Value1 = string.Empty;
                lboc.Value2 = string.Empty;
                var lc = new LabelCriteria
                {
                    LabelId = labelId,
                    FieldName = fieldName,
                    Type = (byte)type,
                    IsAvailable = isChecked,
                    LabelOperatorCriterias = new List<LabelOperatorCriteria>()
                };
                lc.LabelOperatorCriterias.Add(lboc);
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
                var el = new LastSyncCriteriaElement(true);
                el.Model.CriteriaId = labelCriteriaId;
                parent.Children.Add(el);
            }
            //Reload LabelFilterViewModel
            var rvm = PageNavigatorHelper.GetRightElementViewModel();
            rvm.LoadLabelView(true);
            
        }

        private void Addbackgrount_DoWork(object sender, DoWorkEventArgs e)
        {
            var lc = (LabelCriteria)e.Argument;
            labelCriteriaId = Functions.AddLabelCriteria(lc);
        }

        private void CbLastSyncOpes_DropDownClosed(object sender, System.EventArgs e)
        {
            PageNavigatorHelper.GetMainContentViewModel<LabelViewModel>().OnSaveLabel(null);
        }

        private string currentText = "";
        private void TxtLastSyncCriteria_LostFocus(object sender, RoutedEventArgs e)
        {
            if (currentText != TxtLastSyncCriteria.Text)
            {
                PageNavigatorHelper.GetMainContentViewModel<LabelViewModel>().OnSaveLabel(null);
                currentText = TxtLastSyncCriteria.Text;
            }
        }
    }
}