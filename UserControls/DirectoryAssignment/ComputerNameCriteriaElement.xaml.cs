﻿using System.Collections.Generic;
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
    public partial class ComputerNameCriteriaElement : UserControl
    {
        private int labelCriteriaId;
        private string currentText = "";
        public ComputerNameCriteriaElement()
        {
            InitializeComponent();
            Model = new ComputerNameCriteriaViewModel(this);
            currentText = TxtComputerCriteria.Text;
        }

        public ComputerNameCriteriaElement(bool isVisibleAddBtn) : this()
        {
            Model = new ComputerNameCriteriaViewModel(this, isVisibleAddBtn);
        }

        public ComputerNameCriteriaViewModel Model
        {
            get { return DataContext as ComputerNameCriteriaViewModel; }
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
                var el = parent.Children[parent.Children.Count - 1] as ComputerNameCriteriaElement;
                if (el != null)
                {
                    var evm = el.Model;
                    evm.BtnAddVisible = true;
                    if (parent.Children.Count == 1)
                    {
                        evm.BtnDelVisible = false;
                    }
                }
                var elfirst = parent.Children[0] as ComputerNameCriteriaElement;
                if (elfirst != null)
                {
                    var evm = elfirst.Model;
                    evm.LabelOrVisible = false;
                }
            }
        }

        
        private void Background_DoWork(object sender, DoWorkEventArgs e)
        {
            var cid = (int) e.Argument;
            Functions.DeleteDirectoryAssignmentRuleCriteria(cid);
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var parent = Parent as StackPanel;
            var labelElem = parent.TryFindParent<AssignmentCriterialElement>();
            if (labelElem != null)
            {
                var id = labelElem.Model.Id;
                var isChecked = labelElem.Model.ChbComputerChecked;
                var fieldName = CommonConstants.EndpointComputerName;
                var type = CriteriaType.String;
                var lboc = new AssignmentOperatorCriteria();
                lboc.Operator = ConstantHelper.IsOperator;
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

        private void Addbackgrount_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var parent = Parent as StackPanel;
            if (parent != null)
            {
                Model.BtnAddVisible = false;
                Model.BtnDelVisible = true;
                var el = new ComputerNameCriteriaElement(true);
                el.Model.CriteriaId = labelCriteriaId;
                parent.Children.Add(el);
            }
            
        }
        private void Addbackgrount_DoWork(object sender, DoWorkEventArgs e)
        {
            var lc = (AssignmentRuleCriteriaEnt) e.Argument;
            labelCriteriaId = Functions.AddDirectoryAssignmentRuleCriteria(lc);
        }
       
        private void TxtComputerCriteria_TextChanged(object sender, TextChangedEventArgs e)
        {
           
                
        }

        private void TxtComputerCriteria_LostFocus(object sender, RoutedEventArgs e)
        {
            if (currentText != TxtComputerCriteria.Text)
            {
                Model.TxtComputerCriteria = TxtComputerCriteria.Text;
                var parent = Parent as StackPanel;
                var labelElem = parent.TryFindParent<AssignmentCriterialElement>();
                if (labelElem != null)
                {
                    labelElem.Model.OnSaveRuleCriteria(null);
                }
                currentText = TxtComputerCriteria.Text;
                
            }
        }

       private void CbComputerOpes_DropDownClosed(object sender, System.EventArgs e)
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