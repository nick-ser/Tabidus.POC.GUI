using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using Newtonsoft.Json;
using Tabidus.POC.Common.Constants;
using Tabidus.POC.Common.DataRequest;
using Tabidus.POC.Common.DataResponse;
using Tabidus.POC.Common.Model;
using Tabidus.POC.Common.Model.Software;
using Tabidus.POC.Common.Utils;
using Tabidus.POC.EncryptDecryptHelper;
using Tabidus.POC.GUI.MainUpdateSource;
using Tabidus.POC.GUI.ServiceReference;

namespace Tabidus.POC.GUI.Common
{
	public delegate T ServiceClientHandler<out T>(POCServiceClient sc);

	public delegate void ServiceClientHandler(POCServiceClient sc);
	public class ServiceManager
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(ServiceManager));
		public static T Invoke<T>(ServiceClientHandler<T> handler)
		{
			var sc = new POCServiceClient("NetTcpBinding_IPOCService");

			try
			{
				_log.Info("ServiceClientHandler: " + handler.Method);
				return handler(sc);
			}
			catch (TimeoutException ex)
			{
				_log.Error("Timeout error: {0}" + ex.Message, ex);
				sc.Abort();
			}
			catch (FaultException fe)
			{
				_log.Error("Timeout error: {0}" + fe.Message, fe);
				sc.Abort();
			}
			catch (CommunicationException ce)
			{
				_log.Error("Timeout error: {0}" + ce.Message, ce);
				sc.Abort();
			}
			finally
			{
				if (sc.State != CommunicationState.Closed)
					sc.Close();
			}
			return default(T);
		}

		public static void Invoke(ServiceClientHandler handler)
		{
			var sc = new POCServiceClient("NetTcpBinding_IPOCService");
			try
			{
				_log.Info("ServiceClientHandler: " + handler.Method);
				handler(sc);
			}
			catch (TimeoutException ex)
			{
				_log.Error("Timeout error: {0}" + ex.Message, ex);
				sc.Abort();
			}
			catch (FaultException fe)
			{
				_log.Error("Timeout error: {0}" + fe.Message, fe);
				sc.Abort();
			}
			catch (CommunicationException ce)
			{
				_log.Error("Timeout error: {0}" + ce.Message, ce);
				sc.Abort();
			}
			finally
			{
				if (sc.State != CommunicationState.Closed)
					sc.Close();
			}
		}

	    private static POCServiceClient pocClientServiceForAutoRefresh;
		public static List<LastUpdated> GetLastUpdateData()
		{
			try
			{
                if (pocClientServiceForAutoRefresh == null)
                    pocClientServiceForAutoRefresh = new POCServiceClient("NetTcpBinding_IPOCService");
			    if (pocClientServiceForAutoRefresh.ChannelFactory.State == CommunicationState.Faulted
			        || pocClientServiceForAutoRefresh.ChannelFactory.State == CommunicationState.Closed)
			    {
			        pocClientServiceForAutoRefresh.Close();
                    pocClientServiceForAutoRefresh = null;
                    pocClientServiceForAutoRefresh = new POCServiceClient("NetTcpBinding_IPOCService");
			    }
                var lastUpdate = pocClientServiceForAutoRefresh.GetLastUpdateData();
                var decryptionData = lastUpdate;
                var lupds = JsonConvert.DeserializeObject<List<LastUpdated>>(decryptionData);
                return lupds;
			}
			catch (Exception ex)
			{
				_log.Error(ex);
				throw;
			}
		}

		public static DataResponse Upload(FileUploadRequest request, string checkSum)
		{
			var dataResponse = new DataResponse(false, -1);
			try
			{
				using (var fileStream = new FileStream(request.VirtualPath, FileMode.Open, FileAccess.Read))
				{
					request.DataStream = fileStream;
					using (var sc = new MainUpdateSourceServiceClient("BasicHttpBinding_IMainUpdateSourceService", ApplicationContext.MainUpdateSourceUrl))
					{
						bool status;
						//Return Task<MessageResult>
						var strResult = sc.UploadFile(checkSum, request.Name, request.SecurityKey,
													request.VirtualPath, request.DataStream, out status);
						dataResponse = RequestResponseUtils.GetData<DataResponse>(strResult);
					}
				}
			}
			catch (Exception ex)
			{
				dataResponse.Message = ex.Message;
				//Log
				_log.Error("ServiceManager.Upload Error", ex);
			}
			return dataResponse;
		}

		public static double TransferToServerAgent(TransferToAgentDataRequest request)
		{
			var dataResponse = Invoke(sc => RequestResponseUtils.GetData<TransferToAgentDataResponse>(
				sc.TransferToServerAgent,
				request));
			return dataResponse.TotalSize;
		}
		public static DeleteSoftwareDataResponse DeleteSoftwareFile(List<string> filesToDelete)
		{
			try
			{
				var request = new FilesToDeleteRequest
				{
					SecurityKey = RequestResponseUtils.EncryptString(AppSettings.GetConfig<string>(CommonConstants.MESSAGE_KEY)),
					FilesToDelete = filesToDelete
				};
				return Invoke(sc => RequestResponseUtils.GetData<DeleteSoftwareDataResponse>(sc.DeleteSoftwareFiles, request));
			}
			catch (Exception ex)
			{
				_log.Error("DeleteSoftwareFile Error", ex);
				return new DeleteSoftwareDataResponse(false, new DeleteSoftwareResult());
			}
		}

		public static void UpdateMainUpdateSourceConfig()
		{
			try
			{
				using (var sc = new POCServiceClient("NetTcpBinding_IPOCService"))
				{
					var sourceUrl = RequestResponseUtils.DecryptString(
							sc.GetMainUpdateSourceUrl()
						);

					if (!string.IsNullOrWhiteSpace(sourceUrl))
					{
						ApplicationContext.MainUpdateSourceUrl = sourceUrl;
					}
					else
					{
						var config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
						var systemServiceModalSectionGroups = config.SectionGroups["system.serviceModel"];

						var servicesSectionInformation = systemServiceModalSectionGroups.Sections["client"].SectionInformation;
						var xmlText = servicesSectionInformation.GetRawXml();
						var xmlDoc = new XmlDocument();
						xmlDoc.LoadXml(xmlText);

						var endpointNode =
						 xmlDoc.SelectSingleNode(
							  "/client/endpoint[@name='BasicHttpBinding_IMainUpdateSourceService']");
						if (endpointNode != null)
						{
							ApplicationContext.MainUpdateSourceUrl = endpointNode.Attributes["address"].Value;
						}
					}
				}
			}
			catch (Exception ex)
			{
				_log.Error("UpdateMainUpdateSourceConfig: {0}" + ex.Message, ex);
				DialogHelper.Error("Unable get MainUpdateSource service url");
			}
			
		}
	}
}
