using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Infragistics.Controls.Menus;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.Common;

namespace Tabidus.POC.GUI.ViewModel.Label
{
    /// <summary>
    /// Class LabelFilterViewModel.
    /// </summary>
    public class LabelFilterViewModel : ViewModelBase
    {
        /// <summary>
        /// The _label data tree
        /// </summary>
        private XamDataTree _labelDataTree;
        /// <summary>
        /// The _label end points
        /// </summary>
        private List<LabelEndPointsData> _labelEndPoints;
        /// <summary>
        /// The _label tree data source
        /// </summary>
        private List<DirectoryNode> _labelTreeDataSource;
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelFilterViewModel"/> class.
        /// </summary>
        /// <param name="labelDataTree">The label data tree.</param>
        public LabelFilterViewModel(XamDataTree labelDataTree)
        {
            _labelDataTree = labelDataTree;
        }

        /// <summary>
        /// Gets the label tree data source.
        /// </summary>
        /// <value>The label tree data source.</value>
        public List<DirectoryNode> LabelTreeDataSource
        {
            get
            {
                return _labelTreeDataSource;
            }
        }
        /// <summary>
        /// Loads the data.
        /// </summary>
        /// <param name="labelEndPoints">The label end points.</param>
        public void LoadData(List<LabelEndPointsData> labelEndPoints)
        {
            _labelEndPoints = labelEndPoints;

            BuildTree();
            OnPropertyChanged("LabelTreeDataSource");
        }
        public void LoadData(List<LabelEndPointsData> labelEndPoints, List<DirectoryNode> labelNodesSelected)
        {
            _labelEndPoints = labelEndPoints;

            BuildTree(labelNodesSelected);
            OnPropertyChanged("LabelTreeDataSource");
        }
        /// <summary>
        /// Builds the tree.
        /// </summary>
        private void BuildTree()
        {
            _labelTreeDataSource = new List<DirectoryNode>();
            foreach (var labelEndPoint in _labelEndPoints)
            {
                var directoryNode = CreateDirectoryNode(labelEndPoint.Id, labelEndPoint.Name);
                directoryNode.DirectoryNodes = new ObservableCollection<DirectoryNode>(CreateChildNodes(labelEndPoint.EndPointDatas));
                _labelTreeDataSource.Add(directoryNode);
            }
            _labelDataTree.ItemsSource = _labelTreeDataSource;
            List<DirectoryNode> firstNode = _labelTreeDataSource.Select(i => i).Take(1).ToList();
            PageNavigatorHelper.GetRightElementViewModel().RefreshLabelTreeData(firstNode);
        }
        private void BuildTree(List<DirectoryNode> labelNodesSelected)
        {
            _labelTreeDataSource = new List<DirectoryNode>();
            foreach (var labelEndPoint in _labelEndPoints)
            {
                var directoryNode = CreateDirectoryNode(labelEndPoint.Id, labelEndPoint.Name);
                directoryNode.DirectoryNodes = new ObservableCollection<DirectoryNode>(CreateChildNodes(labelEndPoint.EndPointDatas));
                _labelTreeDataSource.Add(directoryNode);
            }
            _labelDataTree.ItemsSource = _labelTreeDataSource;

	        if (_labelTreeDataSource.Count == 0)
	        {
		        _labelDataTree.Width = PageNavigatorHelper._MainWindow.GrdRightElement.ActualWidth;
	        }
	        else
	        {
		        _labelDataTree.Width = ApplicationContext.GridRightOriginalWidth;
	        }
			PageNavigatorHelper.GetRightElementViewModel().RefreshLabelTreeData(labelNodesSelected);
        }
        /// <summary>
        /// Creates the directory node.
        /// </summary>
        /// <param name="labelId">The label identifier.</param>
        /// <param name="labelName">Name of the label.</param>
        /// <returns>DirectoryNode.</returns>
        private DirectoryNode CreateDirectoryNode(int labelId, string labelName)
        {
            return new DirectoryNode
            {
                NodeId = labelId,
                Title = labelName,
                IsFolder = true,
                NodeColor = CommonConstants.DEFAULT_TEXT_COLOR,
                NodeWidth = ApplicationContext.GridRightOriginalWidth,
                Guid = Guid.NewGuid()
        };
        }

        /// <summary>
        /// Creates the child nodes.
        /// </summary>
        /// <param name="endPointDatas">The end point datas.</param>
        /// <returns>List&lt;DirectoryNode&gt;.</returns>
        private List<DirectoryNode> CreateChildNodes(IEnumerable<EndPointData> endPointDatas)
        {
            var result = new List<DirectoryNode>();
            foreach (var endPointData in endPointDatas)
            {
                result.Add(CreateComputerNode(endPointData));
            }

            return result;
        }

        /// <summary>
        /// Creates the computer node.
        /// </summary>
        /// <param name="endPointData">The end point data.</param>
        /// <returns>DirectoryNode.</returns>
        private DirectoryNode CreateComputerNode(EndPointData endPointData)
        {
            return new DirectoryNode
            {
                NodeId = endPointData.EndpointId,
                Title = endPointData.SystemName,
                IsFolder = false,
                ComputerType = endPointData.ComputerType ?? 0,
                NodeColor = endPointData.PowerState == 0 ? CommonConstants.POWERSTATE_ONLINE_ENDPOINT : CommonConstants.POWERSTATE_OFFLINE_ENDPOINT ,
                //NodeColor = endPointData.ColorCode == "0" ? CommonConstants.POWERSTATE_OFFLINE_ENDPOINT : endPointData.ColorCode == "1" ? CommonConstants.POWERSTATE_ONLINE_ENDPOINT : CommonConstants.POWERSTATE_OFFLINE_ENDPOINT,
                IsNoAgent = string.IsNullOrEmpty(endPointData.Id),
                NodeWidth = ApplicationContext.GridRightOriginalWidth,
                Guid = Guid.NewGuid(),
                ImagePath =EndPoint.GetImages(endPointData.ComputerType.ToString(), endPointData.ColorCode, true)
        };
        }
    }
}
