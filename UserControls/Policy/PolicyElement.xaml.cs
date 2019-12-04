using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Infragistics.DragDrop;
using Infragistics.Windows;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.Model.DirectoryAssignment;
using Tabidus.POC.Common.Model.Discovery;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.POCAgent;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls.Discovery;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.DirectoryAssignment;
using Tabidus.POC.GUI.ViewModel.Policy;

namespace Tabidus.POC.GUI.UserControls.Policy
{
    /// <summary>
    ///     Interaction logic for AssignmentCriterialElement.xaml
    /// </summary>
    public partial class PolicyElement : UserControl
    {
        private string _headerExpanderOrg;

        public PolicyElement()
        {
            InitializeComponent();
            DataContext = new PolicyElementViewModel(this);
            Loaded += PolicyElement_Loaded;
        }

        public PolicyElementViewModel Model
        {
            get { return DataContext as PolicyElementViewModel; }
        }

        private void PolicyElement_Loaded(object sender, RoutedEventArgs e)
        {
            var btnHeader = Expander.FindChild<System.Windows.Controls.Label>("BtnExpandHeader");
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
                var btnHeader = Expander.FindChild<System.Windows.Controls.Label>("BtnExpandHeader");
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = _headerExpanderOrg;
                    var messageDialog =
                        PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.ShowMessageDialog("Policy name cannot be empty", "Message");
                }
                else
                {
                    UpdateRule(tb.Text);
                }
                tb.Visibility = Visibility.Hidden;
                ApplicationContext.IsBusy = false;
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
            var dateNow = DateTime.Now;
            TimeSpan span = dateNow - _editTimeStart;
            int milisecondRange = (int)span.TotalMilliseconds;
            if (milisecondRange < 10)
            {
                return;
            }
            var btnHeader = Expander.FindChild<System.Windows.Controls.Label>("BtnExpandHeader");
            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = _headerExpanderOrg;
                var messageDialog =
                    PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;
                messageDialog.ShowMessageDialog("Policy name cannot be empty", "Message");
            }
            else
            {
                UpdateRule(tb.Text);
            }

            tb.Visibility = Visibility.Hidden;
            ApplicationContext.IsBusy = false;
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
            if (Model.IsActived)
            {
                var pocAgentVm = PageNavigatorHelper.GetMainContentViewModel<POCAgentViewModel>();
                if (pocAgentVm != null)
                {
                    pocAgentVm.Name = name;
                    pocAgentVm.SavePolicyCommand.Execute(null);
                }
            }
            
            _headerExpanderOrg = name;
        }

        public DateTime _editTimeStart;
        private void BtnHeader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((System.Windows.Controls.Label) sender).Visibility = Visibility.Hidden;
            var tbHeader = Expander.FindChild<TextBox>("TbExpandHeader");
            if (tbHeader != null)
            {
                tbHeader.Visibility = Visibility.Visible;
                tbHeader.Focus();
                tbHeader.SelectAll();
            }
            _editTimeStart = DateTime.Now;
        }

        private void Expander_OnExpanded(object sender, RoutedEventArgs e)
        {
            ActiveCurrentExpander();
        }

        public void ActiveCurrentExpander()
        {
            var container = Parent as StackPanel;
            if (container != null)
            {
                foreach (var child in container.Children)
                {
                    if (child.GetType() == typeof(PolicyElement))
                    {
                        var ldapElem = child as PolicyElement;
                        ldapElem.Model.IsActived = false;
                    }
                }
            }
            Model.IsActived = true;
            Model.OnActiveChanged();
        }

        private void DragSource_DragEnter(object sender, Infragistics.DragDrop.DragDropCancelEventArgs e)
        {
            //set drag data
            var xdt = e.DragSource as Border;
            var cpr = xdt.TryFindParent<Expander>();
            var ep = cpr.DataContext as PolicyElementViewModel;
            if (ep == null)
            {
                e.Cancel = true;
            }
            else e.Data = ep;

            //change allow or not
            e.OperationType = OperationType.DropNotAllowed;
            if (e.DropTarget is XamDataTreeNodeControl)
            {
                e.OperationType = OperationType.Move;
            }
        }

        private void DragSource_OnDragLeave(object sender, DragDropEventArgs e)
        {
            e.OperationType = OperationType.DropNotAllowed;
        }

        private void DragSource_Drop(object sender, Infragistics.DragDrop.DropEventArgs e)
        {
            var data = e.Data as PolicyElementViewModel;
            var drca = e.DropTarget as XamDataTreeNodeControl;
            if (drca != null && data!=null)
            {
                var node = drca.Node;
                if (node != null)
                {
                    var ndata = node.Data as DirectoryNode;
                    var listPolicyAssign = new List<PolicyAssign>();
                    var pa = new PolicyAssign
                    {
                        Color = data.ExpanderBackgroundColor,
                        ObjectId = ndata.NodeId,
                        ObjectType = ndata.IsFolder?0:1,
                        PolicyAgentId = data.Id
                    };
                    listPolicyAssign.Add(pa);
                    var policyAssigningBg = new BackgroundWorkerHelper();
                    policyAssigningBg.AddDoWork(SaveBackgroundWorker_DoWork)
                        .DoWork(listPolicyAssign);
                }
            }
        }
        private void SaveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Dispatcher.Invoke(DispatcherPriority.Render, (Action)(() =>
            {
                var listPolicyAssign = e.Argument as List<PolicyAssign>;
                var pocAgentVm = PageNavigatorHelper.GetMainContentViewModel<POCAgentViewModel>();
                pocAgentVm.AssignPolicy(listPolicyAssign);
            }));
        }
        private void DragSource_DragStart(object sender, Infragistics.DragDrop.DragDropStartEventArgs e)
        {
            var rightTreeVm = PageNavigatorHelper.GetRightElementViewModel();
            rightTreeVm.SetNodeDropable();
            var xdt = e.DragSource as Border;
            var cpr = xdt.TryFindParent<Expander>();
            var ep = cpr.DataContext as PolicyElementViewModel;
            var dragSource = sender as DragSource;
            DataTemplate cardLayout = new DataTemplate();
            cardLayout.DataType = typeof(StackPanel);

            FrameworkElementFactory cardHolder = new FrameworkElementFactory(typeof(TextBlock));
            cardHolder.SetValue(TextBlock.TextProperty, ep.Name);
            cardHolder.SetValue(TextBlock.ForegroundProperty, (SolidColorBrush)(new BrushConverter().ConvertFrom(ep.ExpanderBackgroundColor)));
            cardLayout.VisualTree = cardHolder;
            dragSource.DragTemplate = cardLayout;
        }

        private void DragSource_DragOver(object sender, Infragistics.DragDrop.DragDropMoveEventArgs e)
        {
            if (e.DropTarget is XamDataTreeNodeControl)
            {
                XamDataTreeNodeControl drca = e.DropTarget as XamDataTreeNodeControl;
                var dtnode = drca.Node;
                if (dtnode == null)
                {
                    e.OperationType = OperationType.DropNotAllowed;
                }
            }
        }

        private void DragSource_OnDragEnd(object sender, DragDropEventArgs e)
        {
            var rightTreeVm = PageNavigatorHelper.GetRightElementViewModel();
            rightTreeVm.SetNodeDropableOrNot();
        }
    }
}