using System.Windows;
using System.Windows.Controls;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Endpoint;

namespace Tabidus.POC.GUI.UserControls.Endpoint
{
    /// <summary>
    ///     Interaction logic for GroupHeaderElement.xaml
    /// </summary>
    public partial class EndpointViewHeaderElement : UserControl
    {
        private EndpointViewHeaderViewModel _model;

        public EndpointViewHeaderElement()
        {
            InitializeComponent();
            Model = new EndpointViewHeaderViewModel(this);
        }

        public EndpointViewHeaderViewModel Model
        {
            get { return DataContext as EndpointViewHeaderViewModel;}
            set
            {
                _model = value;
                DataContext = _model;
            }
        }

        public void UpdateHeader(int index, string header, string description = "", string color=CommonConstants.DEFAULT_TEXT_COLOR)
        {
            _model.SystemName = header;
            _model.ActivedButtonIndex = index;
            _model.ColorCodeMessage = description;
            _model.ColorCodeMessageColor = color;
        }

	    private void OnTaskClick(object sender, RoutedEventArgs e)
	    {
			var button = sender as Button;
			if (button != null)
			{
				button.ContextMenu.PlacementTarget = button;
				button.ContextMenu.IsOpen = true;
			}
		}
    }
}