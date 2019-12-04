using System.Windows;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.Common
{
    /// <summary>
    /// Class DialogHelper.
    /// </summary>
    public class DialogHelper
    {
        /// <summary>
        /// Alerts the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        public static void Alert(string message, string title = "Alert Dialog")
        {
            ShowDialog(DialogState.Alert, message, title);
        }
        /// <summary>
        /// Warnings the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        public static void Warning(string message, string title = "Warning Dialog")
        {
            ShowDialog(DialogState.Warning, message, title);
        }
        /// <summary>
        /// Errors the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        public static void Error(string message, string title = "Error Dialog")
        {
            ShowDialog(DialogState.Error, message, title);
        }
        /// <summary>
        /// Confirms the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool Confirm(string message, string title = "Confirm Dialog")
        {
            return ShowDialog(DialogState.Confirm, message, title) == MessageBoxResult.Yes;
        }
        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <param name="message">The message.</param>
        /// <param name="title">The title.</param>
        /// <returns>MessageBoxResult.</returns>
        private static MessageBoxResult ShowDialog(DialogState state, string message, string title = "Information Dialog")
        {
            var dialogView = PageNavigatorHelper._MainWindow.MessageDialogView;
            switch (state)
            {
                case DialogState.Warning:
                    dialogView.ShowMessageDialog(message, title, state);
                    return MessageBoxResult.OK; 
                case DialogState.Error:
                    dialogView.ShowMessageDialog(message, title, state);
                    return MessageBoxResult.OK; 
                case DialogState.Confirm:
                    var confirmDialog = new ConfirmDialog(message, title);
                    confirmDialog.BtnOk.Focus();
                    return confirmDialog.ShowDialog() ?? false ? MessageBoxResult.Yes : MessageBoxResult.No;
                default:
                    dialogView.ShowMessageDialog(message, title, state);
                    return MessageBoxResult.OK;
            }
        }

	    public static void ShowLoading()
	    {
			var mainViewModel = PageNavigatorHelper.GetMainModel();
			if (mainViewModel != null)
				mainViewModel.IsBusy = true;
		}

	    public static void HideLoading()
	    {
			var mainViewModel = PageNavigatorHelper.GetMainModel();
			if (mainViewModel != null)
				mainViewModel.IsBusy = false;
		}
    }
    /// <summary>
    /// Enum DialogState
    /// </summary>
    public enum DialogState
    {
        /// <summary>
        /// The alert
        /// </summary>
        Alert,
        /// <summary>
        /// The warning
        /// </summary>
        Warning,
        /// <summary>
        /// The error
        /// </summary>
        Error,
        /// <summary>
        /// The confirm
        /// </summary>
        Confirm
    }
}
