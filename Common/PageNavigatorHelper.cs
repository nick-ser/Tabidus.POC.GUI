using System;
using System.Windows;
using System.Windows.Controls;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.GUI.ViewModel.MainWindowView;

namespace Tabidus.POC.GUI.Common
{
    /// <summary>
    ///     This class uses to manage pages of main content
    ///     every times we want to switch the main content page we use this class
    /// </summary>
    public static class PageNavigatorHelper
    {
        public static MainWindow _MainWindow;
        private static FrameworkElement _currentPage;
        private static FrameworkElement _previousPage;

        /// <summary>
        ///     Switch new page
        /// </summary>
        /// <param name="newPage"></param>
        public static void Switch(Page newPage)
        {
            if (_currentPage != null)
            {
                _previousPage = _currentPage;
            }

            _currentPage = newPage;
            if (_previousPage == null)
            {
                _previousPage = _currentPage;
            }
            _MainWindow.FrmMainContent.Navigate(newPage);
        }

        public static void Switch(UserControl newPage)
        {
            if (_currentPage != null)
            {
                _previousPage = _currentPage;
            }
            _MainWindow.FrmMainContent.Navigate(newPage);
            _currentPage = newPage;
        }

        /// <summary>
        ///     Switch previous page
        /// </summary>
        public static void SwitchBack()
        {
            Switch(_previousPage as Page);
        }

        public static T GetMainContentViewModel<T>()
        {
            var page = _currentPage as Page;
            if (page != null && page.DataContext is T)
                return (T)page.DataContext;
            return default(T);
        }

        public static bool IsCurrent<T>() where T : Page
        {
            return _currentPage is T;
        }
        public static T GetMainContent<T>() where T:Page
        {
            return _currentPage as T;
        }

        public static RightTreeViewModel GetRightElementViewModel()
        {
            return _MainWindow.RightTreeElement.DataContext as RightTreeViewModel;
        }

        public static LeftNavigationViewModel GetLeftElementViewModel()
        {
            return _MainWindow.LeftNavigation.DataContext as LeftNavigationViewModel;
        }

        public static MainWindowViewModel GetMainModel()
        {
            return _MainWindow.DataContext as MainWindowViewModel;
        }
    }
}