using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace SimpleMessages.Svc.Extensions
{
    public class CorsEnabler : IEndpointBehavior, IDispatchMessageInspector
    {
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            endpointDispatcher.DispatchRuntime.MessageInspectors.Add(this);
        }

        public void Validate(ServiceEndpoint endpoint)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var requiredHeaders = new Dictionary<string, string>
            {
              { "Access-Control-Allow-Origin", "*" },
              { "Access-Control-Request-Method", "GET,POST,PUT,DELETE,OPTIONS" },
              { "Access-Control-Allow-Methods", "GET,POST,PUT,DELETE,OPTIONS" },
              //{ "Access-Control-Allow-Headers", "X-Requested-With,Content-Type,Safe-Repository-Path,Safe-Repository-Token" }
              { "Access-Control-Allow-Headers", "Content-Type,X-SimpleMessages-SvcToken" }
              //{ "Access-Control-Allow-Credentials", "true" },
            };

            var httpHeader = reply.Properties["httpResponse"] as HttpResponseMessageProperty;
            foreach (var item in requiredHeaders)
            {
                httpHeader.Headers.Add(item.Key, item.Value);
            }

        }
    }
}
