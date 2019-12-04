using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.Task;
using EndpointViewHeaderElement = Tabidus.POC.GUI.UserControls.Endpoint.EndpointViewHeaderElement;

namespace Tabidus.POC.GUI.ViewModel.Endpoint
{
    public class EndpointViewHeaderViewModel : ViewModelBase
    {
        private readonly EndpointViewHeaderElement _view;

        public EndpointViewHeaderViewModel(EndpointViewHeaderElement view)
        {
            _view = view;
            _view.Loaded += _view_Loaded;
            TabSelectedCommand = new RelayCommand<Button>(OnTabSelected);
			AddTaskCommand = new RelayCommand(OnExecuteAddTask, CanExecuteAddTask);
		}

        #region Command

        public ICommand TabSelectedCommand { get; private set; }
		public ICommand AddTaskCommand { get; private set; }
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

		#endregion

		private void _view_Loaded(object sender, RoutedEventArgs e)
        {
	        ActiveButtonTab(ActivedButtonIndex);
        }

        #region Private Function

	    private void ActiveButtonTab(int activeIndex)
	    {
			var pnlBar = _view.PnlMenuBar.Children;
			foreach (var ctrl in pnlBar)
			{
				if (ctrl.GetType() == typeof(Button))
				{
					var btn = (Button)ctrl;
					if (btn.Name == "BtnEndpointViewMenu" + activeIndex)
					{
						btn.Style = _view.FindResource("MenuButtonHover") as Style;
						break;
					}
				}
			}
		}
        private void OnTabSelected(Button btn)
        {
            if (btn == null)
                return;
            switch (btn.Content.ToString())
            {
                case "Infos":
                    if (!PageNavigatorHelper.IsCurrent<EndPointViewPage>())
                    {
                        PageNavigatorHelper.Switch(new EndPointViewPage());
                        var viewModel =
                            PageNavigatorHelper.GetMainContentViewModel<EndpointViewModel>();
                        viewModel.SetParams(ApplicationContext.NodesSelected[0].NodeId);
                        viewModel.ReloadData();
                    }

                    break;
                case UIConstant.GroupPolicies:
                    if (!PageNavigatorHelper.IsCurrent<PolicyEnhancementPage>())
                    {
                        PageNavigatorHelper.Switch(new PolicyEnhancementPage());
                    }

                    break;
				case UIConstant.TaskListPage:
		            if (!PageNavigatorHelper.IsCurrent<TaskListPage>())
		            {
						var taskListPage = new TaskListPage();
						PageNavigatorHelper.Switch(taskListPage);
					}
					break;
                default:
                    //Endpoint
                    break;
            }
        }

        public void UpdateHeader(int activeIndex = 1)
        {
            var epd =
                ApplicationContext.EndPointListTree.Find(r => r.EndpointId == ApplicationContext.NodesSelected[0].NodeId);
	        if (epd != null)
	        {
				var comType = epd.ComputerType;

				SystemName = epd.SystemName;

				var ComputerType = string.IsNullOrWhiteSpace(epd.ID) ? "" : Functions.UppercaseFirst(comType);

				ImageHeader = string.IsNullOrEmpty(epd.ID)
					? "../../Images/logo_noagent.png"
					: string.IsNullOrWhiteSpace(ComputerType)
						? "../../Images/Notebook.png"
						: string.Format("../../Images/{0}.png", ComputerType);

				var Color = epd.Color;

				ColorCodeMessageColor = Color;
				//Reset color message
				ColorCodeMessage = "";
				TextStatusVisible = true;
				if (string.IsNullOrEmpty(epd.ID))
				{
					ColorCodeMessage = "POC Agent not installed";
				}
				else if (!string.IsNullOrEmpty(epd.LastSyncDayText))
				{
					ColorCodeMessage =
						string.Concat(epd.AgentText, "\n", epd.LastSyncDayText).Trim();
				}
				else if (string.IsNullOrEmpty(epd.LastSyncDayText)
						 && (Color == CommonConstants.GREEN_OFFLINE_COLOR
							 || Color == CommonConstants.GREEN_ONLINE_COLOR)
						 && !string.IsNullOrEmpty(epd.ID)
					)
				{
					TextStatusVisible = false;
				}
				ActiveButtonTab(activeIndex);
			}
		}

        #endregion

        #region Properties

        private string _systemName;
        private string _imageHeader;
        private string font_color;
        private string _colorCodeMessage;
        private string _colorCodeMessageColor;

        public string SystemName
        {
            get { return _systemName; }
            set
            {
                _systemName = value;
                OnPropertyChanged("SystemName");
            }
        }

        public string ImageHeader
        {
            get { return _imageHeader; }
            set
            {
                _imageHeader = value;
                OnPropertyChanged("ImageHeader");
            }
        }
        public string FontColor
        {
            get { return font_color; }
            set
            {
                font_color = value;
                OnPropertyChanged("FontColor");
            }
        }
        public string ColorCodeMessage
        {
            get { return _colorCodeMessage; }
            set
            {
                _colorCodeMessage = value;
                OnPropertyChanged("ColorCodeMessage");
            }
        }

        public string ColorCodeMessageColor
        {
            get { return _colorCodeMessageColor; }
            set
            {
                _colorCodeMessageColor = value;
                OnPropertyChanged("ColorCodeMessageColor");
            }
        }

        private int _activedButtonIndex;

        public int ActivedButtonIndex
        {
            get { return _activedButtonIndex; }
            set
            {
                _activedButtonIndex = value;
                OnPropertyChanged("ActivedButtonIndex");
            }
        }

        private bool _textStatusVisible = true;

        public bool TextStatusVisible
        {
            get { return _textStatusVisible; }
            set
            {
                _textStatusVisible = value;
                OnPropertyChanged("TextStatusVisible");
            }
        }

        #endregion
    }
}