using System.Windows;
using Infragistics.Controls.Interactions;
using Tabidus.POC.GUI.Common;

namespace Tabidus.POC.GUI.View
{
    public class GenericXamlDialogWindow : XamDialogWindow
    {
        public GenericXamlDialogWindow()
        {
            StartupPosition = StartupPosition.Center;
            IsModal = true;
            Visibility = Visibility.Collapsed;
        }
        public void ShowWindow(string headerCaption = UIConstant.MoveDialogHeaderText)
        {
            Header = headerCaption;
            Visibility = Visibility.Visible;
            Show();
        }
        public void HideWindow()
        {
            Visibility = Visibility.Hidden;
            Close();
        }
    }
}
