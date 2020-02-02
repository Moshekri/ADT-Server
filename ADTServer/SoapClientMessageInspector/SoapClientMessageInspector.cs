using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.IO;
using AdtSvrCmn.Objects;
using System.Configuration;

namespace SoapClientMessageInspector
{
    public class SoapClientMessageInspector : IClientMessageInspector
    {
        ApplicationConfiguration _config;
        bool writeMessagesToLog = false;
        public SoapClientMessageInspector(ApplicationConfiguration config)
        {
           writeMessagesToLog =  bool.Parse( ConfigurationManager.AppSettings["WriteSoapMessagesToLog"]);

            _config = config;
        }
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
           
            string logDir = _config.LogFilePath;
            string logFileName = "ReplayFromWebService.txt";
            string path = Path.Combine(logDir, logFileName);
            AddHeadear(path);
            File.AppendAllText(path, reply.ToString());
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            string logDir = _config.LogFilePath;
            string logFileName = "QueryToWebService.txt";
            string path = Path.Combine(logDir, logFileName);
            AddHeadear(path);
            File.AppendAllText(path, request.ToString());
            return request;
        }

        private void AddHeadear(string path)
        {
            File.AppendAllText(path, "\r\n" + "***********************************" + "\r\n");
            File.AppendAllText(path, DateTime.Now.ToShortDateString() + " ");
            File.AppendAllText(path, DateTime.Now.ToLongTimeString() + "\r\n");
        }
    }
    public class MessageEndPointBehavior : IEndpointBehavior
    {
        ApplicationConfiguration _config;
        public MessageEndPointBehavior(ApplicationConfiguration config)
        {
            _config = config;
        }
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            return;
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            SoapClientMessageInspector inspector = new SoapClientMessageInspector(_config);
            clientRuntime.ClientMessageInspectors.Add(inspector); ;
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            return;
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            return;
        }
    }
}
