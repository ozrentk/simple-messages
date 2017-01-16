using SimpleMessages.DB.Models;
using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace SimpleMessages.Svc.Service
{
    [ServiceContract]
    public interface IMessages
    {
        /// <summary>
        /// Pre-flighted CORS requests start with OPTIONS-method request
        /// <see cref="https://developer.mozilla.org/en-US/docs/Web/HTTP/Access_control_CORS"/>
        /// </summary>
        [WebInvoke(Method = "OPTIONS", UriTemplate = "*")]
        bool HandleHttpOptionsRequest();

        /// <summary>
        /// Just an echo
        /// </summary>
        /// <returns>Echo</returns>
        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "echo?text={text}")]
        string Echo(string text);

        /// <summary>
        /// The only way for the client to send a message to the other client.
        /// Administrator can impersonate another client.
        /// </summary>
        /// <param name="from">The sending client</param>
        /// <param name="to">The receiving client</param>
        /// <param name="message">The message content</param>
        /// <returns>The message</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            Method = "PUT",
            UriTemplate = "message?from={from}&to={to}")]
        Message Send(string from, string to, Message message);

        /// <summary>
        /// Receives all the messages and the notifications for the specified client
        /// </summary>
        /// <param name="to">The receiving client</param>
        /// <returns>Delivery = all the messages along with notifications</returns>
        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "delivery?to={to}")]
        Delivery Receive(string to);

        /// <summary>
        /// Should be for external user registration (e.g. bulk registration via some service etc.)
        /// </summary>
        /// <param name="user">Data for creation of the new user</param>
        /// <returns>The newly created user</returns>
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            Method = "POST",
            UriTemplate = "user")]
        User RegisterUser(User user);

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "message?search={token}&from={from}&to={to}&fromDate={timeFrom}&toDate={timeTo}&includeDeleted={includeDeleted}")]
        Message[] FindMessages(string from, string to, DateTime timeFrom, DateTime timeTo, string token, bool includeDeleted = false);

        /// <summary>
        /// The only way for the client to update a message
        /// </summary>
        /// <param name="guid">The message identifier</param>
        /// <param name="message">The message being updated</param>
        [OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "message?guid={guid}")]
        void UpdateMessage(Guid guid, Message message);

        /// <summary>
        /// The way for a client to delete a specific message
        /// </summary>
        /// <param name="guid">The message identifier</param>
        [OperationContract]
        [WebInvoke(Method = "DELETE", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "message?guid={guid}")]
        void RemoveMessage(Guid guid);

        [OperationContract]
        [WebInvoke(Method = "DELETE", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "notifications?from={from}&to={to}&timeFrom={timeFrom}&timeTo={timeTo}")]
        Guid[] RemoveMessages(string from, string to, DateTime timeFrom, DateTime timeTo);

        /// <summary>
        /// The way for a client to notify the other side of the message reception or reading (appearance on the screen)
        /// </summary>
        /// <param name="guid">The message identifier</param>
        /// <param name="status">The message status</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames")]
        [OperationContract]
        [WebInvoke(Method = "PUT", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "notification?guid={guid}&status={status}")]
        void NotifyMessageStatus(Guid guid, MessageStatusType status);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords")]
        [OperationContract]
        [WebInvoke(Method = "PUT", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "notifications?from={from}&to={to}&status={status}")]
        void NotifyMessageStatuses(string from, string to, MessageStatusType status);

        /// <summary>
        /// The way for the client to receive information of the registered users
        /// </summary>
        /// <param name="token">Search token (e.g. token "an" matches user "Vedran" or "Ana" etc.)</param>
        /// <returns>Array of matched user names</returns>
        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "user?search={token}")]
        string[] FindUserNames(string token);
    }
}
