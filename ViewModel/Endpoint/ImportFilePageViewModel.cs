using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;
using Tabidus.POC.GUI.ViewModel.MainWindowView;

namespace Tabidus.POC.GUI.ViewModel.Endpoint
{
    /// <summary>
    /// Class ImportFilePageViewModel.
    /// </summary>
    public class ImportFilePageViewModel : PageViewModelBase
    {
        #region Private Variable
        private readonly MainWindowViewModel _mainViewModel;
        /// <summary>
        /// The _view
        /// </summary>
        private ImportFilePage _view;
        /// <summary>
        /// The _data tree import source
        /// </summary>
        private ObservableCollection<DirectoryNode> _dataTreeImportSource;
        /// <summary>
        /// The _import directory nodes
        /// </summary>
        private List<DirectoryNode> _importDirectoryNodes;
        /// <summary>
        /// The _current directory node
        /// </summary>
        private DirectoryNode _currentDirectoryNode;
        /// <summary>
        /// The _directory computer importer
        /// </summary>
        private DirectoryComputerImporter _directoryComputerImporter;
        #endregion
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportFilePageViewModel"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <param name="folderId">The folder identifier.</param>
        public ImportFilePageViewModel(ImportFilePage view, int folderId)
        {
            _mainViewModel = PageNavigatorHelper.GetMainModel();
            ImportCommand = new RelayCommand(ExecuteImport, CanImport);
            CancelCommand = new RelayCommand(ExecuteCancel);
            BrowseFileCommand = new RelayCommand(ExecuteBrowseFile, CanBrowseFile);
            _view = view;
            ReloadData(folderId);
            VisibleSimulate = Visibility.Collapsed;           
            MenuImportButtonStyle = _view.FindResource("MenuButton") as Style;
        }
        #endregion
        #region Properties

        /// <summary>
        /// Gets or sets the data tree import source.
        /// </summary>
        /// <value>The data tree import source.</value>
        public ObservableCollection<DirectoryNode> DataTreeImportSource
        {
            get
            {
                return _dataTreeImportSource;
            }
            set
            {
                _dataTreeImportSource = value;
                OnPropertyChanged("DataTreeImportSource");
            }
        }

        /// <summary>
        /// Gets or sets the impo directory nodes.
        /// </summary>
        /// <value>The impo directory nodes.</value>
        public List<DirectoryNode> ImpoDirectoryNodes
        {
            get
            {
                return _importDirectoryNodes;
            }
            set
            {
                _importDirectoryNodes = value;
                OnPropertyChanged("ImpoDirectoryNodes");
            }
        }

        /// <summary>
        /// Gets or sets the current directory node.
        /// </summary>
        /// <value>The current directory node.</value>
        public DirectoryNode CurrentDirectoryNode
        {
            get
            {
                return _currentDirectoryNode;
            }
            set
            {
                _currentDirectoryNode = value;
                OnPropertyChanged("CurrentDirectoryNode");
            }
        }
        /// <summary>
        /// The _file path
        /// </summary>
        private string _filePath;

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath
        {
            get { return _filePath; }
            set
            {
                _filePath = value; 
                OnPropertyChanged("FilePath");
            }
        }
           
        private Style _menuImportButtonStyle;
        public Style MenuImportButtonStyle
        {
            get
            {
                return _menuImportButtonStyle;
            }
            set
            {
                _menuImportButtonStyle = value;
                OnPropertyChanged("MenuImportButtonStyle");
            }
        }
        /// <summary>
        /// The _visible simulate
        /// </summary>
        private Visibility _visibleSimulate;
        /// <summary>
        /// Gets or sets the visible simulate.
        /// </summary>
        /// <value>The visible simulate.</value>
        public Visibility VisibleSimulate
        {
            get
            {
                return _visibleSimulate;
            }
            set
            {
                _visibleSimulate = value;
                OnPropertyChanged("VisibleSimulate");
            }
        }

       
        #endregion
        #region Command
        /// <summary>
        /// Gets the import command.
        /// </summary>
        /// <value>The import command.</value>
        public ICommand ImportCommand { get; private set; }
        /// <summary>
        /// Gets the cancel command.
        /// </summary>
        /// <value>The cancel command.</value>
        public ICommand CancelCommand { get; private set; }
        /// <summary>
        /// Gets the browse file command.
        /// </summary>
        /// <value>The browse file command.</value>
        public ICommand BrowseFileCommand { get; private set; }
        /// <summary>
        /// Executes the import.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void ExecuteImport(object obj)
        {
            VisibleSimulate = Visibility.Visible;
            //MenuButtonStyle = _view.FindResource("MenuButtonHover") as Style;
            MenuImportButtonStyle = _view.FindResource("MenuImportButton") as Style;
           _mainViewModel.ShowMessage("Importing...");         
            var executeImport = new BackgroundWorker();
            executeImport.DoWork += OnExecuteImport_Dowork;
            executeImport.RunWorkerCompleted += OnExecuteImport_RunWorkerCompleted;
            executeImport.RunWorkerAsync();            
        }
        void OnExecuteImport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _view.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (Action) (() =>
             {
                VisibleSimulate = Visibility.Collapsed;
                _mainViewModel.HideMessage();
                CommandManager.InvalidateRequerySuggested();
                MenuImportButtonStyle = _view.FindResource("MenuButton") as Style;
                ApplicationContext.IsReloadForRefresh = false;
                 //                MakeTree(CurrentDirectoryNode, !string.IsNullOrWhiteSpace(ApplicationContext.SearchText), ApplicationContext.SearchText, false, false, true);
                 ApplicationContext.IsBusy = false;
             }));
        }
        private void OnExecuteImport_Dowork(object sender, DoWorkEventArgs e)
        {
            try
            {
                _view.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, (Action)(() =>
                {
                    ApplicationContext.IsBusy = true;
                    CommandManager.InvalidateRequerySuggested();
                    var dataImport = _directoryComputerImporter.DirectoryComputerCollections.Where(c => c.Id == 0).ToList();
                    if (dataImport.Count > 0)
                    {

                        var result = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<ImportFolderComputerResponse>(
                            sc.ImportFolderComputer,
                            dataImport));
                        if (result == null)
                        {
                            DialogHelper.Alert("Can not receive status data");
                            return;
                        }

                        _directoryComputerImporter = null;
                        FilePath = string.Empty;

                    }
                    else
                    {
                        DialogHelper.Alert("There is no data to insert.");
                    }
                }));
            }
            catch (Exception)
            {
                ApplicationContext.IsBusy = false;
            }
            
        }
        /// <summary>
        /// Determines whether this instance can import the specified pars.
        /// </summary>
        /// <param name="pars">The pars.</param>
        /// <returns><c>true</c> if this instance can import the specified pars; otherwise, <c>false</c>.</returns>
        private bool CanImport(object pars)
        {
            return _directoryComputerImporter != null && _directoryComputerImporter.IsValid;
        }
        /// <summary>
        /// Executes the cancel.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void ExecuteCancel(object obj)
        {
            _directoryComputerImporter = null;
            if (!PageNavigatorHelper.IsCurrent<EndPointListPage>())
                PageNavigatorHelper.Switch(new EndPointListPage());
            var viewModel =
                PageNavigatorHelper.GetMainContentViewModel<GroupViewModel>();
            var selectedFolderIds = new List<int>();
            var selectedEndpointIds = new List<int>();
            selectedFolderIds.Add(CurrentDirectoryNode.NodeId);
            viewModel.SetParamsValueForDirectory(selectedFolderIds, selectedEndpointIds,
                ApplicationContext.SearchText, false, Guid.NewGuid(),"");
            viewModel.GetData();
            
            FilePath = string.Empty;
        }

        /// <summary>
        /// Determines whether this instance can cancel the specified pars.
        /// </summary>
        /// <param name="pars">The pars.</param>
        /// <returns><c>true</c> if this instance can cancel the specified pars; otherwise, <c>false</c>.</returns>
        private bool CanCancel(object pars)
        {
            return _directoryComputerImporter != null;
        }
        /// <summary>
        /// Executes the browse file.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void ExecuteBrowseFile(object obj)
        {
            var dlg = new OpenFileDialog { Multiselect = false, CheckFileExists = true };
            if (!string.IsNullOrWhiteSpace(FilePath))
            {
                if (File.Exists(FilePath))
                {
                    dlg.InitialDirectory = Path.GetDirectoryName(FilePath);
                    dlg.FileName = Path.GetFileName(FilePath);
                }
                else if (System.IO.Directory.Exists(FilePath))
                    dlg.InitialDirectory = FilePath;
                else
                    dlg.InitialDirectory = Path.GetPathRoot(FilePath);
            }
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var bgBrowseFile = new BackgroundWorker();
                bgBrowseFile.DoWork += OnBrowseFile_Dowork;
                bgBrowseFile.RunWorkerCompleted += OnBrowseFile_RunWorkerCompleted;
                bgBrowseFile.RunWorkerAsync(dlg.FileName);
            }
           
        }

        /// <summary>
        /// Handles the <see cref="E:BrowseFile_RunWorkerCompleted" /> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RunWorkerCompletedEventArgs"/> instance containing the event data.</param>
        void OnBrowseFile_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
           
            if (!_directoryComputerImporter.IsValid)
            {
                //DialogHelper.Warning(_directoryComputerImporter.Message);
                DialogHelper.Warning("The data format is invalid.");
                FilePath = string.Empty;
            } 
            else
            {
                var itemNote = _directoryComputerImporter.DirectoryComputerCollections
                  .FirstOrDefault(c => c.Id == CurrentDirectoryNode.NodeId);
                 bool hasNewItem = false;
                   CurrentDirectoryNode.IsExpanded = true;
                   CommandManager.InvalidateRequerySuggested();
                   XamDataTreeBuilder.AppendTree(CurrentDirectoryNode, itemNote,
                                                   _directoryComputerImporter.DirectoryComputerCollections,ref hasNewItem);
                ApplicationContext.ImportNodesExpanded = new List<int>();
                //Expanded all node has child imported node
                XamDataTreeBuilder.ExpandedPathNode(CurrentDirectoryNode);
                XamDataTreeBuilder.ExpandedChildNode(CurrentDirectoryNode);
                XamDataTreeBuilder.SetNodeExpandedState(_view.DataTreeImport.Nodes);
                //Clear data                
                ApplicationContext.ImportNodes = new ObservableCollection<DirectoryNode>();
                ApplicationContext.ImportNodesExpanded =new List<int>();
                if (!hasNewItem)
                {
                    DialogHelper.Warning("All nodes existed in directory.");
                    FilePath = string.Empty;
                }
                CommandManager.InvalidateRequerySuggested();
               
            }
            var bgBrowseFile = sender as BackgroundWorker;
            if (bgBrowseFile != null)
            {
                bgBrowseFile.DoWork += OnBrowseFile_Dowork;
                bgBrowseFile.RunWorkerCompleted += OnBrowseFile_RunWorkerCompleted;
            }
            VisibleSimulate = Visibility.Collapsed;
            _mainViewModel.IsBusy = false;
        }

        /// <summary>
        /// Handles the <see cref="E:BrowseFile_Dowork" /> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DoWorkEventArgs"/> instance containing the event data.</param>
        void OnBrowseFile_Dowork(object sender, DoWorkEventArgs e)
        {
            VisibleSimulate = Visibility.Visible;
            //MenuButtonStyle = _view.FindResource("MenuButtonHover") as Style;
            _mainViewModel.IsBusy = true;
            FilePath = e.Argument.ToString();
            if (_directoryComputerImporter != null)
            {
                ReloadData(CurrentDirectoryNode.NodeId);
            }
            CurrentDirectoryNode.IsExpanded = true;
            _directoryComputerImporter = new DirectoryComputerImporter(FilePath, GetDirectoryTree());            
        }
        /// <summary>
        /// Determines whether this instance [can browse file] the specified pars.
        /// </summary>
        /// <param name="pars">The pars.</param>
        /// <returns><c>true</c> if this instance [can browse file] the specified pars; otherwise, <c>false</c>.</returns>
        private bool CanBrowseFile(object pars) { return true; }
        #endregion
        #region Public Function

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public override void Refresh()
        {
            //Reload tree
            if (CurrentDirectoryNode != null)
            {
                ReloadData(CurrentDirectoryNode.NodeId);    
            }
        }

        #endregion
        #region Private Function
        /// <summary>
        /// Gets the directory tree.
        /// </summary>
        /// <returns>List&lt;DirectoryComputerItem&gt;.</returns>
        private List<DirectoryComputerItem> GetDirectoryTree()
        {
           var listDirectoryComputerItem= ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<DirectoryComputerItem>>(
                sc.GetAllDirectoryComputer, new DirectoryComputerRequest(_currentDirectoryNode.NodeId)));
            //listDirectoryComputerItem = listDirectoryComputerItem.Where(li => li.IsDirectory).ToList();
            return listDirectoryComputerItem;
        }

        private void ReloadData(int folderId)
        {
            DataTreeImportSource = XamDataTreeBuilder.MakeTree(folderId, out _currentDirectoryNode);
            // CurrentDirectoryNode.NodeColor = "#264A50";
        }
        #endregion
    }
}
