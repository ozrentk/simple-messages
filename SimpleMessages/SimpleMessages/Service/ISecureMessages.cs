using SimpleMessages.Models;
using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SimpleMessages.Service
{
    [ServiceContract]
    public interface ISecureMessages
    {
        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "delivery")]
        Delivery Receive();

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "message?search={token}&from={from}&to={to}&fromDate={fromDate}&toDate={toDate}&includeDeleted={includeDeleted}")]
        Message[] FindMessages(string from, string to, DateTime timeFrom, DateTime timeTo, string token, bool includeDeleted = false);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            Method = "PUT",
            UriTemplate = "message?to={to}")]
        Guid? Send(string to, Message message);

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
            UriTemplate = "notifications?to={to}&status={status}")]
        void NotifyStatuses(string to, MessageStatusType status);

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "user?search={token}")]
        User[] FindUsers(string token);
    }
}
