using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Discovery;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.LDAP;
using Tabidus.POC.Common.Model.POCAgent;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.Common.Model.Task;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls.Policy;
using Tabidus.POC.GUI.ViewModel.Policy;
using Tabidus.POC.GUI.ViewModel.Software;

namespace Tabidus.POC.GUI
{
    public class ApplicationContext
    {
        public static bool IsReload = false;
        public static bool IsReloadForRefresh = false;
        public static bool IsBusy = false;
        public static string ServerAddress = Functions.GetServerAddress();
        /// <summary>
        ///     A list of all endpoints
        /// </summary>
        public static List<EndPoint> EndPointListAll { get; set; }

        /// <summary>
        ///     List of tree endpoints
        /// </summary>
        public static List<EndPoint> EndPointListTree { get; set; }

        /// <summary>
        ///     A list of all directories
        /// </summary>
        public static List<Directory> FolderListAll { get; set; }

        /// <summary>
        ///     A list of tree directories
        /// </summary>
        public static List<Directory> FolderListTree { get; set; }

        /// <summary>
        ///     label tree data
        /// </summary>
        public static List<LabelEndPointsData> LableEndpointDatas { get; set; }

        /// <summary>
        ///     Tree nodeId selected, use this when editing the node
        /// </summary>
        public static int NodeId { get; set; }

        public static int SoftwareSelectedNodeId { get; set; }

        /// <summary>
        ///     Target note id, use to move nodes
        /// </summary>
        public static int NodeTargetId { get; set; }
        public static List<DirectoryNode> SelectedTargetNodes { get; set; }
        /// <summary>
        ///     Validate when moving a node
        /// </summary>
        public static bool IsError { get; set; }

        /// <summary>
        ///     Validate after added a note
        /// </summary>
        public static bool IsAddNode { get; set; }

        public static bool IsEditNode { get; set; }

        /// <summary>
        ///     Check if after delete a node
        /// </summary>
        public static bool IsDeleteNode { get; set; }

        /// <summary>
        ///     a List of all selected node
        /// </summary>
        public static List<DirectoryNode> NodesSelected { get; set; }

        public static List<DirectoryNode> LabelNodesSelected { get; set; }

        public static List<DirectoryNode> LDAPNodesSelected { get; set; }
        /// <summary>
        ///     List of note id that expanded, use to refresh tree data
        /// </summary>
        public static List<int> ExpandedIds { get; set; }

        public static List<int> LabelExpandedIds { get; set; }
        public static string SearchText { get; set; }
        public static string TotalEndpoint { get; set; }
        public static string FolderPathName { get; set; }
        public static DirectoryNode BelowNode { get; set; }
        public static bool IsFromLabel { get; set; }
        public static double GridRightOriginalWidth { get; set; }
        public static bool IsRebuildTree { get; set; }
        public static List<LastUpdated> LastRefresh { get; set; }
        public static List<DirectoryNode> DirNodesSelectedBeforeSearch { get; set; }
        public static List<int> DirExpandedNodesBeforeSearch { get; set; }
        public static ObservableCollection<DirectoryNode> DirectoryTreeSourceBeforeSearch { get; set; }
        public static bool DirSearched { get; set; }
        public static List<DirectoryNode> LabelNodesSelectedBeforeSearch { get; set; }
        public static List<int> LabelExpandedNodesBeforeSearch { get; set; }
        public static List<DirectoryNode> LabelTreeSourceBeforeSearch { get; set; }
        public static List<LabelEndPointsData> LableEndpointDatasBeforeSearch { get; set; }
        public static bool LabelSearched { get; set; }
        public static XamDataTreeNode NodeEditting { get; set; }
        public static ObservableCollection<DirectoryNode> ImportNodes { get; set; }
        public static List<int> ImportNodesExpanded { get; set; }
        public static double GroupViewVerticalOffset { get; set; }
        public static  bool CanRefreshEndpoint { get; set; }
        public static bool CanRefreshDiscovery { get; set; }
        public static bool CanRefreshLDAP { get; set; }
        public static bool CanRefreshSoftware { get; set; }
        public static bool CanRefreshUpdateSource { get; set; }
        public static bool CanRefreshPolicyAgent { get; set; }
        public static bool CanRefreshPolicyAgentDirectory { get; set; }
		public static bool CanRefreshTask { get; set; }
        #region Endpoint Grid column width

        public static int IDWidth { get; set; }
        public static int TypeWidth { get; set; }
        public static int IPv4Width { get; set; }
        public static int IPv6Width { get; set; }
        public static int DomainWidth { get; set; }
        public static int PowerStateWidth { get; set; }
        public static int SystemNameWidth { get; set; }
        public static int UserNameWidth { get; set; }
        public static int OSNameWidth { get; set; }
        public static int LastSyncWidth { get; set; }

        #endregion

        #region Neighborhood Watch Grid column width

        public static int NeVendorWidth { get; set; }
        public static int NeMACWidth { get; set; }
        public static int NeIPv4Width { get; set; }
        public static int NeIPv6Width { get; set; }
        public static int NeDomainWidth { get; set; }
        public static int NeLastSeenWidth { get; set; }
        public static int NeSystemNameWidth { get; set; }
        public static int NeDetectedByWidth { get; set; }
        public static int NeOSNameWidth { get; set; }
        public static int NeConfirmedWidth { get; set; }

        #endregion
        #region Software Grid column width

        public static int SoftwareNameWidth { get; set; }
        public static int SoftwareVersionWidth { get; set; }
        public static int SoftwareCommentWidth { get; set; }
        public static int SoftwareExecutableWidth { get; set; }
        public static int SoftwareParametersWidth { get; set; }
        public static int SoftwareSizeWidth { get; set; }

        #endregion
        #region Label combobox items

        public static List<string> CbComputerOpeItems { get; set; }
        public static List<string> CbVendorOpeItems { get; set; }
        public static List<string> CbModelOpeItems { get; set; }
        public static List<string> CbOsOpeItems { get; set; }
        public static List<string> CbDomainOpeItems { get; set; }
        public static List<ComboboxItem> CbPlatformOpeItems { get; set; }
        public static List<string> CbComputerTypeOpeItems { get; set; }
        public static List<string> CbMemoryOpeItems { get; set; }
        public static List<string> CbHarddiskOpeItems { get; set; }
        public static List<string> CbIPv4OpeItems { get; set; }
        public static List<string> CbIPv6OpeItems { get; set; }
        public static List<string> CbLastSyncOpeItems { get; set; }
        public static List<string> CbColorCodeOpeItems { get; set; }
        public static List<string> CbVendorCriteriaItems { get; set; }
        public static List<string> CbOsCriteriaItems { get; set; }
        public static List<string> CbDomainCriteriaItems { get; set; }
        public static List<string> CbModelCriteriaItems { get; set; }

        #endregion
        public static List<NeighborhoodWatch> AllNeighborhoodWatch { get; set; }
        public static double NeighborhoodWatchViewVerticalOffset { get; set; }

        public static List<string> LDAPTypes { get; set; }
        public static Dictionary<string, int> LdapTypeDictionary { get; set; }
        public static List<LDAP> LDAPList { get; set; }
        public static LDAP LDAPActived { get; set; }
        public static ConcurrentDictionary<int, LDAPDirectoriesEndpoints> LdapDirectoriesEndpointsDictionary { get; set; }
        public static List<string> LdapExpandedIdList { get; set; }
        public static List<ComboboxItem> CbLDAPItems { get; set; }

        public static GetRulesDataResponse AssignmentRulesData { get; set; }

        public static List<UpdateSource> UpdateSourceList { get; set; }
        public static List<SoftwareContent> SoftwareList { get; set; }
        public static List<UpdateSourceSoftware> UpdateSourceSoftwareList { get; set; }
        public static List<UpdateSourceElementViewModel> UpdateSourceElementViewModelsSelected { get; set; }
        public static int TransferScheduleId { get; set; }
		public static string MainUpdateSourceUrl { get; set; }
        public static List<PolicyElementViewModel> PoliciesList { get; set; }
        public static List<POCAgent> POCAgentList { get; set; }
        public static List<PolicyAssign> FolderPolicyList { get; set; }
        public static List<PolicyAssign> EndpointPolicyList { get; set; }
		public static List<SoftwareContent> TaskSoftwareList { get; set; }
		public static List<SoftwareTask> TaskSoftwareSelectedList { get; set; }
		public static ConcurrentDictionary<int, TaskProgressResult> TaskProgressDictionary { get; set; }

	}
}