using System.Windows;
using System.Windows.Controls;
using Infragistics.Windows.DataPresenter;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ViewModel.Software;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for SoftwarePage.xaml
    /// </summary>
    public partial class SoftwarePage : Page
    {
        public SoftwarePage()
        {
            InitializeComponent();
            var softwareColumnWidth = Functions.GetConfig("SOFTWARE_VIEW_WIDTH_KEY", "");
            ApplicationContext.SoftwareNameWidth = Functions.GetColumnWidth("Name", softwareColumnWidth);
            ApplicationContext.SoftwareVersionWidth = Functions.GetColumnWidth("Version", softwareColumnWidth);
            ApplicationContext.SoftwareCommentWidth = Functions.GetColumnWidth("Comment", softwareColumnWidth);
            ApplicationContext.SoftwareExecutableWidth = Functions.GetColumnWidth("Executable", softwareColumnWidth);
            ApplicationContext.SoftwareParametersWidth = Functions.GetColumnWidth("Parameters", softwareColumnWidth);
            ApplicationContext.SoftwareSizeWidth = Functions.GetColumnWidth("Size", softwareColumnWidth);
            
            SoftwareDataGrid.FieldLayouts.DataPresenter.GroupByAreaLocation = GroupByAreaLocation.None;
            Model = new SoftwareViewModel(this);
			Model.Refresh();
        }

        public SoftwareViewModel Model
        {
            get { return DataContext as SoftwareViewModel; }
            set { DataContext = value; }
        }

        private void Field_Resized(object sender, SizeChangedEventArgs e)
        {
            Field field = (sender as LabelPresenter).Field;
            if (field.Name == "LastSync")
            {
                if ((int)field.LabelWidthResolved != 130)
                    SetColumnWidth(field.Name, (int)field.LabelWidthResolved);
                else
                {
                    field.Width = new FieldLength(GetColumnWidth(field.Name));
                }
            }
            else
            {
                if (field.Width != null)
                {
                    SetColumnWidth(field.Name, (int)field.LabelWidthResolved);
                }
            }

        }

        private void Field_Loaded(object sender, RoutedEventArgs e)
        {
            Field field = (sender as LabelPresenter).Field;
            field.Width = new FieldLength(GetColumnWidth(field.Name));
        }


        private int GetColumnWidth(string id)
        {
            switch (id)
            {
                case "Name":
                    return ApplicationContext.SoftwareNameWidth;
                case "Version":
                    return ApplicationContext.SoftwareVersionWidth;
                case "Comment":
                    return ApplicationContext.SoftwareCommentWidth;
                case "FileName":
                    return ApplicationContext.SoftwareExecutableWidth;
                case "Parameters":
                    return ApplicationContext.SoftwareParametersWidth;
                case "Size":
                    return ApplicationContext.SoftwareSizeWidth;
                default:
                    return 100;
            }
        }

        private void SetColumnWidth(string id, int width)
        {
            switch (id)
            {
                case "Name":
                    ApplicationContext.SoftwareNameWidth = width;
                    break;
                case "Version":
                    ApplicationContext.SoftwareVersionWidth = width;
                    break;
                case "Comment":
                    ApplicationContext.SoftwareCommentWidth = width;
                    break;
                case "FileName":
                    ApplicationContext.SoftwareExecutableWidth = width;
                    break;
                case "Parameters":
                    ApplicationContext.SoftwareParametersWidth = width;
                    break;
                case "Size":
                    ApplicationContext.SoftwareSizeWidth = width;
                    break;
                default:
                    break;
            }
            var widthConfigText =
                string.Format(
                    "Name:{0};Version:{1};Comment:{2};Executable:{3};Parameters:{4};Size:{5}", ApplicationContext.SoftwareNameWidth, ApplicationContext.SoftwareVersionWidth, ApplicationContext.SoftwareCommentWidth, ApplicationContext.SoftwareExecutableWidth, ApplicationContext.SoftwareParametersWidth, ApplicationContext.SoftwareSizeWidth);
            Functions.WriteToConfig("SOFTWARE_VIEW_WIDTH_KEY", widthConfigText);
        }
    }
}