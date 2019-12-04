using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.POCAgent;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.MainWindowView;

namespace Tabidus.POC.GUI.ViewModel.Policy
{
    public class EndpointDirectoryDialogViewModel : ViewModelBase
    {
        private readonly MessageDialog _messageDialog =
            PageNavigatorHelper._MainWindow.MessageDialogContentControl.Content as MessageDialog;

        private readonly EndpointDirectoryDialog _view;
        private readonly MainWindowViewModel _mainWindowViewModel;

        private ObservableCollection<DirectoryNode> _treeDataSource;

        public EndpointDirectoryDialogViewModel(EndpointDirectoryDialog view)
        {
            _view = view;
            _mainWindowViewModel = PageNavigatorHelper.GetMainModel();
            OkCommand = new RelayCommand(OnMoveCommand, CanExecuteCommand);
        }

        public ICommand OkCommand { get; set; }

        public ObservableCollection<DirectoryNode> TreeDataSource
        {
            get { return _treeDataSource; }
            set
            {
                _treeDataSource = value;
                OnPropertyChanged("TreeDataSource");
            }
        }

        private void OnMoveCommand(object args)
        {
            var listPolicyAssign = new List<PolicyAssign>();
            var policyId = ApplicationContext.PoliciesList[0].Id;
            var policyColor = ApplicationContext.PoliciesList[0].ExpanderBackgroundColor;
            foreach (var sn in ApplicationContext.SelectedTargetNodes)
            {
                var policyAssign = new PolicyAssign
                {
                    Color = policyColor,
                    PolicyAgentId = policyId,
                    ObjectId = sn.NodeId,
                    ObjectType = sn.IsFolder ? 0 : 1
                };
                listPolicyAssign.Add(policyAssign);
            }
            _mainWindowViewModel.ShowMessage("Policy Assigning...");
            var policyAssigningBg = new BackgroundWorkerHelper();
            policyAssigningBg.AddDoWork(SaveBackgroundWorker_DoWork)
                .AddRunWorkerCompleted(OnSaveData_RunWorkerCompleted)
                .DoWork(listPolicyAssign);
        }

        private void SaveBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _view.Dispatcher.Invoke(DispatcherPriority.Render, (Action) (() =>
            {
                var listPolicyAssign = e.Argument as List<PolicyAssign>;
                var pocAgentVm = PageNavigatorHelper.GetMainContentViewModel<POCAgentViewModel>();
                pocAgentVm.AssignPolicy(listPolicyAssign);
            }));
        }

        private void OnSaveData_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _view.Close();

            _mainWindowViewModel.HideMessage();
        }

        private bool CanExecuteCommand(object args)
        {
            return ApplicationContext.SelectedTargetNodes != null && ApplicationContext.SelectedTargetNodes.Count > 0;
        }

        public void MakeTree()
        {
            var rootId = ApplicationContext.FolderListAll == null ||
                         (ApplicationContext.FolderListAll != null && ApplicationContext.FolderListAll.Count == 0)
                ? 1
                : ApplicationContext.FolderListAll.Min(r => r.FolderId);
            var rootName = ApplicationContext.FolderListAll == null ||
                           (ApplicationContext.FolderListAll != null && ApplicationContext.FolderListAll.Count == 0)
                ? "Company"
                : ApplicationContext.FolderListAll.Find(r => r.FolderId == rootId).FolderName;

            // create root, build subtree and return it
            var node = new DirectoryNode
            {
                NodeId = rootId,
                Title = rootName,
                IsFolder = true,
                NodeColor = CommonConstants.DEFAULT_TEXT_COLOR
            };
            MakeSubTree(node);
            var listNode = new ObservableCollection<DirectoryNode>();
            listNode.Add(node);
            TreeDataSource = listNode;
        }

        /// <summary>
        ///     Make sub tree using recursive
        /// </summary>
        /// <param name="parentNode"></param>
        private void MakeSubTree(DirectoryNode parentNode)
        {
            // find all children of parent node (they have parentId = id of parent node)
            var nodes = ApplicationContext.FolderListAll.Where(e => e.ParentId == parentNode.NodeId)
                .Select(e => new DirectoryNode
                {
                    NodeId = e.FolderId,
                    Title = e.FolderName,
                    IsFolder = true,
                    NodeColor = CommonConstants.DEFAULT_TEXT_COLOR,
                    NodeWidth = ApplicationContext.GridRightOriginalWidth,
                    Guid = e.Guid
                }).OrderBy(o => o.Title);
            // find all children of parent node (they have parentId = id of parent node)
            var nodes2 = ApplicationContext.EndPointListAll.Where(
                e => e.FolderId != null && e.FolderId == parentNode.NodeId)
                .Select(
                    e =>
                        new DirectoryNode
                        {
                            NodeId = e.EndpointId,
                            Title = e.SystemName,
                            IsFolder = false,
                            ComputerType = e.ComputerType == "server" ? 0 : e.ComputerType == "desktop" ? 1 : 2,
                            PowerState = e.PowerState == "offline" ? 0 : 1,
                            IsNoAgent = string.IsNullOrEmpty(e.GUIID),
                            //NodeColor = (e.PowerState == "offline" ? CommonConstants.GREEN_OFFLINE_COLOR : CommonConstants.GREEN_ONLINE_COLOR)
                            NodeColor = e.Color,
                            NodeWidth = ApplicationContext.GridRightOriginalWidth,
                            Guid = e.Guid
                        }).OrderBy(o => o.Title);
            // build subtree for each child and add it in parent's children collection
            foreach (var node in nodes)
            {
                MakeSubTree(node);
                parentNode.DirectoryNodes.Add(node);
            }
            foreach (var node in nodes2)
            {
                parentNode.DirectoryNodes.Add(node);
            }
        }
    }
}