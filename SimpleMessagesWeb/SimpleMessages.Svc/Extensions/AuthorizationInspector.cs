using SimpleMessages.Svc.Attributes;
using SimpleMessages.Svc.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Channels;

namespace SimpleMessages.Svc.Extensions
{
    public class AuthorizationInspector : IEndpointBehavior, IDispatchMessageInspector //IParameterInspector
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        [System.Diagnostics.DebuggerHidden]
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            var propDict = request.Properties.ToDictionary(k => k.Key, k => k.Value);
            if (!propDict.ContainsKey("HttpOperationName"))
                return null;

            var opName = propDict["HttpOperationName"].ToString();
            var method = typeof(MessageService).GetMethod(opName);
            var attrs = Attribute.GetCustomAttributes(method, typeof(AuthorizeAttribute), true);
            var attr = (AuthorizeAttribute)attrs.FirstOrDefault();

            if (attr == null)
                return null;

            // Check authorization
            if (!attr.CheckRoles())
            {
                throw new FaultException("ERROR: authorization");
            }

            return null;
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
        }
    }
}
