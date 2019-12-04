using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.DataPresenter.Events;
using Tabidus.POC.GUI.ViewModel;
using Tabidus.POC.Common;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.GUI.Common;
using Tabidus.POC.GUI.ViewModel.Endpoint;

namespace Tabidus.POC.GUI.View
{
    /// <summary>
    ///     Interaction logic for EndPointListPage.xaml
    /// </summary>
    public partial class EndPointListPage : Page
    {
        public EndPointListPage()
        {
            InitializeComponent();
            EndPointDataGrid.FieldLayouts.DataPresenter.GroupByAreaLocation = GroupByAreaLocation.None;
            var groupHeaderViewModel = this.GroupHeaderElement.DataContext as GroupHeaderViewModel;
            if (groupHeaderViewModel != null)
            {
                groupHeaderViewModel.ActivedButtonIndex = 1;
            }
            DataContext = new GroupViewModel(this);
        }
        
        private void Field_Resized(object sender, SizeChangedEventArgs e)
        {
            Field field = (sender as LabelPresenter).Field;
            if (field.Name == "LastSync")
            {
                if ((int) field.LabelWidthResolved != 130)
                    SetColumnWidth(field.Name, (int) field.LabelWidthResolved);
                else
                {
                    field.Width = new FieldLength(GetColumnWidth(field.Name));
                }
            }
            else
            {
                if (field.Width != null)
                {
                    SetColumnWidth(field.Name, (int)field.LabelWidthResolved);
                }
            }
            
        }

        private void Field_Loaded(object sender, RoutedEventArgs e)
        {
            Field field = (sender as LabelPresenter).Field;
            field.Width = new FieldLength(GetColumnWidth(field.Name));
        }
        

        private int GetColumnWidth(string id)
        {
            switch (id)
            {
                case "ID":
                    return ApplicationContext.IDWidth;
                case "ComputerType":
                    return ApplicationContext.TypeWidth;
                case "OSName":
                    return ApplicationContext.OSNameWidth;
                case "IPv4":
                    return ApplicationContext.IPv4Width;
                case "IPv6":
                    return ApplicationContext.IPv6Width;
                case "Domain":
                    return ApplicationContext.DomainWidth;
                case "PowerState":
                    return ApplicationContext.PowerStateWidth;
                case "SystemName":
                    return ApplicationContext.SystemNameWidth;
                case "UserName":
                    return ApplicationContext.UserNameWidth;
                case "LastSync":
                    return ApplicationContext.LastSyncWidth;
                default:
                    return 100;
            }
        }

        private void SetColumnWidth(string id, int width)
        {
            switch (id)
            {
                case "ID":
                    ApplicationContext.IDWidth = width;
                    break;
                case "ComputerType":
                    ApplicationContext.TypeWidth = width;
                    break;
                case "OSName":
                    ApplicationContext.OSNameWidth = width;
                    break;
                case "IPv4":
                    ApplicationContext.IPv4Width = width;
                    break;
                case "IPv6":
                    ApplicationContext.IPv6Width = width;
                    break;
                case "Domain":
                    ApplicationContext.DomainWidth = width;
                    break;
                case "PowerState":
                    ApplicationContext.PowerStateWidth=width;
                    break;
                case "SystemName":
                    ApplicationContext.SystemNameWidth=width;
                    break;
                case "UserName":
                    ApplicationContext.UserNameWidth = width;
                    break;
                case "LastSync":
                    ApplicationContext.LastSyncWidth = width;
                    break;
                default:
                    break;
            }
            var widthConfigText =
                string.Format(
                    "SystemName:{0};Domain:{1};UserName:{2};IPv4:{3};IPv6:{4};Type:{5};OSName:{6};PowerState:{7};LastSync:{8};GroupName:{9}",ApplicationContext.SystemNameWidth, ApplicationContext.DomainWidth, ApplicationContext.UserNameWidth,ApplicationContext.IPv4Width,ApplicationContext.IPv6Width,ApplicationContext.TypeWidth, ApplicationContext.OSNameWidth, ApplicationContext.PowerStateWidth, ApplicationContext.LastSyncWidth, ApplicationContext.IDWidth);
            Functions.WriteToConfig("GROUPVIEW_WIDTH_KEY", widthConfigText);
        }

        private void EndPointDataGrid_OnSelectedItemsChanged(object sender, SelectedItemsChangedEventArgs e)
        {
            var source = e.Source as XamDataGrid;
            if (source != null)
            {
                var ep = source.ActiveDataItem as EndPoint;
                if (ep != null)
                {
                    var rightModel = PageNavigatorHelper.GetRightElementViewModel();
                    Thread.Sleep(500);
                    if (rightModel != null && rightModel.DirectoryPushed)
                    {
                        var dir = ApplicationContext.FolderListAll.Find(r => r.FolderId == ep.FolderId);
                        var listNodes = new List<int>();
                        GetParentListId(listNodes, dir);
                        PageNavigatorHelper.GetRightElementViewModel().SelectTreeNode(ep.EndpointId, listNodes, TreeViewSelectMode.Endpoint);
                    }
                    else if (rightModel != null && !rightModel.DirectoryPushed)
                    {
                        var nodeSelected = new DirectoryNode
                        {
                            NodeId = ep.EndpointId,
                            IsFolder = false
                        };
                        
                        foreach (var le in ApplicationContext.LableEndpointDatas)
                        {
                            var endp = le.EndPointDatas.Find(r => r.EndpointId == ep.EndpointId);
                            if (endp != null)
                            {
                                ApplicationContext.LabelExpandedIds.Add(le.Id);
                                break;
                            }
                        }
                        
                        PageNavigatorHelper.GetRightElementViewModel().SelectLabelNodeFromGrid(nodeSelected);
                    }
                    
                }
                
            }
            
        }

        private void GetParentListId(List<int> listNode, Directory dir)
        {
            if (dir == null) return;
            listNode.Add(dir.FolderId);
            foreach (var ep in ApplicationContext.FolderListAll)
            {
                if (dir.ParentId == ep.FolderId)
                {
                    GetParentListId(listNode, ep);
                    break;
                }
            }
        }

        private void EndPointDataGrid_ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (!ApplicationContext.IsReload)
            {
                //ApplicationContext.GroupViewVerticalOffset = EndPointDataGrid.ScrollInfo.VerticalOffset;             
            }
            else
            {
                //EndPointDataGrid.ScrollInfo.SetVerticalOffset(ApplicationContext.GroupViewVerticalOffset);
                //EndPointDataGrid.ScrollInfo.CanHorizontallyScroll = true;
                ApplicationContext.IsReload = false;
            }
        }
    }
}