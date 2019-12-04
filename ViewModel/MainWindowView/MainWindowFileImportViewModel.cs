using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.Endpoint;

namespace Tabidus.POC.GUI.ViewModel.MainWindowView
{
    public partial class MainWindowViewModel
    {
        partial void MainWindowFileImportViewModel()
        {
            ImportFromFileCommand = new RelayCommand(OnImportFromFileExecute, CanImportFromFile);
        }
        #region Private methods
        /// <summary>
        ///     Moving folders and endpoints
        /// </summary>
        public void MoveDirectoriesAndEndpointsAction(MoveFoldersAndEndpointsInputArgs mags)
        {
            var moveNodeBackgroundWorker = new BackgroundWorker();
            moveNodeBackgroundWorker.DoWork += MoveNodeBackgroundWorker_DoWork;
            moveNodeBackgroundWorker.RunWorkerCompleted += MoveNodeBackgroundWorker_RunWorkerCompleted;
            moveNodeBackgroundWorker.RunWorkerAsync(mags);
        }
        private void OnAddFileExecute(object pars)
        {
            //Show add panel
            _view.BdAddButton.Visibility = Visibility.Visible;
            _view.menugrid.Visibility = Visibility.Visible;

        }

        private void OnMoveFileExecute(object pars)
        {
            var moveTreeViewModel = _view.MoveTargetDialogView.DataContext as MoveTreeNodeViewModel;
            if (moveTreeViewModel != null)
            {
                moveTreeViewModel.MakeTree();
            }
            ApplicationContext.NodeTargetId = 0;
            //Show choose target to move dialog
            _view.ShowMoveDialog();
        }

        private bool CanImportFromFile(object pars)
        {
            try
            {
                if (!PageNavigatorHelper.GetRightElementViewModel().DirectoryPushed)
                {
                    return false;
                }
                return ApplicationContext.NodesSelected != null
                       && ApplicationContext.NodesSelected.Count == 1
                       && ApplicationContext.NodesSelected[0].IsFolder;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        private bool CanMoveAndDeleteDirectory(object pars)
        {
            try
            {
                if (!PageNavigatorHelper.GetRightElementViewModel().DirectoryPushed)
                {
                    return false;
                }
                return ApplicationContext.NodesSelected != null
                       && ApplicationContext.NodesSelected.Count >= 1;
            }
            catch (Exception)
            {
                return false;
            }
            
        }
        /// <summary>
        ///     Move folders and enpoints into another folder
        /// </summary>
        /// <param name="margs">parameters that contain folderId list, endpoidId list and taget forderId</param>
        private void MoveDirectoriesAndEndpoints(MoveFoldersAndEndpointsInputArgs margs)
        {
            try
            {
                Logger.Info("Starting move directory and endpoint");

                using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
                {
                    var data = JsonConvert.SerializeObject(margs);
                    sc.MoveFoldersAndEndpoints(EncryptionHelper.EncryptString(data, KeyEncryption));
                    
                    //refresh tree data
                    _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        PageNavigatorHelper.GetRightElementViewModel().SearchText = string.Empty;
                        ApplicationContext.SearchText = string.Empty;
                        PageNavigatorHelper._MainWindow.RightTreeElement.BackButton.Visibility = Visibility.Collapsed;
                        ApplicationContext.DirSearched = false;
                        var dn = new DirectoryNode { IsFolder = true, NodeId = margs.TargerFolderId };
                        MakeTreeNode(dn, false, true);
                    }));
                }
                
                Logger.Info("Ended move directory and endpoint");
            }
            catch (Exception ex)
            {                
                //refresh tree data
                _view.Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    MakeTreeNode(0);
                    var messageDialog = _view.MessageDialogContentControl.Content as MessageDialog;
                    messageDialog.ShowMessageDialog(
                        "Cannot move this tree note due to exception occured, please see the log file under the Logs for more information",
                        "Message");
                }));
                Logger.Error(ex.StackTrace);
            }
        }

        private void OnImportFromFileExecute(object pars)
        {
                _view.menugrid.Visibility = Visibility.Hidden;
            _view.BdAddButton.Visibility = Visibility.Collapsed;
            //Show Import From File dialog
            //_view.ShowImportFileDialog(ApplicationContext.NodeId);

            //Navigation to Import Page
            var importFilePage = new ImportFilePage(ApplicationContext.NodesSelected.Count > 0 ? ApplicationContext.NodesSelected[0].NodeId : 1);
            var leftModel = PageNavigatorHelper.GetLeftElementViewModel();
            if (leftModel != null)
            {
                if(NavigationIndex != (int)NavigationIndexes.Endpoint && NavigationIndex != (int)NavigationIndexes.Label)
                    leftModel.ChangeEndpointNavigationState();
            }
            PageNavigatorHelper.Switch(importFilePage);
        }
        #endregion
    }
}
