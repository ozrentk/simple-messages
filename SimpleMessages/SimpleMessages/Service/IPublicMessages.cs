using SimpleMessages.Models;
using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SimpleMessages.Service
{
    [ServiceContract]
    public interface IPublicMessages
    {
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*")]
        bool HandleHttpOptionsRequest();

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "echo?text={text}")]
        string Echo(string text);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            Method ="POST",
            UriTemplate = "users")]
        Guid? RegisterUser(User user);
    }
}
