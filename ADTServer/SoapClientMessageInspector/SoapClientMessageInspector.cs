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

namespace SoapClientMessageInspector
{
    public class SoapClientMessageInspector : IClientMessageInspector
    {
        
        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            File.WriteAllText("c:\\logs\\reply.txt", reply.ToString());
        }

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            File.WriteAllText("c:\\logs\\request.txt" , request.ToString());
            return request;
        }
    }
    public class MessageEndPointBehavior : IEndpointBehavior
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            return;
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            SoapClientMessageInspector inspector = new SoapClientMessageInspector();
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
