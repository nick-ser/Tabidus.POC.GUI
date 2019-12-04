using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Infragistics.Controls.Interactions;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.MainWindowView;

namespace Tabidus.POC.GUI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //this.Background = new ImageBrush(new System.Windows.Media.Imaging.BitmapImage(new Uri(@"..\..\Images\bg.png", UriKind.RelativeOrAbsolute)));
            ApplicationContext.GridRightOriginalWidth = 230;
            ImportFolderComputerView = new ImportFolderComputerDialog();
            MessageDialogView = new MessageDialog();
            MoveTargetDialogView = new MoveTargetDialog();
            NeighborhoodWatchMoveTargetDialogView = new NeighborhoodWatchMoveTargetDialog();
            Loaded += MainWindow_Loaded;
        }

        public RightTreeViewModel RightTreeViewModel
        {
            get { return RightTreeElement.Model; }
        }

        /// <summary>
        ///     Main windows form loaded event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var mainWindowViewModel = new MainWindowViewModel(this);
            DataContext = mainWindowViewModel;
            FrmMainContent.Navigating += FrmMainContent_Navigating;
            var fontFamily = new FontFamily(CommonConstants.FONT_FAMILY);
            this.FontFamily = fontFamily;

        }
        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            //this.MouseLeftButtonDown += delegate { DragMove(); };
            this.Left = SystemParameters.WorkArea.Left;
            this.Top = SystemParameters.WorkArea.Top;
            this.Height = SystemParameters.WorkArea.Height;
            this.Width = SystemParameters.WorkArea.Width;
        }
        /// <summary>
        ///     Navigating page event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMainContent_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                e.Cancel = true;
            }
            if (e.NavigationMode == NavigationMode.Refresh)
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// hidden add panel when click outside it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Grid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source != BtnAdd && e.Source != BtnAddFolder && e.Source != BtnAddComputer && e.Source != BtnFromFile && e.Source != BtnFromLDAP)
            {
                BdAddButton.Visibility = Visibility.Hidden;
                menugrid.Visibility = Visibility.Hidden;
            }
        }


        public ImportFolderComputerDialog ImportFolderComputerView
        {
            get
            {
                return ImportFolderComputerContentControl.Content as ImportFolderComputerDialog;
            }
            set
            {
                ImportFolderComputerContentControl.Content = value;
            }
        }

        public MessageDialog MessageDialogView
        {
            get
            {
                return MessageDialogContentControl.Content as MessageDialog;
            }
            set
            {
                MessageDialogContentControl.Content = value;
            }
        }

        public MoveTargetDialog MoveTargetDialogView
        {
            get { return MoveTargetDialogContentControl.Content as MoveTargetDialog; }
            set { MoveTargetDialogContentControl.Content = value; }
        }
        public NeighborhoodWatchMoveTargetDialog NeighborhoodWatchMoveTargetDialogView
        {
            get { return NeighborhoodWatchMoveDialogContentControl.Content as NeighborhoodWatchMoveTargetDialog; }
            set { NeighborhoodWatchMoveDialogContentControl.Content = value; }
        }

        public void ShowImportFileDialog(int folderId)
        {
            ImportFolderComputerView.Model.LoadFolderId(folderId);
            MessageDialogView.Close();
            ImportFolderComputerView.ShowWindow();
        }

        public void ShowMoveDialog()
        {
            MessageDialogView.Close();
            MoveTargetDialogView.ShowWindow();
        }


        private void Thumb_OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (PageNavigatorHelper._MainWindow != null)
            {
                var rvm = PageNavigatorHelper.GetRightElementViewModel();
                if (rvm != null)
                    rvm.SetNodeWidth(RightTreeElement.DataTree.Nodes, GrdRightElement.ActualWidth);
                ApplicationContext.GridRightOriginalWidth = GrdRightElement.ActualWidth;
            }
        }

        public void DynamicShowDialog(GenericXamlDialogWindow dlg, ViewModelBase viewModel = null, string header=null)
        {
            if (viewModel != null)
                dlg.DataContext = viewModel;
            DynamicDialogContentControl.Content = dlg;
            if (string.IsNullOrWhiteSpace(header))
            {
                dlg.ShowWindow();
            }
            else
            {
                dlg.ShowWindow(header);
            }
            
        }
    }
}