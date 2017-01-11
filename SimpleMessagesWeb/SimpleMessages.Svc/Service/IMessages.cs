using SimpleMessages.DB.Models;
using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SimpleMessages.Svc.Service
{
    [ServiceContract]
    public interface IMessages
    {
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*")]
        bool HandleHttpOptionsRequest();

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "echo?text={text}")]
        string Echo(string text);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            Method = "PUT",
            UriTemplate = "message?from={from}&to={to}")]
        Message Send(string from, string to, Message message);

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "delivery?to={to}")]
        Delivery Receive(string to);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            Method = "POST",
            UriTemplate = "user")]
        User RegisterUser(User user);

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "message?search={token}&from={from}&to={to}&fromDate={timeFrom}&toDate={timeTo}&includeDeleted={includeDeleted}")]
        Message[] FindMessages(string from, string to, DateTime timeFrom, DateTime timeTo, string token, bool includeDeleted = false);

        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "message?guid={guid}")]
        void UpdateMessage(Guid guid, Message message);

        [OperationContract]
        [WebInvoke(Method = "DELETE", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "message?guid={guid}")]
        void RemoveMessage(Guid guid);

        [OperationContract]
        [WebInvoke(Method = "DELETE", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "notifications?from={from}&to={to}&timeFrom={timeFrom}&timeTo={timeTo}")]
        Guid[] RemoveMessages(string from, string to, DateTime timeFrom, DateTime timeTo);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
        [OperationContract]
        [WebInvoke(Method = "PUT", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "notification?guid={guid}&status={status}")]
        void NotifyStatus(Guid guid, MessageStatusType status);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
        [OperationContract]
        [WebInvoke(Method = "PUT", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "notifications?from={from}&to={to}&status={status}")]
        void NotifyStatuses(string from, string to, MessageStatusType status);

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "user?search={token}")]
        string[] FindUserNames(string token);
    }
}
