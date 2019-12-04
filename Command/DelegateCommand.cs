using System;
using System.Windows.Input;

namespace Tabidus.POC.GUI.Command
{
    public delegate void ParameterAction(Object param);

    public class DelegateCommand : ICommand
    {
        public Func<bool> CanExecuteFunc { get; set; }
        public ParameterAction CommandAction { get; set; }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public void Execute(object parameter)
        {
            CommandAction(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}