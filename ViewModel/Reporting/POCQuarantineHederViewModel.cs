using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.UserControls.Reporting;

namespace Tabidus.POC.GUI.ViewModel.Reporting
{
    public class POCQuarantineHederViewModel : ViewModelBase
    {
        private POCQuarantineHeaderElement _view;
        public ICommand RecoverCommand { get; private set; }
        public ICommand DeleteCommand { get; private set; }

        public POCQuarantineHederViewModel(POCQuarantineHeaderElement view)
        {
            RecoverCommand = new RelayCommand<Button>(OnRecover, CanRecover);
            DeleteCommand = new RelayCommand<Button>(OnDelete, CanDelete);
            _view = view;
            _view.Loaded += ViewOnLoaded;
        }

        private void ViewOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
        }

        private void OnRecover(Button btn)
        {
        }

        private bool CanRecover(Button btn)
        {
            return true;
        }

        private void OnDelete(Button btn)
        {
        }

        private bool CanDelete(Button btn)
        {
            return true;
        }
    }
}
