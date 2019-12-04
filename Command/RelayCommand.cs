using System;
using System.Windows.Input;

namespace Tabidus.POC.GUI.Command
{
    //comment the purpose of RelayCommand class
    /// <summary>
    /// Class RelayCommand.
    /// </summary>
    public class RelayCommand : RelayCommand<object>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="methodToExecute">The method to execute.</param>
        /// <param name="canExecuteEvaluator">The can execute evaluator.</param>
        public RelayCommand(Action<object> methodToExecute, Func<object, bool> canExecuteEvaluator)
            : base(methodToExecute, canExecuteEvaluator)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand"/> class.
        /// </summary>
        /// <param name="methodToExecute">The method to execute.</param>
        public RelayCommand(Action<object> methodToExecute)
            : base(methodToExecute)
        {
        }
    }

    /// <summary>
    /// Class RelayCommand.
    /// </summary>
    /// <typeparam name="TParam">The type of the t parameter.</typeparam>
    public class RelayCommand<TParam> : ICommand
    {
        /// <summary>
        /// The _method to execute
        /// </summary>
        private readonly Action<TParam> _methodToExecute;
        /// <summary>
        /// The _can execute evaluator
        /// </summary>
        private readonly Func<TParam, bool> _canExecuteEvaluator;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{TParam}"/> class.
        /// </summary>
        /// <param name="methodToExecute">The method to execute.</param>
        /// <param name="canExecuteEvaluator">The can execute evaluator.</param>
        public RelayCommand(Action<TParam> methodToExecute, Func<TParam, bool> canExecuteEvaluator)
        {
            _methodToExecute = methodToExecute;
            _canExecuteEvaluator = canExecuteEvaluator;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommand{TParam}"/> class.
        /// </summary>
        /// <param name="methodToExecute">The method to execute.</param>
        public RelayCommand(Action<TParam> methodToExecute)
            : this(methodToExecute, null)
        {
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            if (_canExecuteEvaluator == null)
            {
                return true;
            }
            else
            {
                return _canExecuteEvaluator.Invoke((TParam)parameter);
            }
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            _methodToExecute.Invoke((TParam)parameter);
        }
    }
}
