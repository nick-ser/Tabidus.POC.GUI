using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Endpoint;

namespace Tabidus.POC.GUI.UserControls.Endpoint
{
    /// <summary>
    /// Interaction logic for GroupHeaderElement.xaml
    /// </summary>
    public partial class GroupHeaderElement : UserControl
    {
        /// <summary>
        /// The _model
        /// </summary>
        private GroupHeaderViewModel _model;
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupHeaderElement"/> class.
        /// </summary>
        public GroupHeaderElement()
        {
            InitializeComponent();
            Model = new GroupHeaderViewModel(this);
            //isnotification.ItemsSource = "NOTIFICATION";
            //istask.ItemsSource = "TASKS";
            //ispolicy.ItemsSource = "POLICIES";
        }

        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <value>The model.</value>
        public GroupHeaderViewModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                DataContext = _model;
            }
        }

        /// <summary>
        /// Updates the header.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="header">The header.</param>
        /// <param name="description">The description.</param>
        public void UpdateHeader(int index, string header, string description = "")
        {
            _model.FolderPathName = header;
            _model.ActivedButtonIndex = index;
            _model.TotalEndpoints = description;
        }

        private void OnDirectoryAssignmentClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            //if (button != null)
            //{
            //    button.ContextMenu.PlacementTarget = button;
            //    button.ContextMenu.IsOpen = true;
            //}
        }

	    private void TaskMenu_OnClick(object sender, RoutedEventArgs e)
	    {
			var button = sender as Button;
			if (button != null)
			{
                //button.SnapsToDevicePixels = true;
                //button.OverridesDefaultStyle = true;
				button.ContextMenu.PlacementTarget = button;
				button.ContextMenu.IsOpen = true;
			}
		}

        private void Border_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {

        }
    }
}