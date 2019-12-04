using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.DirectoryAssignment;
using Tabidus.POC.GUI.ViewModel.Task;
using GroupHeaderElement = Tabidus.POC.GUI.UserControls.Endpoint.GroupHeaderElement;

namespace Tabidus.POC.GUI.ViewModel.Endpoint
{
    /// <summary>
    /// Class GroupHeaderViewModel.
    /// </summary>
    public class GroupHeaderViewModel : ViewModelBase
    {
        /// <summary>
        /// The _view
        /// </summary>
        private static GroupHeaderElement _view;
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupHeaderViewModel" /> class.
        /// </summary>
        /// <param name="view">The view.</param>
        public GroupHeaderViewModel(GroupHeaderElement view)
        {
            _view = view;
            _view.Loaded += View_Loaded;

            TabSelectedCommand = new RelayCommand<Button>(OnTabSelected, CanTabSelected);
            AddRuleCommand = new RelayCommand(OnExecuteAddRule, CanExecuteAddRule);
            DeleteRuleCommand = new RelayCommand(OnExecuteDeleteRule, CanExecuteDeleteRule);
			AddTaskCommand = new RelayCommand(OnExecuteAddTask, CanExecuteAddTask);
        }

	    private bool CanExecuteAddTask(object arg)
	    {
		    return true;
	    }

	    private void OnExecuteAddTask(object obj)
	    {
			//Execute TaskListViewModel.AddTaskCommand
			var taskListViewModel = PageNavigatorHelper.GetMainContentViewModel<TaskListViewModel>();
			if (taskListViewModel != null)
			{
				taskListViewModel.AddTaskCommand.Execute(null);
			}
			_view.TaskCtxMenu.IsOpen = false;
		}

	    private bool CanExecuteDeleteRule(object arg)
        {
            return true;
        }

        private void OnExecuteDeleteRule(object obj)
        {
            var directoryAssignment = PageNavigatorHelper.GetMainContentViewModel<AssignmentViewModel>();
            if (directoryAssignment != null)
            {
                directoryAssignment.DeleteRuleCommand.Execute(null);
            }
            //_view.AssignmentCtxMenu.IsOpen = false;
        }

        private bool CanExecuteAddRule(object arg)
        {
            return true;
        }

        private void OnExecuteAddRule(object obj)
        {
            var directoryAssignment = PageNavigatorHelper.GetMainContentViewModel<AssignmentViewModel>();
            if (directoryAssignment != null)
            {
                directoryAssignment.AddRuleCommand.Execute(null);
            }
           // _view.AssignmentCtxMenu.IsOpen = true;
            //_view.AssignmentCtxMenu.IsOpen = false;
        }

        /// <summary>
        /// Handles the Loaded event of the View control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs" /> instance containing the event data.</param>
        private void View_Loaded(object sender, RoutedEventArgs e)
        {
			ActiveButtonTab(ActivedButtonIndex);
        }

	    private void ActiveButtonTab(int activeIndex)
	    {
			var pnlBar = _view.PnlMenuBar.Children;
			foreach (var ctrl in pnlBar)
			{
				if (ctrl.GetType() == typeof(Button))
				{
					var btn = (Button)ctrl;
					if (btn.Name == "BtnGroupMenu" + activeIndex)
					{
						btn.Style = _view.FindResource("MenuButtonHover") as Style;
						break;
					}
				}
			}
		}
        #region Command
        /// <summary>
        /// Gets the tab selected command.
        /// </summary>
        /// <value>The tab selected command.</value>
        public ICommand TabSelectedCommand { get; private set; }
        public ICommand AddRuleCommand { get; private set; }
        public ICommand DeleteRuleCommand { get; private set; }
		public ICommand AddTaskCommand { get; private set; }

        #endregion
        #region Properties
        /// <summary>
        /// The _active button index
        /// </summary>
        private int _activeButtonIndex = 1;

        /// <summary>
        /// Gets or sets the index of the actived button.
        /// </summary>
        /// <value>The index of the actived button.</value>
        public int ActivedButtonIndex
        {
            get { return _activeButtonIndex; }
            set
            {
                _activeButtonIndex = value;
                OnPropertyChanged("ActivedButtonIndex");
            }
        }

        /// <summary>
        /// The _folder path name
        /// </summary>
        private string _folderPathName;

        /// <summary>
        /// Gets or sets the name of the folder path.
        /// </summary>
        /// <value>The name of the folder path.</value>
        public string FolderPathName
        {
            get { return _folderPathName; }
            set
            {
                _folderPathName = value;
                OnPropertyChanged("FolderPathName");
            }
        }

        /// <summary>
        /// The _total endpoints
        /// </summary>
        private string _totalEndpoints;

        /// <summary>
        /// Gets or sets the total endpoints.
        /// </summary>
        /// <value>The total endpoints.</value>
        public string TotalEndpoints
        {
            get { return _totalEndpoints; }
            set
            {
                _totalEndpoints = value;
                OnPropertyChanged("TotalEndpoints");
            }
        }

        #endregion
        #region Private Function
        /// <summary>
        /// Called when [tab selected].
        /// </summary>
        /// <param name="btn">The BTN.</param>
        private void OnTabSelected(Button btn)
        {
            if (btn == null || ApplicationContext.NodesSelected == null || ApplicationContext.NodesSelected.Count == 0)
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

            switch (btn.Content.ToString())
            {
                case UIConstant.ColorCodes:
                    PageNavigatorHelper.Switch(new ColorCodePage(folders[0]));
                    break;
                case UIConstant.Endpoints:
                    var fids = folders.Select(e => e.NodeId).ToList();
                    var eids = endpoints.Select(e => e.NodeId).ToList();
                    if (!PageNavigatorHelper.IsCurrent<EndPointListPage>())
                        PageNavigatorHelper.Switch(new EndPointListPage());
                    var viewModel =
                        PageNavigatorHelper.GetMainContentViewModel<GroupViewModel>();

                    viewModel.SetParamsValueForDirectory(fids, eids,
                        ApplicationContext.SearchText, false, Guid.NewGuid(), "");
                    viewModel.GetData();
                    break;
                case UIConstant.DirectoryAssignment:
                    if (!PageNavigatorHelper.IsCurrent<DirectoryAssignmentPage>())
                    {
                        var assignmentPage = new DirectoryAssignmentPage();
                        PageNavigatorHelper.Switch(assignmentPage);
                    }
                    break;
				case UIConstant.TaskListPage:
					if (!PageNavigatorHelper.IsCurrent<TaskListPage>())
					{
						var taskListPage = new TaskListPage();
						PageNavigatorHelper.Switch(taskListPage);
					}
					break;
                case UIConstant.GroupPolicies:
                    if (!PageNavigatorHelper.IsCurrent<PolicyEnhancementPage>())
                    {
                        PageNavigatorHelper.Switch(new PolicyEnhancementPage());
                    }
                    break;
            }
        }

        private bool CanTabSelected(Button btn)
        {
            if (btn == null || ApplicationContext.NodesSelected == null || ApplicationContext.NodesSelected.Count == 0 || ApplicationContext.IsFromLabel)
                return false;

            switch (btn.Content.ToString())
            {
                case UIConstant.ColorCodes:
                case UIConstant.DirectoryAssignment:
                //case UIConstant.GroupPolicies:
				case UIConstant.TaskListPage:
                    return ApplicationContext.NodesSelected.Count == 1;
                default:
                    return true;
            }

        }
        #endregion

        #region Public Function

        public void UpdateDirectoryHeader(int directoryId, int activeIndex = 1)
        {
	        //ActivedButtonIndex = activeIndex;
	        //ActiveButtonTab(activeIndex);
			var endplst = new List<EndPoint>();

            var endpointlist = GetDirectoryEndpoints(directoryId);
            foreach (var e in endpointlist)
            {
                if (!(endplst.Select(ep => ep.EndpointId).Contains(e.EndpointId)))
                {
                    endplst.Add(e);
                }
            }

            var dirSelected = ApplicationContext.FolderListAll.Find(r => r.FolderId == directoryId);
            var listParentId = new List<string>();
            GetPathNode(listParentId, dirSelected);
            listParentId.Reverse();
            FolderPathName = string.Join(" | ", listParentId);

            TotalEndpoints = endplst.Count == 0
                ? string.Empty
                : endplst.Count == 1 ? endplst.Count + " Endpoint" : endplst.Count + " Endpoints";
        }
        private void GetPathNode(List<string> listNode, Directory dir)
        {
            if (dir == null) return;
            listNode.Add(dir.FolderName);
            foreach (var ep in ApplicationContext.FolderListAll)
            {
                if (dir.ParentId == ep.FolderId)
                {
                    GetPathNode(listNode, ep);
                    break;
                }
            }
        }
        #endregion
    }
}
