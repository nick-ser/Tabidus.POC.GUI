using System;
using System.Windows.Threading;

namespace Tabidus.POC.GUI.Common
{
    public delegate void TimeChangedEventHandler();

    public class AppTimer
    {
        #region Properties

        #region Private

        private static DispatcherTimer appTimer;

        #endregion Private

        #region Public

        public static event TimeChangedEventHandler AppTimeChanged;

        #endregion Public

        #endregion Properties

        #region Functions

        #region Private

        private static void appTimer_Tick(object sender, EventArgs e)
        {
            if (AppTimeChanged != null) AppTimeChanged();
        }

        #endregion Private

        #region Public

        public static void StartAppTimer(TimeChangedEventHandler timeChanged)
        {
            if (appTimer == null)
            {
                var appInterval = Functions.GetConfig("APP_INTERVAL", 300);
                appTimer = new DispatcherTimer();
                appTimer.Interval = new TimeSpan(0, 0, appInterval); //appInterval Second for display as Clock Counter
                appTimer.Tick += appTimer_Tick;
                appTimer.Start();
            }

            //Attach the handle
            AppTimeChanged += timeChanged;
        }

        public static void StopAppTimer()
        {
            if (appTimer != null)
            {
                appTimer.Stop();
                appTimer = null;
            }
        }

        #endregion Public

        #endregion Functions
    }
}