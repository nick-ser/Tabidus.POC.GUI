using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.Command;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Software
{
    public class SoftwareViewModel : PageViewModelBase
    {
        private readonly SoftwarePage _view;

        private ObservableCollection<SoftwareContentView> _viewList;

        public SoftwareViewModel(SoftwarePage view)
        {
            _view = view;
            ViewList = new ObservableCollection<SoftwareContentView>();
            TabSelectedCommand = new RelayCommand<Button>(OnMenuSelected, CanMenuAction);
        }
        public ICommand TabSelectedCommand { get; private set; }
        public ObservableCollection<SoftwareContentView> ViewList
        {
            get { return _viewList; }
            set
            {
                _viewList = value;
                OnPropertyChanged("ViewList");
            }
        }

        private string _sourceName;

        public string SourceName
        {
            get { return _sourceName; }
            set
            {
                _sourceName = value;
                OnPropertyChanged("SourceName");
            }
        }

        public override void Refresh()
        {
            Functions.GetAllSoftware();
            Functions.GetAllUpdateSourceSoftware();
            BuidPage();
        }

        public void BuidPage()
        {
	        ApplicationContext.TaskSoftwareList = null;

			if (ApplicationContext.SoftwareList == null)
            {
                Functions.GetAllSoftware();
            }
            if (ApplicationContext.UpdateSourceSoftwareList == null)
            {
                Functions.GetAllUpdateSourceSoftware();
            }
            var updSourceSoftwares =
                ApplicationContext.UpdateSourceSoftwareList.Where(
                    r => r.UpdateSourceId == ApplicationContext.SoftwareSelectedNodeId).Select(r => r.SoftwareId).ToList();
            var softwareOfSource =
                ApplicationContext.SoftwareList.Where(r => updSourceSoftwares.Contains(r.Id) && r.UpdateSourceId == ApplicationContext.SoftwareSelectedNodeId);
            var scvl = new ObservableCollection<SoftwareContentView>();
            foreach (var software in softwareOfSource)
            {
                var scv = new SoftwareContentView(software);
                scvl.Add(scv);
            }
            ViewList = scvl;
        }
        private void OnMenuSelected(Button btn)
        {
            if (btn == null)
                return;

            switch (btn.Name)
            {
                case UIConstant.BtnSoftwareAddPackage:
                    DisplayAddOrEditDialog(0);
                    break;
                case UIConstant.BtnSoftwareEditPackage:
                    var selectedId = ViewList.First(r => r.IsSelected).Id;
                    DisplayAddOrEditDialog(selectedId);
                    break;
                case UIConstant.BtnSoftwareDeletePackage:
                    var confirmDlg = new ConfirmDialog("Are you sure you want to delete?","Delete Software");
                    if (confirmDlg.ShowDialog() == true)
                    {
                        DeleteContent();
                    }
                    break;
            }
        }

        private bool CanMenuAction(Button btn)
        {
            if (btn == null)
                return false;
			//Disable button when not selected at main update source
			var mainUpdateSource = ApplicationContext.UpdateSourceList.FirstOrDefault(c => c.ParentId == 0);
	        if (mainUpdateSource == null || ApplicationContext.SoftwareSelectedNodeId != mainUpdateSource.Id)
	        {
				return false;
			}
			//Count package selected
			var countSelected = ViewList.Count(r => r.IsSelected);
			switch (btn.Name)
            {
				//Allow delete many
                case UIConstant.BtnSoftwareDeletePackage:
		            return countSelected > 0;
				//Allow edit one
				case UIConstant.BtnSoftwareEditPackage:
		            return countSelected == 1;
            }
            return true;
        }

        private void DisplayAddOrEditDialog(int contentId)
        {
            var dlg = new AddOrEditSoftwareDialog();
            dlg.Model.IsAdding = contentId==0;
            dlg.Model.Id = contentId;
            dlg.Model.AddOrEditText = "Add content";
            if (contentId > 0)
            {
                var software = ApplicationContext.SoftwareList.Find(r => r.Id == contentId);
                dlg.Model.AddOrEditText = "Edit content";
                dlg.Model.Name = software.Name;
                dlg.Model.Version = software.Version;
                dlg.Model.Comment = software.Comment;
                dlg.Model.Executable = software.FileName;
                dlg.Model.Parameters = software.Parameters;
                dlg.Model.Size = software.Size;
                dlg.Model._oriFileName = software.FileName;
                dlg.Model._oriVersion = software.Version;
            }
            PageNavigatorHelper._MainWindow.DynamicShowDialog(dlg, null, "Add or edit software");
        }

        private void DeleteContent()
        {
            var selectedIds = ViewList.Where(r => r.IsSelected).Select(r=>r.Id).ToList();
				var filesToDelete = ViewList.Where(r => r.IsSelected).Select(r => r.Name).ToList();
				var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK",
                StringValue = string.Join(",",selectedIds)
            };
            using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            {
                var requestData = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(requestObj),
                    KeyEncryption);
                sc.DeleteContent(requestData);
            }
				ServiceManager.DeleteSoftwareFile(filesToDelete);
            ApplicationContext.SoftwareList.RemoveAll(r => selectedIds.Contains(r.Id));
            ApplicationContext.UpdateSourceSoftwareList.RemoveAll(r => selectedIds.Contains(r.SoftwareId));
            BuidPage();
        }
        
    }
}