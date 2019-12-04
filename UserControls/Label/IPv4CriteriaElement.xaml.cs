using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
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
    public partial class IPv4CriteriaElement : UserControl
    {
        private int labelCriteriaId;
        public IPv4CriteriaElement()
        {
            InitializeComponent();
            Model = new IPv4CriteriaViewModel(this);
        }

        public IPv4CriteriaElement(bool isAddBtnVisible) : this()
        {
            Model = new IPv4CriteriaViewModel(this, isAddBtnVisible);
        }

        public IPv4CriteriaViewModel Model
        {
            get { return DataContext as IPv4CriteriaViewModel; }
            set { DataContext = value; }
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
                var el = parent.Children[parent.Children.Count - 1] as IPv4CriteriaElement;
                if (el != null)
                {
                    var evm = el.Model;
                    evm.BtnAddVisible = true;
                    if (parent.Children.Count == 1)
                    {
                        evm.BtnDelVisible = false;
                    }
                }
                var elfirst = parent.Children[0] as IPv4CriteriaElement;
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
                var isChecked = labelElem.Model.ChbIPv4Checked;
                var fieldName = CommonConstants.EndpointIPv4;
                var type = CriteriaType.String;
                var lboc = new LabelOperatorCriteria();
                lboc.Operator = ConstantHelper.IsBetweenOperator;
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

        private void Addbackgrount_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var parent = Parent as StackPanel;
            if (parent != null)
            {
                Model.BtnAddVisible = false;
                Model.BtnDelVisible = true;
                var el = new IPv4CriteriaElement(true);
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

      
        private string currentText = "";
        private void TxtIPv4Criteria1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (currentText != TxtIPv4Criteria1.Text)
            {
                Model.TxtIPv4Criteria1 = TxtIPv4Criteria1.Text;
                PageNavigatorHelper.GetMainContentViewModel<LabelViewModel>().OnSaveLabel(null);
                currentText = TxtIPv4Criteria1.Text;
            }
        }

        private string currentText2 = "";
        private void TxtIPv4Criteria2_LostFocus(object sender, RoutedEventArgs e)
        {
            if (currentText2 != TxtIPv4Criteria2.Text)
            {
                Model.TxtIPv4Criteria2 = TxtIPv4Criteria2.Text;
                PageNavigatorHelper.GetMainContentViewModel<LabelViewModel>().OnSaveLabel(null);
                currentText2 = TxtIPv4Criteria2.Text;
            }
        }

        private void CbIPv4Opes_DropDownClosed(object sender, System.EventArgs e)
        {
            PageNavigatorHelper.GetMainContentViewModel<LabelViewModel>().OnSaveLabel(null);
        }
    }
}