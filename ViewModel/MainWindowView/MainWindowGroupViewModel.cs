using System;
using System.Linq;
using Tabidus.POC.GUI.Common;

namespace Tabidus.POC.GUI.ViewModel.MainWindowView
{
    public partial class MainWindowViewModel
    {
        partial void MainWindowGroupViewModel()
        {
            //set default endpoint group grid column header width
            var groupColumnWidth = Functions.GetConfig("GROUPVIEW_WIDTH_KEY", "");
            var neighborhoodColumnWidth = Functions.GetConfig("NEIGHBORHOOD_VIEW_WIDTH_KEY", "");
            ApplicationContext.IDWidth = GetColumnWidth("GroupName", groupColumnWidth);
            ApplicationContext.TypeWidth = GetColumnWidth("Type", groupColumnWidth);
            ApplicationContext.DomainWidth = GetColumnWidth("Domain", groupColumnWidth);
            ApplicationContext.SystemNameWidth = GetColumnWidth("SystemName", groupColumnWidth);
            ApplicationContext.UserNameWidth = GetColumnWidth("UserName", groupColumnWidth);
            ApplicationContext.IPv4Width = GetColumnWidth("IPv4", groupColumnWidth);
            ApplicationContext.IPv6Width = GetColumnWidth("IPv6", groupColumnWidth);
            ApplicationContext.OSNameWidth = GetColumnWidth("OSName", groupColumnWidth);
            ApplicationContext.PowerStateWidth = GetColumnWidth("PowerState", groupColumnWidth);
            ApplicationContext.LastSyncWidth = GetColumnWidth("LastSync", groupColumnWidth);

            //set default neighborhood watch grid column header width
            ApplicationContext.NeMACWidth = GetColumnWidth("MAC", neighborhoodColumnWidth);
            ApplicationContext.NeDetectedByWidth = GetColumnWidth("DetectedBy", neighborhoodColumnWidth);
            ApplicationContext.NeDomainWidth = GetColumnWidth("Domain", neighborhoodColumnWidth);
            ApplicationContext.NeSystemNameWidth = GetColumnWidth("SystemName", neighborhoodColumnWidth);
            ApplicationContext.NeVendorWidth = GetColumnWidth("Vendor", neighborhoodColumnWidth);
            ApplicationContext.NeIPv4Width = GetColumnWidth("IPv4", neighborhoodColumnWidth);
            ApplicationContext.NeIPv6Width = GetColumnWidth("IPv6", neighborhoodColumnWidth);
            ApplicationContext.NeOSNameWidth = GetColumnWidth("OSName", neighborhoodColumnWidth);
            ApplicationContext.NeLastSeenWidth = GetColumnWidth("LastSeen", neighborhoodColumnWidth);
            ApplicationContext.NeConfirmedWidth = GetColumnWidth("Confirmed", neighborhoodColumnWidth);
        }
        #region Private methods

        private int GetColumnWidth(string name, string list)
        {
            var cw = list.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var gnw =
                (cw.Find(r => r.Contains(name)).Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList())[1];
            return Int32.Parse(gnw);
        }
       


        #endregion
    }
}
