using System.Windows.Controls;
using Tabidus.POC.Common;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.Endpoint;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    /// Interaction logic for ColorCodePage.xaml
    /// </summary>
    public partial class ColorCodePage : Page
    {
        public ColorCodePage()
        {
            InitializeComponent();
            Model = new ColorCodePageViewModel(this);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorCodePage"/> class.
        /// </summary>
        public ColorCodePage(DirectoryNode directoryNode)
        {
            InitializeComponent();
            Model = new ColorCodePageViewModel(directoryNode,this);
            //GroupHeaderElement.UpdateHeader(6, ApplicationContext.FolderPathName, ApplicationContext.TotalEndpoint);//Text, 
        }

        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <value>The model.</value>
        public ColorCodePageViewModel Model
        {
            set
            {
                DataContext = value;
            }
        }
    }
}
