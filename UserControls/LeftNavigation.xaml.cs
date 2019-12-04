using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.GUI.ViewModel;

namespace Tabidus.POC.GUI.UserControls
{
    /// <summary>
    ///     Interaction logic for LeftNavigation.xaml
    /// </summary>
    public partial class LeftNavigation : UserControl
    {
        public LeftNavigation()
        {
            InitializeComponent();
            DataContext = new LeftNavigationViewModel(this);
            Loaded += LeftNavigation_Loaded;
            icTodoList.ItemsSource = "ENDPOINT";

        }

        private void LeftNavigation_Loaded(object sender, RoutedEventArgs e)
        {
            BtnEndPoint.Tag = 1;
            BtnEndPoint.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.MAINBACKGROUND_HOVER_COLOR));
            BtnEndPoint.BorderThickness = new Thickness(0, 0, 4, 0);
            BtnEndPoint.BorderBrush = (Brush)(new BrushConverter().ConvertFrom("#1dabed"));
            BtnEndPoint.Foreground = Brushes.White;
            endpointimg.Visibility = Visibility.Collapsed;
            endpointimg_hover.Visibility = Visibility.Visible;
        }

        public void SetActiveButton(string btnName)
        {
            var pnlchr = PnlNavigator.Children;
            //find all button in navigation panel to reset style
            foreach (var ch in pnlchr)
            {
                if (ch.GetType() == typeof(Button))
                {
                    ((Button)ch).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.MAINBACKGROUND_COLOR));
                    ((Button)ch).Tag = 0;
                }
                //find sub button
                if (ch.GetType() == typeof(StackPanel))
                {
                    var spnl = ((StackPanel)ch).Children;
                    foreach (var sch in spnl)
                    {
                        if (sch.GetType() == typeof(StackPanel))
                        {
                            var sspnl = ((StackPanel)sch).Children;
                            foreach (var ssch in sspnl)
                            {
                                if (ssch.GetType() == typeof(Button))
                                {
                                    ((Button)ssch).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.SUBBUTTON_COLOR));
                                    ((Button)ssch).Tag = 0;
                                }
                            }
                        }

                    }
                }
            }
            var btn = FindName(btnName) as Button;
            if (btn.Name.Contains("Sub"))
            {
                btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.SUBBUTTON_HOVER_COLOR));
            }
            else
            {
                btn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.MAINBACKGROUND_HOVER_COLOR));
                btn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#fff"));
                //btn.BorderThickness = new Thickness(0, 0, 7, 0);
                //btn.BorderBrush = Brushes.Blue;
                btn.BorderThickness = new Thickness(0, 0, 4, 0);
                btn.BorderBrush = (Brush)(new BrushConverter().ConvertFrom("#1dabed"));
                btn.Foreground = Brushes.White;
            }

            btn.Tag = 1;
        }

        /// <summary>
        /// setting style for button when it clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var pnlchr = PnlNavigator.Children;
            //find all button in navigation panel to reset style
            foreach (var ch in pnlchr)
            {
                if (ch.GetType() == typeof(Button))
                {
                    unhoverButton((Button)ch);
                }
                //find sub button
                if (ch.GetType() == typeof(StackPanel))
                {
                    var spnl = ((StackPanel)ch).Children;
                    foreach (var sch in spnl)
                    {
                        if (sch.GetType() == typeof(StackPanel))
                        {
                            var sspnl = ((StackPanel)sch).Children;
                            foreach (var ssch in sspnl)
                            {
                                if (ssch.GetType() == typeof(Button))
                                {
                                    ((Button)ssch).Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8e8f98"));
                                    ((Button)ssch).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.SUBBUTTON_COLOR));
                                    ((Button)ssch).Tag = 0;
                                }
                            }
                        }

                    }
                }
            }

            //set style for button that clicked
            if (sender.GetType() == typeof(Button))
            {
                hoverButton((Button)sender);
            }
        }

        private void hoverButton(Button tmpBtn)
        {
            tmpBtn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.MAINBACKGROUND_HOVER_COLOR));
            tmpBtn.Tag = 1;
            //tmpBtn.BorderThickness = new Thickness(0, 0, 7, 0);
            //tmpBtn.BorderBrush = Brushes.Blue;
            tmpBtn.BorderThickness = new Thickness(0, 0, 4, 0);
            tmpBtn.BorderBrush = (Brush)(new BrushConverter().ConvertFrom("#1dabed"));
            tmpBtn.Foreground = Brushes.White;
            //To activate icon of selected buttons
            if (tmpBtn.Name == "BtnEndPoint")
            {
                endpointimg.Visibility = Visibility.Collapsed;
                endpointimg_hover.Visibility = Visibility.Visible;
            }
            if (tmpBtn.Name == "BtnDiscovery")
            {
                discoveryimg.Visibility = Visibility.Collapsed;
                discoveryimg_hover.Visibility = Visibility.Visible;
            }
            if (tmpBtn.Name == "BtnSoftware")
            {
                softwareimg.Visibility = Visibility.Collapsed;
                softwareimg_hover.Visibility = Visibility.Visible;
            }
            if (tmpBtn.Name == "BtnLicense")
            {
                licenseimg.Visibility = Visibility.Collapsed;
                licenseimg_hover.Visibility = Visibility.Visible;
            }
            if (tmpBtn.Name == "BtnPolicy")
            {
                policyimage.Visibility = Visibility.Collapsed;
                policyimage_hover.Visibility = Visibility.Visible;
            }
            if (tmpBtn.Name == "BtnReporting")
            {
                reportingimg.Visibility = Visibility.Collapsed;
                reportingimg_hover.Visibility = Visibility.Visible;
            }
            if (tmpBtn.Name == "BtnNotification")
            {
                notificationimage.Visibility = Visibility.Collapsed;
                notificationimage_hover.Visibility = Visibility.Visible;
            }
            if (tmpBtn.Name == "BtnSetting")
            {
                settingimg.Visibility = Visibility.Collapsed;
                settingimg_hover.Visibility = Visibility.Visible;
            }
        }

        private void unhoverButton(Button tmpBtn)
        {
            tmpBtn.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.MAINBACKGROUND_COLOR));
            tmpBtn.Tag = 0;
            tmpBtn.BorderThickness = new Thickness(0);
            tmpBtn.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#8e8f98"));
            if (tmpBtn.Name == "BtnEndPoint")
            {
                endpointimg.Visibility = Visibility.Visible;
                endpointimg_hover.Visibility = Visibility.Collapsed;
            }
            if (tmpBtn.Name == "BtnDiscovery")
            {
                discoveryimg.Visibility = Visibility.Visible;
                discoveryimg_hover.Visibility = Visibility.Collapsed;
            }
            if (tmpBtn.Name == "BtnSoftware")
            {
                softwareimg.Visibility = Visibility.Visible;
                softwareimg_hover.Visibility = Visibility.Collapsed;
            }
            if (tmpBtn.Name == "BtnLicense")
            {
                licenseimg.Visibility = Visibility.Visible;
                licenseimg_hover.Visibility = Visibility.Collapsed;
            }
            if (tmpBtn.Name == "BtnPolicy")
            {
                policyimage.Visibility = Visibility.Visible;
                policyimage_hover.Visibility = Visibility.Collapsed;
            }
            if (tmpBtn.Name == "BtnReporting")
            {
                reportingimg.Visibility = Visibility.Visible;
                reportingimg_hover.Visibility = Visibility.Collapsed;
            }
            if (tmpBtn.Name == "BtnNotification")
            {
                notificationimage.Visibility = Visibility.Visible;
                notificationimage_hover.Visibility = Visibility.Collapsed;
            }
            if (tmpBtn.Name == "BtnSetting")
            {
                settingimg.Visibility = Visibility.Visible;
                settingimg_hover.Visibility = Visibility.Collapsed;
            }
        }


        /// <summary>
        /// setting style for sub button when it clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var pnlchr = PnlNavigator.Children;
            //find all button in navigation panel to reset style
            foreach (var ch in pnlchr)
            {
                if (ch.GetType() == typeof(Button))
                {
                    ((Button)ch).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.MAINBACKGROUND_COLOR));
                    ((Button)ch).Tag = 0;
                }
                var a = ((Button)sender).Content;
                //find sub button
                if (ch.GetType() == typeof(StackPanel))
                {
                    var spnl = ((StackPanel)ch).Children;
                    foreach (var sch in spnl)
                    {
                        if (sch.GetType() == typeof(StackPanel))
                        {
                            var sspnl = ((StackPanel)sch).Children;
                            foreach (var ssch in sspnl)
                            {
                                if (ssch.GetType() == typeof(Button))
                                {
                                    ((Button)ssch).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.SUBBUTTON_COLOR));
                                    ((Button)ssch).Tag = 0;
                                    ((Button)ssch).Foreground = (Brush)(new BrushConverter().ConvertFrom("#8e8f98"));
                                }
                            }
                        }

                    }
                }
            }

            //set style for button that clicked
            ((Button)sender).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.SUBBUTTON_HOVER_COLOR));
            ((Button)sender).Tag = 1;
            ((Button)sender).Foreground = (Brush)(new BrushConverter().ConvertFrom("#FFF"));

        }

        /// <summary>
        /// setting hover button style
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Button) sender).Background = (SolidColorBrush) (new BrushConverter().ConvertFrom(CommonConstants.MAINBACKGROUND_HOVER_COLOR));
            
        }

        /// <summary>
        /// setting hover sub button style
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubButton_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Button)sender).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.SUBBUTTON_HOVER_COLOR));
        }

        /// <summary>
        /// setting mouse out button style
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEndPoint_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (((Button) sender).Tag == null || (((Button) sender).Tag != null && (int) ((Button) sender).Tag == 0))
            {
                ((Button) sender).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.MAINBACKGROUND_COLOR));
            }
        }

        /// <summary>
        /// setting mouse out button style
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubBtnEndPoint_OnMouseLeave(object sender, MouseEventArgs e)
        {
            if (((Button)sender).Tag == null || (((Button)sender).Tag != null && (int)((Button)sender).Tag == 0))
            {
                ((Button)sender).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.SUBBUTTON_COLOR));
            }
        }

        private void OnDirectoryAssignmentClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.IsOpen = true;
            }
            var pnlchr = PnlNavigator.Children;
            //find all button in navigation panel to reset style
            foreach (var ch in pnlchr)
            {
                if (ch.GetType() == typeof(Button))
                {
                    ((Button)ch).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.MAINBACKGROUND_COLOR));
                    ((Button)ch).Tag = 0;
                }
                //find sub button
                if (ch.GetType() == typeof(StackPanel))
                {
                    var spnl = ((StackPanel)ch).Children;
                    foreach (var sch in spnl)
                    {
                        if (sch.GetType() == typeof(StackPanel))
                        {
                            var sspnl = ((StackPanel)sch).Children;
                            foreach (var ssch in sspnl)
                            {
                                if (ssch.GetType() == typeof(Button))
                                {
                                    ((Button)ssch).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.SUBBUTTON_COLOR));
                                    ((Button)ssch).Tag = 0;
                                }
                            }
                        }

                    }
                }
            }

            //set style for button that clicked
            ((Button)sender).Background = (SolidColorBrush)(new BrushConverter().ConvertFrom(CommonConstants.SUBBUTTON_HOVER_COLOR));
            ((Button)sender).Tag = 1;
        }
    }
}