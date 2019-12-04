using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.POCAgent;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.UserControls.Policy;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.ViewModel.Policy
{
    public class PolicyEnhancementPageViewModel : PageViewModelBase
    {
        public readonly PolicyEnhancementPage _view;

        public PolicyEnhancementPageViewModel(PolicyEnhancementPage view)
        {
            _view = view;
            BuidPage();
        }

        public void UpdateHeader()
        {
	        if (IsSelectSingleEndpoint())
	        {
		        var headerViewModel = _view.EndpointHeaderViewModel;
		        if (headerViewModel != null && ApplicationContext.NodesSelected.Count > 0)
		        {
			        headerViewModel.UpdateHeader(3);
		        }
	        }
	        else
	        {
				var headerViewModel = _view.HeaderViewModel;
				if (headerViewModel != null)
				{
					if (ApplicationContext.NodesSelected.Count == 1)
					{
						headerViewModel.UpdateDirectoryHeader(ApplicationContext.NodesSelected[0].NodeId);
					}
					else
					{
						headerViewModel.FolderPathName = "Selected Objects";
						headerViewModel.TotalEndpoints = GetTotalEndpoint();
					}
				}
			}
        }

	    private string GetTotalEndpoint()
	    {
		    var listEndpointSelected = ApplicationContext.NodesSelected.Where(c => !c.IsFolder).Select(c => c.NodeId);
		    var listFolderSelected = ApplicationContext.NodesSelected.Where(c => c.IsFolder).Select(c => c.NodeId);
			var listEndpointResult = new List<int>(listEndpointSelected);
		    foreach (var folder in listFolderSelected)
		    {
				listEndpointResult.AddRange(GetAllEndpointOnFolder(folder));
		    }
			var totalEndpointSelected = listEndpointResult.Distinct().Count();
		    return string.Format("{0} {1}", totalEndpointSelected, totalEndpointSelected > 1 ? "Endpoints" : "Endpoint");
	    }

	    private IEnumerable<int> GetAllEndpointOnFolder(int folder)
	    {
			List<int> result = new List<int>();
		    foreach (var endPoint in ApplicationContext.EndPointListAll)
		    {
			    if(endPoint.FolderId == folder)
					result.Add(endPoint.EndpointId);
		    }
		    foreach (var directory in ApplicationContext.FolderListAll)
		    {
			    if (directory.ParentId == folder)
					result.AddRange(GetAllEndpointOnFolder(directory.FolderId));
		    }
		    return result;
	    }

	    private bool IsSelectSingleEndpoint()
		{
			return ApplicationContext.NodesSelected.Count == 1 && !ApplicationContext.NodesSelected[0].IsFolder;

		}
		public void BuidPage()
        {
            if (ApplicationContext.NodesSelected.Count > 0)
            {
	            if (IsSelectSingleEndpoint())
	            {
					_view.PolicyEnhancementHeaderElement.Visibility = Visibility.Collapsed;
					_view.EndpointPolicyEnhancementHeaderElement.Visibility = Visibility.Visible;
				}
                else
                {
                    _view.PolicyEnhancementHeaderElement.Visibility = Visibility.Visible;
                    _view.EndpointPolicyEnhancementHeaderElement.Visibility = Visibility.Collapsed;
                }
                    
                UpdateHeader();
                _view.PnlPolicyContainer.Children.Clear();
                //var selectedNode = ApplicationContext.NodesSelected[0];
                
                var pocAgentList = new List<POCAgent>();
                if (ApplicationContext.POCAgentList == null)
                {
                    Functions.GetAllPolicies();
                }
                if (ApplicationContext.FolderPolicyList == null)
                {
                    Functions.LoadFolderPolicy();
                }
                if (ApplicationContext.EndpointPolicyList == null)
                {
                    Functions.LoadEndpointPolicy();
                }

				//Display all policies
	            foreach (var node in ApplicationContext.NodesSelected)
	            {
		            if (node.IsFolder)
		            {
			            var policyFolder = ApplicationContext.FolderPolicyList.FirstOrDefault(c => c.ObjectId == node.NodeId);
			            if (policyFolder != null)
			            {
				            var pocAgentFolder = ApplicationContext.POCAgentList.FirstOrDefault(c => c.Id == policyFolder.PolicyAgentId);
				            if (pocAgentFolder != null && !pocAgentList.Exists(c=>c.Id == pocAgentFolder.Id))
				            {
					            pocAgentList.Add(pocAgentFolder);
				            }
			            }
		            }
		            else
		            {
						var policyEndpoint = ApplicationContext.EndpointPolicyList.FirstOrDefault(c => c.ObjectId == node.NodeId);
						if (policyEndpoint != null)
						{
							var pocAgentEndpoint = ApplicationContext.POCAgentList.FirstOrDefault(c => c.Id == policyEndpoint.PolicyAgentId);
							if (pocAgentEndpoint != null && !pocAgentList.Exists(c=>c.Id == pocAgentEndpoint.Id))
							{
								pocAgentList.Add(pocAgentEndpoint);
							}
						}
					}
	            }
				//if (selectedNode.IsFolder)
				//{
				//    var directory = ApplicationContext.FolderListAll.Find(r => r.FolderId == selectedNode.NodeId);

				//    var rootPolicy = ApplicationContext.FolderPolicyList.Find(r => r.ObjectId == selectedNode.NodeId);
				//    if (rootPolicy != null)
				//    {
				//        var pocAgent = ApplicationContext.POCAgentList.Find(r => r.Id == rootPolicy.PolicyAgentId);
				//        pocAgentList.Add(pocAgent);
				//    }
				//    FindChildPolicyAssign(directory, pocAgentList);
				//}
				//else
				//{
				//    var rootPolicy = ApplicationContext.EndpointPolicyList.Find(r => r.ObjectId == selectedNode.NodeId);
				//    if (rootPolicy != null)
				//    {
				//        var pocAgent = ApplicationContext.POCAgentList.Find(r => r.Id == rootPolicy.PolicyAgentId);
				//        pocAgentList.Add(pocAgent);
				//    }
				//}
				foreach (var pocAgent in pocAgentList)
                {
                    var policyEnhancement = new PolicyEnhancementElement();
                    policyEnhancement.Model.Id = pocAgent.Id;
                    policyEnhancement.Model.Name = pocAgent.Name;
                    policyEnhancement.Model.POCServer = pocAgent.POCServer;
                    policyEnhancement.Model.Port = pocAgent.Port;
                    policyEnhancement.Model.Key = pocAgent.Key;
                    policyEnhancement.Model.NeighborhoodWatch = pocAgent.NeighborhoodWatch;
                    policyEnhancement.Model.ActiveTransfer = pocAgent.ActiveTransfer;
                    policyEnhancement.Model.TransferInterval = pocAgent.TransferInterval;
                    policyEnhancement.Model.UpdateSource = pocAgent.UpdateSource;
                    policyEnhancement.Model.SynchronizationInterval = pocAgent.SyncInterval;
                    policyEnhancement.Model.ExpanderBackgroundColor = pocAgent.Color;
                    _view.PnlPolicyContainer.Children.Add(policyEnhancement);
                }
            }
        }

        private void FindChildPolicyAssign(Directory parentNode, List<POCAgent> pal)
        {
            // find all children of parent node (they have parentId = id of parent node)
            var nodes = ApplicationContext.FolderListAll.Where(e => e.ParentId == parentNode.FolderId).ToList();
            // find all children of parent node (they have parentId = id of parent node)
            var nodes2 = ApplicationContext.EndPointListAll.Where(e => e.FolderId != null && e.FolderId == parentNode.FolderId)
                .ToList();

            foreach (var node in nodes)
            {
                var rootPolicy = ApplicationContext.FolderPolicyList.Find(r => r.ObjectId == node.FolderId);
                if (rootPolicy != null && !pal.Select(r => r.Color).Contains(rootPolicy.Color))
                {
                    var pocAgent = ApplicationContext.POCAgentList.Find(r => r.Id == rootPolicy.PolicyAgentId);
                    pal.Add(pocAgent);
                }
                FindChildPolicyAssign(node, pal);
            }
            foreach (var node in nodes2)
            {
                var rootPolicy = ApplicationContext.EndpointPolicyList.Find(r => r.ObjectId == node.EndpointId);
                if (rootPolicy != null && !pal.Select(r => r.Color).Contains(rootPolicy.Color))
                {
                    var pocAgent = ApplicationContext.POCAgentList.Find(r => r.Id == rootPolicy.PolicyAgentId);
                    pal.Add(pocAgent);
                }
            }
        }

        private bool _isGroupPolicy = true;

        public bool IsGroupPolicy
        {
            get { return _isGroupPolicy; }
            set
            {
                _isGroupPolicy = value;
                OnPropertyChanged("IsGroupPolicy");
            }
        }
    }
}