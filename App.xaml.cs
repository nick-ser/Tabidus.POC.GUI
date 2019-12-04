using log4net;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.Forms;

namespace Tabidus.POC.GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Contains("-install"))
            {
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                System.Windows.Forms.Application.Run(new Forms.ConfigurationForm());
                Shutdown();
                return;
            }

            base.OnStartup(e);

            Application.Current.DispatcherUnhandledException += OnApplicationUnhandledException;
        }
        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);
        //    var form = new ConfigurationForm();
        //    form.Show();
        //}

        /// <summary>
        /// Handles the <see>
        ///     <cref>E:ApplicationUnhandledException</cref>
        /// </see>
        ///     event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        void OnApplicationUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string errorMessage = string.Format("An application error occurred.\nPlease check whether your data is correct and repeat the action. If this error occurs again there seems to be a more serious malfunction in the application, and you better close it.\n\nError:{0})",

            e.Exception.Message + (e.Exception.InnerException != null ? "\n" +
            e.Exception.InnerException.Message : null));
            var log = LogManager.GetLogger(typeof(App));
            log.Error(errorMessage, e.Exception.InnerException ?? e.Exception);
            var mainViewModel = PageNavigatorHelper.GetMainModel();
            if (mainViewModel != null)
                mainViewModel.StartAppTimer();
#if DEBUG
            e.Handled = false;
#else
            ShowUnhandeledException(e);
#endif

        }
        /// <summary>
        /// Shows the unhandeled exception.
        /// </summary>
        /// <param name="e">The <see cref="DispatcherUnhandledExceptionEventArgs"/> instance containing the event data.</param>
        void ShowUnhandeledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            DialogHelper.Alert("Have an exception on GUI, please view log for details.");
        }
    }
}
