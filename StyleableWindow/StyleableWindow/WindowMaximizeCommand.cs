using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WpfStyleableWindow.StyleableWindow
{
    public class WindowMaximizeCommand : ICommand
    {
        bool i = false;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var window = parameter as Window;
            
            if (window != null)
            {
                if (i== true)
                {
                    //i = 1;
                    //window.Height = 500;
                    //window.Width = 1000;
                    //double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                    //double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                    //double windowWidth = window.Width;
                    //double windowHeight = window.Height;
                    //window.Left = (screenWidth / 2) - (windowWidth / 2);
                    //window.Top = (screenHeight / 2) - (windowHeight / 2);
                    //window.WindowState = WindowState.Normal;
                    window.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                    window.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
                    window.Left = 0;
                    window.Top = 0;
                    i = false;
                }
                else
                {
                    i = true;
                    window.Height = 500;
                    window.Width = 1000;
                    double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
                    double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
                    double windowWidth = window.Width;
                    double windowHeight = window.Height;
                    window.Left = (screenWidth / 2) - (windowWidth / 2);
                    window.Top = (screenHeight / 2) - (windowHeight / 2);
                    //window.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                    //window.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
                    //window.Left = 0;
                    //window.Top = 0;
                    // window.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                }
            }
        }
    }
}