using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls.DirectoryAssignment;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.DirectoryAssignment
{
    public class LDAPAssignmentViewModel : ViewModelBase
    {
        private List<ComboboxItem> _cbLDAPItems;

        private int _cbLDAPSelected;
        private bool _chbAddOnlyComputerChecked;
        private bool _chbExcludeComputerChecked;

        private bool _chbExcludeEmptyFolderChecked;
        private bool _chbExcludeFolderChecked;

        private bool _chbLDAPChecked;
        private bool _chbLDAPFolderChecked;

        private bool _labelOrVisible;
        private string _txtExcludeComputer;
        private string _txtExcludeFolder;
        private string _txtLDAPFolder;
        private readonly LDAPAssignment _view;
        public List<string> ExcludeComputerIds = new List<string>();
        public List<string> ExcludeFolderIds = new List<string>();

        public LDAPAssignmentViewModel(LDAPAssignment view)
        {
            _view = view;
            CbLDAPItems = ApplicationContext.CbLDAPItems;
            CbLDAPSelected = CbLDAPItems!=null&& CbLDAPItems.Count>0?CbLDAPItems[0].Value : 0;
            BrowseLDAPDirectory = new RelayCommand<Button>(OnBrowseLdap, CanBrowseLDAP);
            LDAPAssignmentElementChanged = new RelayCommand(OnLDAPAssignmentElementChanged);
        }

        public ICommand BrowseLDAPDirectory { get; private set; }
        public ICommand LDAPAssignmentElementChanged { get; private set; }

        public bool ChbLDAPChecked
        {
            get { return _chbLDAPChecked; }
            set
            {
                _chbLDAPChecked = value;
                OnPropertyChanged("ChbLDAPChecked");
            }
        }

        public bool ChbLDAPFolderChecked
        {
            get { return _chbLDAPFolderChecked; }
            set
            {
                _chbLDAPFolderChecked = value;
                OnPropertyChanged("ChbLDAPFolderChecked");
            }
        }

        public bool ChbExcludeFolderChecked
        {
            get { return _chbExcludeFolderChecked; }
            set
            {
                _chbExcludeFolderChecked = value;
                OnPropertyChanged("ChbExcludeFolderChecked");
            }
        }

        public bool ChbExcludeComputerChecked
        {
            get { return _chbExcludeComputerChecked; }
            set
            {
                _chbExcludeComputerChecked = value;
                OnPropertyChanged("ChbExcludeComputerChecked");
            }
        }

        public bool ChbExcludeEmptyFolderChecked
        {
            get { return _chbExcludeEmptyFolderChecked; }
            set
            {
                _chbExcludeEmptyFolderChecked = value;
                OnPropertyChanged("ChbExcludeEmptyFolderChecked");
            }
        }

        public bool ChbAddOnlyComputerChecked
        {
            get { return _chbAddOnlyComputerChecked; }
            set
            {
                _chbAddOnlyComputerChecked = value;
                OnPropertyChanged("ChbAddOnlyComputerChecked");
            }
        }

        public bool LabelOrVisible
        {
            get { return _labelOrVisible; }
            set
            {
                _labelOrVisible = value;
                OnPropertyChanged("LabelOrVisible");
            }
        }

        public List<ComboboxItem> CbLDAPItems
        {
            get { return _cbLDAPItems; }
            set
            {
                _cbLDAPItems = value;
                OnPropertyChanged("CbLDAPItems");
            }
        }

        public int CbLDAPSelected
        {
            get { return _cbLDAPSelected; }
            set
            {
                _cbLDAPSelected = value;
                OnPropertyChanged("CbLDAPSelected");
            }
        }

        public string TxtLDAPFolder
        {
            get { return _txtLDAPFolder; }
            set
            {
                _txtLDAPFolder = value;
                OnPropertyChanged("TxtLDAPFolder");
            }
        }

        public string LDAPFolderId { get; set; }
        public int Id { get; set; }

        public string TxtExcludeFolder
        {
            get { return _txtExcludeFolder; }
            set
            {
                _txtExcludeFolder = value;
                OnPropertyChanged("TxtExcludeFolder");
            }
        }

        public string TxtExcludeComputer
        {
            get { return _txtExcludeComputer; }
            set
            {
                _txtExcludeComputer = value;
                OnPropertyChanged("TxtExcludeComputer");
            }
        }

        private void OnBrowseLdap(Button btn)
        {
            if (btn == null)
            {
                return;
            }
            switch (btn.Name)
            {
                case UIConstant.BtnLDAPFolder:
                    DisplayLDAPTreeDialog(1);
                    break;
                case UIConstant.BtnExcludeSubFolders:
                    DisplayLDAPTreeDialog(2);
                    break;
                case UIConstant.BtnExcludeComputer:
                    DisplayLDAPTreeDialog(3);
                    break;
            }
        }

        private bool CanBrowseLDAP(Button btn)
        {
            return CbLDAPSelected > 0;
        }
        private void OnLDAPAssignmentElementChanged(object arg)
        {
            var parent = _view.Parent as StackPanel;
            var labelElem = parent.TryFindParent<AssignmentCriterialElement>();
            if (labelElem != null)
            {
                labelElem.Model.EditRuleCriteriaCommand.Execute(null);
            }
        }

        private void DisplayLDAPTreeDialog(int type)
        {
            ApplicationContext.SelectedTargetNodes = new List<DirectoryNode>();
            var dlg = new ShowLdapDirectoryDialog(this);
            dlg.Model.Type = type;
            dlg.Model.DomainId = CbLDAPSelected;
            dlg.Model.DomainName = CbLDAPItems!=null&& CbLDAPItems.Count>0? CbLDAPItems.Find(r => r.Value == CbLDAPSelected).Text:"";
            var excludeFolderNodes = ExcludeFolderIds.Select(fid => new DirectoryNode {IsFolder = true, GuidString = fid}).ToList();
            var excludeComNodes = ExcludeComputerIds.Select(cid => new DirectoryNode {IsFolder = false, GuidString = cid}).ToList();
            var listNodes = type == 1
                ? (string.IsNullOrEmpty(LDAPFolderId)? new List<DirectoryNode>() : new List <DirectoryNode> {new DirectoryNode {IsFolder = true, GuidString = LDAPFolderId} })
                : type == 2 ? excludeFolderNodes : excludeComNodes; 
            dlg.Model.MakeTree(listNodes);
            PageNavigatorHelper._MainWindow.DynamicShowDialog(dlg);
        }
    }
}