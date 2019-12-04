using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Endpoint
{
    /// <summary>
    /// Class Import Folder Computer ViewModel.
    /// </summary>
    public class ImportFolderComputerViewModel : ViewModelBase
    {
        /// <summary>
        /// The _directory computer importer
        /// </summary>
        private DirectoryComputerImporter _directoryComputerImporter;
        /// <summary>
        /// The _view
        /// </summary>
        private readonly ImportFolderComputerDialog _view;
        #region Properties
        /// <summary>
        /// The _error message
        /// </summary>
        private string _errorMessage;
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The error message.</value>
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; OnPropertyChanged("ErrorMessage"); }
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
            set { _filePath = value; OnPropertyChanged("FilePath"); }
        }
        #endregion
        #region Command
        /// <summary>
        /// Gets the open folder dialog command.
        /// </summary>
        /// <value>The open folder dialog command.</value>
        public ICommand OpenFolderDialogCommand { get; private set; }
        /// <summary>
        /// Gets the import data command.
        /// </summary>
        /// <value>The import data command.</value>
        public ICommand ImportDataCommand { get; private set; }
        #endregion
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ImportFolderComputerViewModel"/> class.
        /// </summary>
        /// <param name="view">The view.</param>
        public ImportFolderComputerViewModel(ImportFolderComputerDialog view)
        {
            _view = view;
            OpenFolderDialogCommand = new RelayCommand(OnOpenFolderDialogExecute);
            ImportDataCommand = new RelayCommand(OnImportDataExecute, CanImportDataExecute);
        }
        #endregion
        #region Public Function
        private int? _folderId;
        public void LoadFolderId(int? folderId)
        {
            FilePath = string.Empty;
            _directoryComputerImporter = null;
            _folderId = folderId;
        }
        #endregion
        #region Private Function
        private List<DirectoryComputerItem> GetDirectoryComputerTree()
        {
            return ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<DirectoryComputerItem>>(
                sc.GetAllDirectoryComputer, new DirectoryComputerRequest(_folderId)));
        }
        /// <summary>
        /// Called when [open folder dialog execute].
        /// </summary>
        /// <param name="pars">The pars.</param>
        private void OnOpenFolderDialogExecute(object pars)
        {
            var dlg = new OpenFileDialog {Multiselect = false, CheckFileExists = true};
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
                FilePath = dlg.FileName;

                _directoryComputerImporter = new DirectoryComputerImporter(FilePath, GetDirectoryComputerTree());
                if (!_directoryComputerImporter.IsValid)
                {
                    //DialogHelper.Warning(_directoryComputerImporter.Message);
                    DialogHelper.Warning("The data format is invalid.");
                    FilePath = string.Empty;
                }
            }
        }
        /// <summary>
        /// Called when click [import data execute].
        /// </summary>
        /// <param name="pars">The pars.</param>
        private void OnImportDataExecute(object pars)
        {
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
                DialogHelper.Alert(result.Message);

                if (_folderId.HasValue)
                {
                    MakeTree(_folderId.Value, !string.IsNullOrWhiteSpace(ApplicationContext.SearchText), ApplicationContext.SearchText);
                }
                _directoryComputerImporter = null;
                FilePath = string.Empty;
                _view.Close();
            }
            else
            {
                DialogHelper.Alert("There is no data to insert.");
            }
        }
        /// <summary>
        /// Determines whether this instance [can import data execute] the specified pars.
        /// </summary>
        /// <param name="pars">The pars.</param>
        /// <returns><c>true</c> if this instance [can import data execute] the specified pars; otherwise, <c>false</c>.</returns>
        private bool CanImportDataExecute(object pars)
        {
            return null != _directoryComputerImporter && 0 < _directoryComputerImporter.DirectoryComputerCollections.Count;
        }
        #endregion
        #region Event Handler

        #endregion
    }
}
