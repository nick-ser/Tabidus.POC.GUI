using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.GUI.Command;
using NeighborhoodHeaderElement = Tabidus.POC.GUI.UserControls.Discovery.NeighborhoodHeaderElement;

namespace Tabidus.POC.GUI.ViewModel.Discovery
{
    /// <summary>
    ///     Class GroupHeaderViewModel.
    /// </summary>
    public class NeighborhoodWatchHeaderViewModel : ViewModelBase
    {
        /// <summary>
        ///     The _view
        /// </summary>
        private static NeighborhoodHeaderElement _view;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="view"></param>
        public NeighborhoodWatchHeaderViewModel(NeighborhoodHeaderElement view)
        {
            _view = view;
            TabSelectedCommand = new RelayCommand<Button>(OnTabSelected, CanTabSelected);
        }

        #region Command

        /// <summary>
        ///     Gets the tab selected command.
        /// </summary>
        /// <value>The tab selected command.</value>
        public ICommand TabSelectedCommand { get; private set; }

        #endregion

        #region Private Function

        /// <summary>
        ///     Called when [tab selected].
        /// </summary>
        /// <param name="btn">The BTN.</param>
        private void OnTabSelected(Button btn)
        {
            if (btn == null || ApplicationContext.NodesSelected == null || ApplicationContext.NodesSelected.Count == 0)
                return;

            switch (btn.Content.ToString())
            {
                case "Color Codes":

                    break;
                case "Endpoints":

                    break;
            }
        }

        private bool CanTabSelected(Button btn)
        {
            return true;
        }

        #endregion
    }
}