using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Controls;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ViewModel.DirectoryAssignment;

namespace Tabidus.POC.GUI.UserControls.DirectoryAssignment
{
    /// <summary>
    ///     Interaction logic for LDAPAssignment.xaml
    /// </summary>
    public partial class LDAPAssignment : UserControl
    {
        public LDAPAssignment()
        {
            InitializeComponent();
            Model = new LDAPAssignmentViewModel(this);
        }

        public LDAPAssignmentViewModel Model
        {
            get { return DataContext as LDAPAssignmentViewModel; }
            set { DataContext = value; }
        }

        private void FormEventChanged()
        {
            var parent = Parent as StackPanel;
            var labelElem = parent.TryFindParent<AssignmentCriterialElement>();
            if (labelElem != null)
            {
                labelElem.Model.EditRuleCriteriaCommand.Execute(null);
            }
        }

        private void CbLDAP_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cbb = (ComboBox) sender;

            if (!(bool)cbb.Tag)
            {
                Model.ExcludeComputerIds = new List<string>();
                Model.ExcludeFolderIds = new List<string>();
                Model.LDAPFolderId = string.Empty;
                Model.TxtLDAPFolder = string.Empty;
                Model.TxtExcludeFolder = string.Empty;
                Model.TxtExcludeComputer = string.Empty;
                FormEventChanged();
            }
            else
            {
                cbb.Tag = false;
            }
            
        }
        
    }
}