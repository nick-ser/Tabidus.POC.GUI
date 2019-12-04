﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18408
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Tabidus.POC.GUI.MainUpdateSource {
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="MainUpdateSource.IMainUpdateSourceService")]
    public interface IMainUpdateSourceService {
        
        // CODEGEN: Generating message contract since the wrapper name (MainFileUploadRequest) of message MainFileUploadRequest does not match the default value (UploadFile)
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMainUpdateSourceService/UploadFile", ReplyAction="http://tempuri.org/IMainUpdateSourceService/UploadFileResponse")]
        Tabidus.POC.GUI.MainUpdateSource.MessageResult UploadFile(Tabidus.POC.GUI.MainUpdateSource.MainFileUploadRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMainUpdateSourceService/UploadFile", ReplyAction="http://tempuri.org/IMainUpdateSourceService/UploadFileResponse")]
        System.Threading.Tasks.Task<Tabidus.POC.GUI.MainUpdateSource.MessageResult> UploadFileAsync(Tabidus.POC.GUI.MainUpdateSource.MainFileUploadRequest request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMainUpdateSourceService/TransferToClientAgent", ReplyAction="http://tempuri.org/IMainUpdateSourceService/TransferToClientAgentResponse")]
        string TransferToClientAgent(string request);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IMainUpdateSourceService/TransferToClientAgent", ReplyAction="http://tempuri.org/IMainUpdateSourceService/TransferToClientAgentResponse")]
        System.Threading.Tasks.Task<string> TransferToClientAgentAsync(string request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="MainFileUploadRequest", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class MainFileUploadRequest {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public string Checksum;
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public string Name;
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public string SecurityKey;
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public string VirtualPath;
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="http://tempuri.org/", Order=0)]
        public System.IO.Stream DataStream;
        
        public MainFileUploadRequest() {
        }
        
        public MainFileUploadRequest(string Checksum, string Name, string SecurityKey, string VirtualPath, System.IO.Stream DataStream) {
            this.Checksum = Checksum;
            this.Name = Name;
            this.SecurityKey = SecurityKey;
            this.VirtualPath = VirtualPath;
            this.DataStream = DataStream;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(WrapperName="MessageResult", WrapperNamespace="http://tempuri.org/", IsWrapped=true)]
    public partial class MessageResult {
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public string Message;
        
        [System.ServiceModel.MessageHeaderAttribute(Namespace="http://tempuri.org/")]
        public bool Status;
        
        public MessageResult() {
        }
        
        public MessageResult(string Message, bool Status) {
            this.Message = Message;
            this.Status = Status;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IMainUpdateSourceServiceChannel : Tabidus.POC.GUI.MainUpdateSource.IMainUpdateSourceService, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class MainUpdateSourceServiceClient : System.ServiceModel.ClientBase<Tabidus.POC.GUI.MainUpdateSource.IMainUpdateSourceService>, Tabidus.POC.GUI.MainUpdateSource.IMainUpdateSourceService {
        
        public MainUpdateSourceServiceClient() {
        }
        
        public MainUpdateSourceServiceClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public MainUpdateSourceServiceClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MainUpdateSourceServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public MainUpdateSourceServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Tabidus.POC.GUI.MainUpdateSource.MessageResult Tabidus.POC.GUI.MainUpdateSource.IMainUpdateSourceService.UploadFile(Tabidus.POC.GUI.MainUpdateSource.MainFileUploadRequest request) {
            return base.Channel.UploadFile(request);
        }
        
        public string UploadFile(string Checksum, string Name, string SecurityKey, string VirtualPath, System.IO.Stream DataStream, out bool Status) {
            Tabidus.POC.GUI.MainUpdateSource.MainFileUploadRequest inValue = new Tabidus.POC.GUI.MainUpdateSource.MainFileUploadRequest();
            inValue.Checksum = Checksum;
            inValue.Name = Name;
            inValue.SecurityKey = SecurityKey;
            inValue.VirtualPath = VirtualPath;
            inValue.DataStream = DataStream;
            Tabidus.POC.GUI.MainUpdateSource.MessageResult retVal = ((Tabidus.POC.GUI.MainUpdateSource.IMainUpdateSourceService)(this)).UploadFile(inValue);
            Status = retVal.Status;
            return retVal.Message;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<Tabidus.POC.GUI.MainUpdateSource.MessageResult> Tabidus.POC.GUI.MainUpdateSource.IMainUpdateSourceService.UploadFileAsync(Tabidus.POC.GUI.MainUpdateSource.MainFileUploadRequest request) {
            return base.Channel.UploadFileAsync(request);
        }
        
        public System.Threading.Tasks.Task<Tabidus.POC.GUI.MainUpdateSource.MessageResult> UploadFileAsync(string Checksum, string Name, string SecurityKey, string VirtualPath, System.IO.Stream DataStream) {
            Tabidus.POC.GUI.MainUpdateSource.MainFileUploadRequest inValue = new Tabidus.POC.GUI.MainUpdateSource.MainFileUploadRequest();
            inValue.Checksum = Checksum;
            inValue.Name = Name;
            inValue.SecurityKey = SecurityKey;
            inValue.VirtualPath = VirtualPath;
            inValue.DataStream = DataStream;
            return ((Tabidus.POC.GUI.MainUpdateSource.IMainUpdateSourceService)(this)).UploadFileAsync(inValue);
        }
        
        public string TransferToClientAgent(string request) {
            return base.Channel.TransferToClientAgent(request);
        }
        
        public System.Threading.Tasks.Task<string> TransferToClientAgentAsync(string request) {
            return base.Channel.TransferToClientAgentAsync(request);
        }
    }
}