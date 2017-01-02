using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace SimpleMessages.Extensions
{
    public class DispatchLogger : IEndpointBehavior, IDispatchMessageInspector
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
            var propDict = request.Properties.ToDictionary(k => k.Key, k => k.Value);

            var method = ((HttpRequestMessageProperty)propDict["httpRequest"]).Method;
            var uri = ((UriTemplateMatch)propDict["UriTemplateMatchResults"]).RequestUri.ToString();
            var opName = propDict["HttpOperationName"].ToString();

            Console.WriteLine(String.Format("REQ: {0} {1} -> '{2}'", method, uri, opName));
            if (!request.IsEmpty)
                Console.WriteLine(request);
            Console.WriteLine();

            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods")]
        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            var propDict = reply.Properties.ToDictionary(k => k.Key, k => k.Value);

            var httpRespProp = propDict["httpResponse"];
            var respMsgProp = (HttpResponseMessageProperty)httpRespProp;

            Console.WriteLine(String.Format("RSP: {0} {1} - '{2}'", (int)respMsgProp.StatusCode, respMsgProp.StatusCode, respMsgProp.StatusDescription));
            if (!reply.IsEmpty)
            {
                // Copy response body
                var buffer = reply.CreateBufferedCopy(Int32.MaxValue);
                reply = buffer.CreateMessage();
                var duplicateMsg = buffer.CreateMessage();
                Console.WriteLine(duplicateMsg);
                //Console.WriteLine("[response sent]");
            }
            Console.WriteLine();
        }
    }
}
