using System.Windows;
using System.Windows.Controls;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Discovery;

namespace Tabidus.POC.GUI.UserControls.Discovery
{
    /// <summary>
    ///     Interaction logic for NetworkButtonElement.xaml
    /// </summary>
    public partial class NetworkButtonElement : UserControl
    {
        public NetworkButtonElement()
        {
            InitializeComponent();
            Model = new NetworkButtonViewModel();
        }

        public NetworkButtonViewModel Model
        {
            get { return DataContext as NetworkButtonViewModel; }
            set { DataContext = value; }
        }

        private void BtnNetwork_OnClick(object sender, RoutedEventArgs e)
        {
            var pnlchr = (this.Parent as StackPanel).Children;
            //find all button in networkd panel to reset style
            foreach (var ch in pnlchr)
            {
                if (ch.GetType() == typeof(NetworkButtonElement))
                {
                    ((NetworkButtonElement)ch).BtnNetwork.Style = FindResource("NetworkButton") as Style;
                }
            }

            //set style for button that clicked
            ((Button)sender).Style = FindResource("NetworkButtonPushed") as Style;
        }

        public void SetActived()
        {
            BtnNetwork.Style = FindResource("NetworkButtonPushed") as Style;
        }
    }
}