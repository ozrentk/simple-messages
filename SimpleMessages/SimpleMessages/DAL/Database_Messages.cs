using SimpleMessages.Exceptions;
using SimpleMessages.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace SimpleMessages.DAL
{
    internal partial class Database
    {
        internal Message[] GetMessages(string to, MessageStatusType lastStatus, bool showDeleted = false)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "GetMessages";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ToName", to);
                cmd.Parameters.AddWithValue("@LastStatusId", (int)lastStatus);
                cmd.Parameters.AddWithValue("@ShowDeleted", showDeleted);

                SqlDataReader reader = null;
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while reading messages from database for user {0}", to);
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't read messages from the database", StorageExceptionType.Read, ex);
                    }

                    throw;
                }

                try
                {
                    var messages = new List<Message>();
                    while (reader.Read())
                    {
                        messages.Add(new Message
                        {
                            Guid = Guid.Parse(reader["Guid"].ToString()),
                            CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                            From = reader["SenderName"].ToString(),
                            To = reader["ReceiverName"].ToString(),
                            Content = reader["Content"].ToString(),
                            Status = (MessageStatusType)int.Parse(reader["StatusId"].ToString()),
                            IsDeleted = reader["IsDeleted"].ToString().Equals("1")
                        });
                    }
                    return messages.ToArray();
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException ||
                        ex is FormatException ||
                        ex is OverflowException ||
                        ex is InvalidCastException)
                    {
                        Console.WriteLine("DATABASE: Internal error while parsing message details from database for user {0}", to);
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't parse the database message data", StorageExceptionType.ParseResult, ex);
                    }

                    throw;
                }
            }
        }

        internal Notification[] GetNotifications(string to)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "GetNotifications";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@ToName", to);

                SqlDataReader reader = null;
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while reading notifications from database for user {0}", to);
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't read notifications from the database", StorageExceptionType.Read, ex);
                    }

                    throw;
                }

                try
                {
                    var notifications = new List<Notification>();
                    while (reader.Read())
                    {
                        notifications.Add(new Notification
                        {
                            Guid = Guid.Parse(reader["Guid"].ToString()),
                            CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                            NotificationType = (NotificationType)int.Parse(reader["NotificationTypeId"].ToString()),
                            OriginUser = reader["OriginName"].ToString(),
                            MessageGuid = Guid.Parse(reader["MessageGuid"].ToString()),
                            MessageStatus = (MessageStatusType)int.Parse(reader["MessageStatusId"].ToString())
                        });
                    }
                    return notifications.ToArray();
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException ||
                        ex is FormatException ||
                        ex is OverflowException ||
                        ex is InvalidCastException)
                    {
                        Console.WriteLine("DATABASE: Internal error while parsing notifications details from database for user {0}", to);
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't parse the database notification data", StorageExceptionType.ParseResult, ex);
                    }

                    throw;
                }
            }
        }

        internal Guid CreateMessage(Message message)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "AddMessage";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FromName", message.From);
                cmd.Parameters.AddWithValue("@ToName", message.To);
                cmd.Parameters.AddWithValue("@Content", message.Content);

                object result = null;
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    result = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while writing the message {0} to database", message);
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't write message to the database", StorageExceptionType.Write, ex);
                    }

                    throw;
                }

                if (result == null)
                {
                    Console.WriteLine("DATABASE: Internal error while reading GUID result from database after message write operation");
                    throw new StorageException("Missing GUID result after database write", StorageExceptionType.WriteResultMissing);
                }

                try
                {
                    var newGuid = Guid.Parse(result.ToString());
                    return newGuid;
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentNullException ||
                        ex is FormatException)
                    {
                        Console.WriteLine("DATABASE: Internal error while parsing GUID from database");
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't parse GUID retrieved from database", StorageExceptionType.ParseResult, ex);
                    }

                    throw;
                }
            }
        }

        internal bool DeleteMessage(Guid guid)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DeleteSingleMessage";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Guid", guid);

                object result = null;
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    result = cmd.ExecuteScalar();

                    if (result != null)
                        return true; // message deleted, notification created
                    else
                        return false; // no such message (message not deleted, notification not created)
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while deleting message {0}", guid);
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't delete message in the database", StorageExceptionType.Write, ex);
                    }

                    throw;
                }
            }
        }

        internal Guid[] DeleteMessages(string from, string to, DateTime? timeFrom, DateTime? timeTo)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DeleteMultipleMessages";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FromName", from);
                cmd.Parameters.AddWithValue("@ToName", to);
                cmd.Parameters.AddWithValue("@TimeFrom", timeFrom);
                cmd.Parameters.AddWithValue("@TimeTo", timeTo);

                SqlDataReader reader = null;
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while deleting messages");
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't delete message in the database", StorageExceptionType.Write, ex);
                    }

                    throw;
                }

                try
                {
                    var messageGuidList = new List<Guid>();
                    while (reader.Read())
                    {
                        messageGuidList.Add(Guid.Parse(reader["Guid"].ToString()));
                    }
                    return messageGuidList.ToArray();
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException ||
                        ex is FormatException ||
                        ex is OverflowException ||
                        ex is InvalidCastException)
                    {
                        Console.WriteLine("DATABASE: Internal error while parsing deleted message guids from database");
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't parse the guid of message deleted from database", StorageExceptionType.ParseResult, ex);
                    }

                    throw;
                }
            }
        }

        internal bool SetMessageStatus(Guid guid, MessageStatusType newStatus)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SetSingleMessageStatus";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Guid", guid);
                cmd.Parameters.AddWithValue("@StatusId", (int)newStatus);

                object result = null;
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    result = cmd.ExecuteScalar();

                    if (result != null)
                        return true; // status updated, notification created
                    else
                        return false; // no such message (status not updated, notification not created)
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while setting message {0} status to {1}", guid, newStatus);
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't set message status in the database", StorageExceptionType.Write, ex);
                    }

                    throw;
                }
            }
        }

        internal Guid[] SetMessageStatus(string fromName, string toName, MessageStatusType newStatus)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "SetMultipleMessageStatus";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FromName", fromName);
                cmd.Parameters.AddWithValue("@ToName", toName);
                cmd.Parameters.AddWithValue("@StatusId", (int)newStatus);

                SqlDataReader reader = null;
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while setting {0} status of all messages sent from {1} to {2}", newStatus, fromName, toName);
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't set all message status in the database", StorageExceptionType.Write, ex);
                    }

                    throw;
                }

                try
                {
                    var messageGuids = new List<Guid>();
                    while (reader.Read())
                    {
                        messageGuids.Add(Guid.Parse(reader["Guid"].ToString()));
                    }
                    return messageGuids.ToArray();
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException ||
                        ex is FormatException ||
                        ex is OverflowException ||
                        ex is InvalidCastException)
                    {
                        Console.WriteLine("DATABASE: Internal error while parsing message guids");
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't parse the message guids from database", StorageExceptionType.ParseResult, ex);
                    }

                    throw;
                }
            }
        }

        internal bool UpdateMessageContent(Guid guid, string content)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UpdateMessageContent";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Guid", guid);
                cmd.Parameters.AddWithValue("@Content", content);

                object result = null;
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    result = cmd.ExecuteScalar();

                    if (result != null)
                        return true; // content updated, notification created
                    else
                        return false; // no such message (status not updated, notification not created)
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while updating message {0} content");
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't update message content in the database", StorageExceptionType.Write, ex);
                    }

                    throw;
                }
            }
        }

        internal Message[] FindMessages(string from, string to, DateTime? timeFrom, DateTime? timeTo, string searchText, bool includeDeleted)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "FindMessages";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@FromName", from);
                cmd.Parameters.AddWithValue("@ToName", to);
                cmd.Parameters.AddWithValue("@TimeFrom", timeFrom);
                cmd.Parameters.AddWithValue("@TimeTo", timeTo);
                cmd.Parameters.AddWithValue("@SearchText", searchText);
                cmd.Parameters.AddWithValue("@IncludeDeleted", includeDeleted);

                SqlDataReader reader = null;
                var messages = new List<Message>();
                try
                {
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    while (reader.Read())
                    {
                        messages.Add(new Message
                        {
                            Guid = Guid.Parse(reader["Guid"].ToString()),
                            CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                            From = reader["SenderName"].ToString(),
                            To = reader["ReceiverName"].ToString(),
                            Content = reader["Content"].ToString(),
                            Status = (MessageStatusType)reader["StatusId"],
                            IsDeleted = reader["IsDeleted"].ToString().Equals("1")
                        });
                    }
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while searching for users in database");
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't read user from the database", StorageExceptionType.Read, ex);
                    }

                    throw;
                }

                return messages.ToArray();
            }
        }
    }
}
