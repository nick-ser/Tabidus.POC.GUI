using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Threading;
using System.Xml;
using Newtonsoft.Json;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.DirectoryAssignment;
using Tabidus.POC.Common.Model.Endpoint;
using Tabidus.POC.Common.Model.LDAP;
using Tabidus.POC.Common.Model.POCAgent;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.ServiceReference;
using Tabidus.POC.GUI.View;

namespace Tabidus.POC.GUI.Common
{
    /// <summary>
    ///     this Class includes all of common functions, example: GetConfig that get appconfig value
    /// </summary>
    public class Functions
    {
        private static readonly string Key = AppSettings.GetConfig<string>(CommonConstants.MESSAGE_KEY);

        public static string UppercaseFirst(string s)
        {
            // Check for empty string.
            if (String.IsNullOrEmpty(s))
            {
                return String.Empty;
            }
            // Return char and concat substring.
            return Char.ToUpper(s[0]) + s.Substring(1);
        }

        /// <summary>
        ///     this Method use getting appconfig value
        /// </summary>
        /// <typeparam name="T">Type of value return</typeparam>
        /// <param name="key">key of appconfig value</param>
        /// <param name="defaultValue">if appconfig value is null, value return will set defaultValue</param>
        /// <returns></returns>
        public static T GetConfig<T>(string key, T defaultValue)
        {
            if (!String.IsNullOrEmpty(key))
            {
                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                var value = config.AppSettings.Settings[key].Value;
                try
                {
                    if (value != null)
                    {
                        var theType = typeof (T);
                        if (theType.IsEnum)
                            return (T) Enum.Parse(theType, value, true);
                        return (T) Convert.ChangeType(value, theType);
                    }
                    return default(T);
                }
                catch
                {
                }
            }
            return defaultValue;
        }

        public static int AddLabelCriteria(LabelCriteria lc)
        {
            using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            {
                var datareq = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(lc),
                    Key);
                var rs = sc.AddLabelCriteria(datareq);
                ApplicationContext.IsRebuildTree = true;
                return EncryptionHelper.DecryptStringToInt(rs, Key);
            }
        }

        public static int AddDirectoryAssignmentRuleCriteria(AssignmentRuleCriteriaEnt lc)
        {
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<DataResponse>(
                sc.AddAssignmentRuleCriteria,
                lc));
            return resultDeserialize.Result;
        }

        public static void DeleteLabelCriteria(int lcid)
        {
            using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            {
                var request = new StringAuthenticateObject {StringValue = lcid.ToString(), StringAuth = "OK"};
                var datareq = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(request),
                    Key);
                sc.DeleteLabelCriteria(datareq);
                ApplicationContext.IsRebuildTree = true;
            }
        }
        public static void DeleteDirectoryAssignmentRuleCriteria(int lcid)
        {
            using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
            {
                var request = new DataRequest(lcid);
                var datareq = EncryptionHelper.EncryptString(JsonConvert.SerializeObject(request),
                    Key);
                sc.DeleteAssignRuleCriteria(datareq);
            }
        }
        public static void WriteToConfig(string configName, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            config.AppSettings.Settings.Remove(configName);
            config.AppSettings.Settings.Add(configName, value);
            config.Save(ConfigurationSaveMode.Modified);
        }

        public static string GetServerAddress()
        {
            var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
            var systemServiceModalSectionGroups = config.SectionGroups["system.serviceModel"];


            var clientSectionInformation = systemServiceModalSectionGroups.Sections["client"].SectionInformation;
            var xmlText = clientSectionInformation.GetRawXml();
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlText);

            var endpointNode = xmlDoc.SelectSingleNode("/client/endpoint[@name='NetTcpBinding_IPOCService']");
            return endpointNode.Attributes["address"].Value;
        }

        public static int GetColumnWidth(string name, string list)
        {
            var cw = list.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var gnw =
                (cw.Find(r => r.Contains(name)).Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList())[1];
            return Int32.Parse(gnw);
        }

        public static void GetPathNode(List<string> listNode, Directory dir)
        {
            if (dir == null) return;
            listNode.Add(dir.FolderName);
            foreach (var ep in ApplicationContext.FolderListAll)
            {
                if (dir.ParentId == ep.FolderId)
                {
                    GetPathNode(listNode, ep);
                    break;
                }
            }
        }

        public static void LoadFolderPolicy()
        {
            var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK"
            };
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<PolicyAssign>>(
                sc.GetFolderPolicies,
                requestObj));

            if (resultDeserialize == null)
            {
                ApplicationContext.FolderPolicyList = new List<PolicyAssign>();
            }
            else
            {
                ApplicationContext.FolderPolicyList = resultDeserialize;
            }
        }
        public static void LoadEndpointPolicy()
        {
            var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK"
            };
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<PolicyAssign>>(
                sc.GetEndpointPolicies,
                requestObj));

            if (resultDeserialize == null)
            {
                ApplicationContext.EndpointPolicyList = new List<PolicyAssign>();
            }
            else
            {
                ApplicationContext.EndpointPolicyList = resultDeserialize;
            }
        }

        public static void GetAllPolicies()
        {
            var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK"
            };
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<POCAgent>>(
                sc.GetAllPolicies,
                requestObj));

            if (resultDeserialize == null)
            {
                ApplicationContext.POCAgentList = new List<POCAgent>();
            }
            else
            {
                ApplicationContext.POCAgentList = resultDeserialize;
            }
            
        }

        public static void GetAllSoftware()
        {
            var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK"
            };
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<SoftwareContent>>(
                sc.GetAllSoftware,
                requestObj));

            if (resultDeserialize == null)
            {
                ApplicationContext.SoftwareList =  new List<SoftwareContent>();
            }
            else
            {
                ApplicationContext.SoftwareList = resultDeserialize;
            }
        }

        public static void GetAllUpdateSourceSoftware()
        {
            var requestObj = new StringAuthenticateObject
            {
                StringAuth = "OK"
            };
            var resultDeserialize = ServiceManager.Invoke(sc => RequestResponseUtils.GetData<List<UpdateSourceSoftware>>(
                sc.GetAllUpdateSourceSoftware,
                requestObj));

            if (resultDeserialize == null)
            {
                ApplicationContext.UpdateSourceSoftwareList = new List<UpdateSourceSoftware>();
            }
            else
            {
                ApplicationContext.UpdateSourceSoftwareList = resultDeserialize;
            }
        }
            }
}