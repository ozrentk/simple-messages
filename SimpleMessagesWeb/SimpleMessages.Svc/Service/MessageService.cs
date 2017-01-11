using SimpleMessages.DB;
using SimpleMessages.DB.Exceptions;
using SimpleMessages.DB.Models;
using SimpleMessages.Svc.Attributes;
using SimpleMessages.Svc.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.ServiceModel.Web;

namespace SimpleMessages.Svc.Service
{
    public class MessageService : IMessages //IPublicMessages, ISecureMessages
    {
        private readonly Database _database;

        public MessageService()
        {
            var connStr = ConfigurationManager.ConnectionStrings["MessagesDb"].ConnectionString;
            _database = new Database(connStr);
        }

        private static bool ValidateModel<T>(T obj, ref HttpStatusCode httpCode, ref string httpDescription)
            where T : class
        {
            string[] errors = null;
            try
            {
                errors = ValidationHelper.Validate(obj);
            }
            catch (Exception)
            {
                httpCode = HttpStatusCode.InternalServerError;
                httpDescription = "Validation unexpectedly failed";
                return false;
            }

            if (errors.Length > 0)
            {
                httpCode = HttpStatusCode.BadRequest;
                var err = errors[0]; // TODO: bad practice?
                httpDescription = err;

                return false;
            }

            return true;
        }

        public bool HandleHttpOptionsRequest()
        {
            if (WebOperationContext.Current != null && WebOperationContext.Current.IncomingRequest.Method == "OPTIONS")
            {
                //WebOperationContext.Current.OutgoingResponse.Headers.Add("Access-Control-Allow-Origin", "*");
                return true;
            }

            return false;
        }

        public string Echo(string text)
        {
            //var mgr = new Identity.UserManager(new Identity.UserStore());
            //var user = mgr.FindAsync("ozren.krznaric@gmail.com", "oz.123").Result;

            //var cookie = WebOperationContext.Current.IncomingRequest.Headers[HttpRequestHeader.Cookie];

            Console.WriteLine("Echo: {0}", text);
            return text;
        }

        public User RegisterUser(User user)
        {
            // TODO: For automation - register user via service
            throw new NotImplementedException("RegisterUser");
        }

        [Authorize(Roles: "User")]
        public Delivery Receive(string to)
        {
            HttpStatusCode httpCode = HttpStatusCode.OK;
            string httpDescription = "";

            WebOperationContext ctx = WebOperationContext.Current;

            Message[] messages = null;
            Notification[] notifications = null;
            try
            {
                messages = _database.GetMessages(to, lastStatus: MessageStatusType.Sent);
                notifications = _database.GetNotifications(to);

                httpCode = HttpStatusCode.OK;
                httpDescription = String.Format("Receiving {0} messages and {1} notifications", messages.Length, notifications.Length);
            }
            catch (StorageException ex)
            {
                Console.WriteLine("DATABASE: {0}", ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);

                httpCode = HttpStatusCode.InternalServerError;
                httpDescription = "Failed";
            }

            var newDelivery = new Delivery
            {
                Messages = messages,
                Notifications = notifications
            };

            ctx.OutgoingResponse.StatusCode = httpCode;
            ctx.OutgoingResponse.StatusDescription = httpDescription;

            return newDelivery;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Argument assigned, model validated")]
        [Authorize(Roles: "User")]
        public Message Send(string from, string to, Message message)
        {

            message.From = from;
            message.To = to;

            HttpStatusCode httpCode = HttpStatusCode.OK;
            string httpDescription = "";

            Message dbMessage = null;
            WebOperationContext ctx = WebOperationContext.Current;
            if (message == null)
            {
                ctx.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                ctx.OutgoingResponse.StatusDescription = "Input data missing";
                return null;
            }
            else if (ValidateModel(message, ref httpCode, ref httpDescription))
            {
                try
                {
                    dbMessage = _database.ExecuteAndGetRow<Message>(
                        "AddMessage",
                        new Dictionary<string, object> {
                            { "FromName", message.From },
                            { "ToName", message.To },
                            { "Content", message.Content }
                        },
                        (columns) => new Message
                        {
                            From = message.From,
                            To = message.To,
                            Content = message.Content,
                            Guid = Guid.Parse(columns["Guid"].ToString()),
                            CreatedAt = DateTime.Parse(columns["CreatedAt"].ToString()),
                            Status = (MessageStatusType)int.Parse(columns["StatusId"].ToString())
                        });

                    httpCode = HttpStatusCode.OK;
                    httpDescription = String.Format("Message created");
                }
                catch (StorageException ex)
                {
                    Console.WriteLine("DATABASE: {0}", ex.Message);
                    if (ex.InnerException != null)
                        Console.WriteLine(ex.InnerException.Message);

                    httpCode = HttpStatusCode.InternalServerError;
                    httpDescription = "Failed";
                }
            }

            ctx.OutgoingResponse.StatusCode = httpCode;
            ctx.OutgoingResponse.StatusDescription = httpDescription;

            return dbMessage;
        }

        public void NotifyStatus(Guid guid, MessageStatusType status)
        {
            var notification = new Notification
            {
                MessageGuid = guid,
                MessageStatus = status
            };

            HttpStatusCode httpCode = HttpStatusCode.OK;
            string httpDescription = "";

            WebOperationContext ctx = WebOperationContext.Current;
            if (notification == null)
            {
                ctx.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                ctx.OutgoingResponse.StatusDescription = "Input data missing";
                return;
            }
            else if (ValidateModel(notification, ref httpCode, ref httpDescription))
            {
                try
                {
                    var messageProcessed = _database.SetMessageStatus(notification.MessageGuid, notification.MessageStatus);

                    httpCode = HttpStatusCode.OK;
                    if (messageProcessed)
                        httpDescription = String.Format("Status for message {0} set to {1}, sender will be notified", notification.MessageGuid, notification.MessageStatus.ToString());
                    else
                        httpDescription = String.Format("No such message");

                }
                catch (StorageException ex)
                {
                    Console.WriteLine("DATABASE: {0}", ex.Message);
                    if (ex.InnerException != null)
                        Console.WriteLine(ex.InnerException.Message);

                    httpCode = HttpStatusCode.InternalServerError;
                    httpDescription = "Failed";
                }
            }

            ctx.OutgoingResponse.StatusCode = httpCode;
            ctx.OutgoingResponse.StatusDescription = httpDescription;
        }

        public void NotifyStatuses(string from, string to, MessageStatusType status)
        {
            var notification = new AggregateNotification
            {
                MessageReceiver = from,
                ForUser = to,
                MessageStatus = status
            };

            HttpStatusCode httpCode = HttpStatusCode.OK;
            string httpDescription = "";

            WebOperationContext ctx = WebOperationContext.Current;
            if (notification == null)
            {
                ctx.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                ctx.OutgoingResponse.StatusDescription = "Input data missing";
                return;
            }
            else if (ValidateModel(notification, ref httpCode, ref httpDescription))
            {
                try
                {
                    var guids = _database.SetMessageStatus(notification.ForUser, notification.MessageReceiver, notification.MessageStatus);

                    httpCode = HttpStatusCode.OK;
                    if (guids.Length > 0)
                        httpDescription = String.Format("Statuses for {0} messages set to {1}; notifications for user sent", guids.Length, notification.MessageStatus/*, notification.ForUser*/);
                    else
                        httpDescription = String.Format("There is nothing to notify");

                }
                catch (StorageException ex)
                {
                    Console.WriteLine("DATABASE: {0}", ex.Message);
                    if (ex.InnerException != null)
                        Console.WriteLine(ex.InnerException.Message);

                    httpCode = HttpStatusCode.InternalServerError;
                    httpDescription = "Failed";
                }
            }

            ctx.OutgoingResponse.StatusCode = httpCode;
            ctx.OutgoingResponse.StatusDescription = httpDescription;
        }

        public string[] FindUserNames(string token)
        {
            HttpStatusCode httpCode = HttpStatusCode.OK;
            string httpDescription = "";

            WebOperationContext ctx = WebOperationContext.Current;

            string[] users = null;
            try
            {
                users = _database.FindUsers(token);

                httpCode = HttpStatusCode.OK;
                httpDescription = String.Format("Found {0} users", users.Length);
            }
            catch (StorageException ex)
            {
                Console.WriteLine("DATABASE: {0}", ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);

                httpCode = HttpStatusCode.InternalServerError;
                httpDescription = "Failed";
            }

            ctx.OutgoingResponse.StatusCode = httpCode;
            ctx.OutgoingResponse.StatusDescription = httpDescription;

            return users;
        }

        public Message[] FindMessages(string from, string to, DateTime timeFrom, DateTime timeTo, string searchText, bool includeDeleted = false)
        {
            WebOperationContext ctx = WebOperationContext.Current;
            if (String.IsNullOrEmpty(to))
            {
                ctx.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                ctx.OutgoingResponse.StatusDescription = "Input data missing";
                return null;
            }

            HttpStatusCode httpCode = HttpStatusCode.OK;
            string httpDescription = "";

            Message[] messages = null;
            try
            {
                messages = 
                    _database.FindMessages(
                        from, 
                        to, 
                        (timeFrom == DateTime.MinValue ? (DateTime?)null : timeFrom),
                        (timeTo == DateTime.MinValue ? (DateTime?)null : timeTo),
                        searchText, 
                        includeDeleted);

                httpCode = HttpStatusCode.OK;
                httpDescription = String.Format("Found {0} messages", messages.Length);
            }
            catch (StorageException ex)
            {
                Console.WriteLine("DATABASE: {0}", ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);

                httpCode = HttpStatusCode.InternalServerError;
                httpDescription = "Failed";
            }

            ctx.OutgoingResponse.StatusCode = httpCode;
            ctx.OutgoingResponse.StatusDescription = httpDescription;

            return messages;
        }

        public void UpdateMessage(Guid guid, Message message)
        {
            HttpStatusCode httpCode = HttpStatusCode.OK;
            string httpDescription = "";

            WebOperationContext ctx = WebOperationContext.Current;
            if (message == null || String.IsNullOrEmpty(message.Content))
            {
                ctx.OutgoingResponse.StatusCode = HttpStatusCode.BadRequest;
                ctx.OutgoingResponse.StatusDescription = "Input data missing";
                return;
            }

            try
            {
                var messageUpdated = _database.UpdateMessageContent(guid, message.Content);

                httpCode = HttpStatusCode.OK;
                if (messageUpdated)
                    httpDescription = String.Format("Content of message {0} succesfully updated, receiver will be notified", guid);
                else
                    httpDescription = String.Format("No such message");
            }
            catch (StorageException ex)
            {
                Console.WriteLine("DATABASE: {0}", ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);

                httpCode = HttpStatusCode.InternalServerError;
                httpDescription = "Failed";
            }

            ctx.OutgoingResponse.StatusCode = httpCode;
            ctx.OutgoingResponse.StatusDescription = httpDescription;
        }

        public void RemoveMessage(Guid guid)
        {
            HttpStatusCode httpCode = HttpStatusCode.OK;
            string httpDescription = "";

            WebOperationContext ctx = WebOperationContext.Current;

            try
            {
                var messageDeleted = _database.DeleteMessage(guid);

                httpCode = HttpStatusCode.OK;
                if (messageDeleted)
                    httpDescription = String.Format("Message {0} succesfully deleted, receiver will be notified", guid);
                else
                    httpDescription = String.Format("No such message");
            }
            catch (StorageException ex)
            {
                Console.WriteLine("DATABASE: {0}", ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);

                httpCode = HttpStatusCode.InternalServerError;
                httpDescription = "Failed";
            }

            ctx.OutgoingResponse.StatusCode = httpCode;
            ctx.OutgoingResponse.StatusDescription = httpDescription;
        }

        public Guid[] RemoveMessages(string from, string to, DateTime timeFrom, DateTime timeTo)
        {
            HttpStatusCode httpCode = HttpStatusCode.OK;
            string httpDescription = "";

            WebOperationContext ctx = WebOperationContext.Current;

            Guid[] deletedGuids = null;
            try
            {
                deletedGuids = _database.DeleteMessages(
                    from,
                    to,
                    (timeFrom == DateTime.MinValue ? (DateTime?)null : timeFrom),
                    (timeTo == DateTime.MinValue ? (DateTime?)null : timeTo));

                httpCode = HttpStatusCode.OK;

                if (deletedGuids.Length > 0)
                    httpDescription = String.Format("{0} messages succesfully deleted, receiver will be notified", deletedGuids.Length);
                else
                    httpDescription = String.Format("No messages found to delete");
            }
            catch (StorageException ex)
            {
                Console.WriteLine("DATABASE: {0}", ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine(ex.InnerException.Message);

                httpCode = HttpStatusCode.InternalServerError;
                httpDescription = "Failed";
            }

            ctx.OutgoingResponse.StatusCode = httpCode;
            ctx.OutgoingResponse.StatusDescription = httpDescription;

            return deletedGuids;
        }
    }
}
