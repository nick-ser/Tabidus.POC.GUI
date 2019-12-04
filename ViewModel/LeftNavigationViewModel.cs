using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.Endpoint;
using Tabidus.POC.GUI.ViewModel.MainWindowView;

namespace Tabidus.POC.GUI.ViewModel
{
    public class LeftNavigationViewModel : ViewModelBase
    {
        private static LeftNavigation _view;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="view"></param>
        public LeftNavigationViewModel(LeftNavigation view)
        {
            SubEndpointVisibility = 1;
            SubDiscoveryVisibility = 0.99;
            SubSoftwareVisibility = 0.99;
            SubPolicyVisibility = 0.99;
            SubReportingVisibility = 0.99;
            var mainModel = GetMainModel();
            if (mainModel != null)
            {
                mainModel.AddDeleteButtonVisible = true;
            }

            _view = view;
            //TabSelectedCommand = new RelayCommand<Button>(OnTabSelected);
            NavigatorCommand = new RelayCommand<Button>(OnNavigatorCommand);
        }

        #region Properties

        //this property use to visible or hidden sub-endpoint button
        private double _subEndpointVisibility;

        public double SubEndpointVisibility
        {
            get { return _subEndpointVisibility; }
            set
            {
                _subEndpointVisibility = value;
                OnPropertyChanged("SubEndpointVisibility");
            }
        }
        private double _subSoftwareVisibility;

        public double SubSoftwareVisibility
        {
            get { return _subSoftwareVisibility; }
            set
            {
                _subSoftwareVisibility = value;
                OnPropertyChanged("SubSoftwareVisibility");
            }
        }
        private double _subPolicyVisibility;

        public double SubPolicyVisibility
        {
            get { return _subPolicyVisibility; }
            set
            {
                _subPolicyVisibility = value;
                OnPropertyChanged("SubPolicyVisibility");
            }
        }

        private double _subReportingVisibility;

        public double SubReportingVisibility
        {
            get { return _subReportingVisibility; }
            set
            {
                _subReportingVisibility = value;
                OnPropertyChanged("SubReportingVisibility");
            }
        }

        private double _subDiscoveryVisibility;

        public double SubDiscoveryVisibility
        {
            get { return _subDiscoveryVisibility; }
            set
            {
                _subDiscoveryVisibility = value;
                OnPropertyChanged("SubDiscoveryVisibility");
            }
        }

        #endregion

        #region Command

        // Implementation of colors(Submenu of Endpoint) on 8th april 2019
        public ICommand TabSelectedCommand { get; private set; }
       

       
        //private void OnTabSelected(Button btn)
        //{
        //    if (btn == null || ApplicationContext.NodesSelected == null || ApplicationContext.NodesSelected.Count == 0)
        //        return;

        //    List<DirectoryNode> folders = new List<DirectoryNode>();
        //    List<DirectoryNode> endpoints = new List<DirectoryNode>();
        //    foreach (var n in ApplicationContext.NodesSelected)
        //    {
        //        if (n.IsFolder)
        //        {
        //            folders.Add(n);
        //        }
        //        else
        //        {
        //            endpoints.Add(n);
        //        }
        //    }

        //    switch (btn.Content.ToString())
        //    {
        //        case UIConstant.ColorCodes:
        //            PageNavigatorHelper.Switch(new ColorCodePage(folders[0]));
        //            break;
        //        case UIConstant.Endpoints:
        //            var fids = folders.Select(e => e.NodeId).ToList();
        //            var eids = endpoints.Select(e => e.NodeId).ToList();
        //            if (!PageNavigatorHelper.IsCurrent<EndPointListPage>())
        //                PageNavigatorHelper.Switch(new EndPointListPage());
        //            var viewModel =
        //                PageNavigatorHelper.GetMainContentViewModel<GroupViewModel>();

        //            viewModel.SetParamsValueForDirectory(fids, eids,
        //                ApplicationContext.SearchText, false, Guid.NewGuid(), "");
        //            viewModel.GetData();
        //            break;
        //        case UIConstant.DirectoryAssignment:
        //            if (!PageNavigatorHelper.IsCurrent<DirectoryAssignmentPage>())
        //            {
        //                var assignmentPage = new DirectoryAssignmentPage();
        //                PageNavigatorHelper.Switch(assignmentPage);
        //            }
        //            break;
        //        case UIConstant.TaskListPage:
        //            if (!PageNavigatorHelper.IsCurrent<TaskListPage>())
        //            {
        //                var taskListPage = new TaskListPage();
        //                PageNavigatorHelper.Switch(taskListPage);
        //            }
        //            break;
        //        case UIConstant.GroupPolicies:
        //            if (!PageNavigatorHelper.IsCurrent<PolicyEnhancementPage>())
        //            {
        //                PageNavigatorHelper.Switch(new PolicyEnhancementPage());
        //            }
        //            break;
        //    }
        //}
        //private void OnTabSelected(Button btn)
        //{
        //    var assignmentViewModel = PageNavigatorHelper.GetMainContentViewModel<GroupHeaderViewModel>();
        //    assignmentViewModel.OnTabSelected(btn);
        //}
        public ICommand NavigatorCommand { get; set; }

        public void SetActiveButton(string btnName)
        {
            _view.SetActiveButton(btnName);
        }

        #endregion

        #region Private methods

        /// <summary>
        ///     Called when [navigator selected].
        /// </summary>
        /// <param name="btn">The BTN.</param>
        private void OnNavigatorCommand(Button btn)
        {
            var rightVM = PageNavigatorHelper.GetRightElementViewModel();
            _view.StSubDiscovery.Visibility = Visibility.Visible;
            _view.StSubEndpoint.Visibility = Visibility.Visible;
            _view.StSubSoftware.Visibility = Visibility.Visible;
            _view.StSubPolicies.Visibility = Visibility.Visible;
            _view.StSubReporting.Visibility = Visibility.Visible;
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            var previousIndex = mainViewModel.NavigationIndex;
            switch (btn.Name)
            {
                case "BtnEndPoint":
                    EndPointNavigationClick();
                    if (rightVM != null)
                    {
                        rightVM.DirectoryTreeVisible = true;
                        rightVM.SoftwareTreeVisible = false;
                        if (rightVM.DirectoryPushed)
                        {
                            mainViewModel.AddDeleteButtonVisible = true;
                        }
                    }
                    break;
                case "SubBtnLabel":
                    OnLabelNavExecute();
                    if (rightVM != null)
                    {
                        rightVM.DirectoryTreeVisible = true;
                        rightVM.SoftwareTreeVisible = false;
                        if (rightVM.DirectoryPushed)
                        {
                            mainViewModel.AddDeleteButtonVisible = true;
                        }
                    }
                    break;
                case "SubBtnColor":
                    CanTabSelected(btn);
                    if (rightVM != null)
                    {
                        rightVM.DirectoryTreeVisible = true;
                        rightVM.SoftwareTreeVisible = false;
                        if (rightVM.DirectoryPushed)
                        {
                            mainViewModel.AddDeleteButtonVisible = true;
                        }
                    }
                    break;
                case "SubBtnAssignments":
                    OnAssignmentNavExecute();
                    if (rightVM != null)
                    {
                        rightVM.DirectoryTreeVisible = true;
                        rightVM.SoftwareTreeVisible = false;
                        if (rightVM.DirectoryPushed)
                        {
                            mainViewModel.AddDeleteButtonVisible = true;
                        }
                    }
                    break;
                case "BtnDiscovery":
                    OnDiscoveryNavExecute();
                    if (rightVM != null)
                    {
                        rightVM.DirectoryTreeVisible = true;
                        rightVM.SoftwareTreeVisible = false;
                        if (rightVM.DirectoryPushed)
                        {
                            mainViewModel.AddDeleteButtonVisible = true;
                        }
                    }
                    break;
                case "SubBtnNeighborhood":
                    OnNeighborhoodNavExecute();
                    if (rightVM != null)
                    {
                        rightVM.DirectoryTreeVisible = true;
                        rightVM.SoftwareTreeVisible = false;
                        if (rightVM.DirectoryPushed)
                        {
                            mainViewModel.AddDeleteButtonVisible = true;
                        }
                    }
                    break;
                case "SubBtnLDAP":
                    OnLDAPNavExecute();
                    if (rightVM != null)
                    {
                        rightVM.DirectoryTreeVisible = true;
                        rightVM.SoftwareTreeVisible = false;
                        if (rightVM.DirectoryPushed)
                        {
                            mainViewModel.AddDeleteButtonVisible = true;
                        }
                    }
                    break;
                case "BtnSoftware":
                    OnSoftwareNavExecute();
                    if (rightVM != null)
                    {
                        rightVM.BuilSoftwareTree();
                        rightVM.DirectoryTreeVisible = false;
                        rightVM.SoftwareTreeVisible = true;
                        mainViewModel.AddDeleteButtonVisible = false;
                    }
                    break;
                case "SubBtnDownload":
                    OnDownloadNavExecute();
                    if (rightVM != null)
                    {
                        rightVM.DirectoryTreeVisible = false;
                        rightVM.SoftwareTreeVisible = true;
                        mainViewModel.AddDeleteButtonVisible = false;
                    }
                    break;
                case "SubBtnTransfer":
                    OnTransferNavExecute();
                    if (rightVM != null)
                    {
                        rightVM.DirectoryTreeVisible = false;
                        rightVM.SoftwareTreeVisible = true;
                        mainViewModel.AddDeleteButtonVisible = false;
                    }
                    break;
                case "BtnLicense":
                    OnLicenseNavExecute();
                    break;
                case "BtnPolicy":
                    OnPolicyNavExecute();
                    break;
                case "SubBtnPOCAgent":
                    OnPOCAgentNavExecute();
                    if (rightVM != null)
                    {
                        rightVM.DirectoryTreeVisible = true;
                        rightVM.SoftwareTreeVisible = false;
                        mainViewModel.AddDeleteButtonVisible = false;
                    }
                    break;
                case "SubBtnEndpointPatron":
                    break;
                case "BtnReporting":
                    OnReportingNavExecute();
                    break;
                case "SubBtnQuarantine":
                    OnQuarantineNavExecute();
                    break;
                case "BtnNotification":
                    OnNotificationNavExecute();
                    break;
                case "BtnSetting":
                    OnSettingNavExecute();
                    break;
            }
            if (btn.Name != "SubBtnPOCAgent" && previousIndex == (int)NavigationIndexes.POCAgent)
            {
                ApplicationContext.IsRebuildTree = true;
                rightVM.ReBuildTree(ApplicationContext.NodesSelected);
            }
        }
        private void CanTabSelected(Button btn)
        {
            if (btn == null || ApplicationContext.NodesSelected == null || ApplicationContext.NodesSelected.Count == 0 || ApplicationContext.IsFromLabel)
                return;
            List<DirectoryNode> folders = new List<DirectoryNode>();
            List<DirectoryNode> endpoints = new List<DirectoryNode>();
            foreach (var n in ApplicationContext.NodesSelected)
            {
                if (n.IsFolder)
                {
                    folders.Add(n);
                }
                else
                {
                    endpoints.Add(n);
                }
            }
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.Colors;
            }
            //PageNavigatorHelper.Switch(new ColorCodePage(folders[0]));
            if (folders.Count == 0)
            {
                PageNavigatorHelper.Switch(new ColorCodePage(endpoints[0]));
            }
            else
            {
                PageNavigatorHelper.Switch(new ColorCodePage(folders[0]));
            }

        }
        private void OnLabelNavExecute()
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.Label;
            }
            PageNavigatorHelper.Switch(new LabelsPage());
        }
        private void OncolorNavExecute()
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.Colors;
            }
            PageNavigatorHelper.Switch(new ColorCodePage());
        }
        private void OnAssignmentNavExecute()
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.Assignment;
            }
            PageNavigatorHelper.Switch(new DirectoryAssignmentPage());
        }
        private void OnNeighborhoodNavExecute()
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.NeighborhoodWatch;
            }
            PageNavigatorHelper.Switch(new NeighborhoodWatchPage());
        }
        private void OnDownloadNavExecute()
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.Download;
            }
            //            PageNavigatorHelper.Switch(new NeighborhoodWatchPage());
        }
        private void OnTransferNavExecute()
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.Transfer;
            }
            PageNavigatorHelper.Switch(new TransferPage());
        }

        private void OnQuarantineNavExecute()
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if(mainViewModel != null)
                mainViewModel.NavigationIndex = (int)NavigationIndexes.Transfer;
            PageNavigatorHelper.Switch(new POCQuarantinePage());
        }

        private void OnPOCAgentNavExecute()
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.POCAgent;
                ApplicationContext.IsRebuildTree = true;
                if (ApplicationContext.FolderPolicyList == null)
                {
                    Functions.LoadFolderPolicy();
                }
                if (ApplicationContext.EndpointPolicyList == null)
                {
                    Functions.LoadEndpointPolicy();
                }
                var rightVM = PageNavigatorHelper.GetRightElementViewModel();
                rightVM.ReBuildTree(ApplicationContext.NodesSelected);
            }
            PageNavigatorHelper.Switch(new POCAgentPage());
        }
        private void OnDiscoveryNavExecute()
        {
            var mainModel = PageNavigatorHelper.GetMainModel();

            SubEndpointVisibility = 0.99;
            SubSoftwareVisibility = 0.99;
            SubPolicyVisibility = 0.99;
            SubReportingVisibility = 0.99;
            SubDiscoveryVisibility = SubDiscoveryVisibility == 1 ? 0.99 : 1;

            if (mainModel != null)
            {
                mainModel.NavigationIndex = (int)NavigationIndexes.Discovery;
            }
        }

        private void OnLDAPNavExecute()
        {
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.LDAP;
            }
            PageNavigatorHelper.Switch(new LDAPPage());
        }

        private void OnSoftwareNavExecute()
        {
            SubEndpointVisibility = 0.99;
            SubDiscoveryVisibility = 0.99;
            SubPolicyVisibility = 0.99;
            SubReportingVisibility = 0.99;
            SubSoftwareVisibility = SubSoftwareVisibility == 1 ? 0.99 : 1;
            var mainModel = GetMainModel();
            if (mainModel != null)
            {
                mainModel.NavigationIndex = (int)NavigationIndexes.Software;
            }
            PageNavigatorHelper.Switch(new SoftwarePage());
        }

        private void OnLicenseNavExecute()
        {
            SubEndpointVisibility = 0.99;
            SubDiscoveryVisibility = 0.99;
            SubPolicyVisibility = 0.99;
            SubReportingVisibility = 0.99;
            SubSoftwareVisibility = 0.99;
            var mainModel = GetMainModel();
            if (mainModel != null)
            {
                // mainModel.AddDeleteButtonVisible = false;
                mainModel.NavigationIndex = (int)NavigationIndexes.License;
            }
        }

        private void OnPolicyNavExecute()
        {
            SubEndpointVisibility = 0.99;
            SubDiscoveryVisibility = 0.99;
            SubSoftwareVisibility = 0.99;
            SubPolicyVisibility = SubPolicyVisibility == 1 ? 0.99 : 1;
            SubReportingVisibility = 0.99;
        }

        private void OnReportingNavExecute()
        {
            SubEndpointVisibility = 0.99;
            SubDiscoveryVisibility = 0.99;
            SubPolicyVisibility = 0.99;
            SubSoftwareVisibility = 0.99;
            SubReportingVisibility = SubReportingVisibility == 1 ? 0.99 : 1;
            //var mainModel = GetMainModel();
            //if (mainModel != null)
            //{
            //    // mainModel.AddDeleteButtonVisible = false;
            //    mainModel.NavigationIndex = (int)NavigationIndexes.Reporting;
            //}
        }

        private void OnNotificationNavExecute()
        {
            SubEndpointVisibility = 0.99;
            SubDiscoveryVisibility = 0.99;
            SubPolicyVisibility = 0.99;
            SubSoftwareVisibility = 0.99;
            SubReportingVisibility = 0.99;
            var mainModel = GetMainModel();
            if (mainModel != null)
            {
                // mainModel.AddDeleteButtonVisible = false;
                mainModel.NavigationIndex = (int)NavigationIndexes.Notification;
            }
        }

        private void OnSettingNavExecute()
        {
            SubEndpointVisibility = 0.99;
            SubDiscoveryVisibility = 0.99;
            SubPolicyVisibility = 0.99;
            SubSoftwareVisibility = 0.99;
            SubReportingVisibility = 0.99;
            var mainModel = GetMainModel();
            if (mainModel != null)
            {
                // mainModel.AddDeleteButtonVisible = false;
                mainModel.NavigationIndex = (int)NavigationIndexes.Setting;
            }
        }

        public void EndPointNavigationClick()
        {
            SubDiscoveryVisibility = 0.99;
            SubSoftwareVisibility = 0.99;
            SubPolicyVisibility = 0.99;
            SubReportingVisibility = 0.99;
            SubEndpointVisibility = SubEndpointVisibility == 1 ? 0.99 : 1;
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.Endpoint;
            }
            var righModel = PageNavigatorHelper.GetRightElementViewModel();
            if (righModel != null)
            {
                if (righModel.DirectoryPushed)
                {
                    var nodeSelected = ApplicationContext.NodesSelected != null &&
                                       ApplicationContext.NodesSelected.Count > 0
                        ? ApplicationContext.NodesSelected[0]
                        : null;
                    if (nodeSelected != null)
                        SelectCurrentTreeNode(nodeSelected);
                }
                else
                {
                    var nodeSelected = ApplicationContext.LabelNodesSelected != null &&
                                       ApplicationContext.LabelNodesSelected.Count > 0
                        ? ApplicationContext.LabelNodesSelected[0]
                        : null;
                    if (nodeSelected != null)
                        righModel.SelectLabelNodeFromGrid(nodeSelected);
                }
            }
        }

        public void ChangeEndpointNavigationState()
        {
            SubDiscoveryVisibility = 0.99;
            SubPolicyVisibility = 0.99;
            SubSoftwareVisibility = 0.99;
            SubReportingVisibility = 0.99;
            SubEndpointVisibility = SubEndpointVisibility == 1 ? 0.99 : 1;
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.Endpoint;
            }
            SetActiveButton("BtnEndPoint");
        }

        public void SelectedLDAPNavigation()
        {
            SubDiscoveryVisibility = 1;
            SubEndpointVisibility = 0.99;
            SubPolicyVisibility = 0.99;
            SubSoftwareVisibility = 0.99;
            SubReportingVisibility = 0.99;
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
            {
                mainViewModel.NavigationIndex = (int)NavigationIndexes.LDAP;
            }
            SetActiveButton("SubBtnLDAP");
        }

        /// <summary>
        ///     Get MainWindowViewModel
        /// </summary>
        /// <returns></returns>
        private static MainWindowViewModel GetMainModel()
        {
            var parentPage = _view.TryFindParent<MainWindow>();
            if (parentPage != null)
            {
                var mainWindowViewModel = parentPage.DataContext as MainWindowViewModel;
                return mainWindowViewModel;
            }
            return null;
        }

        #endregion
    }
}