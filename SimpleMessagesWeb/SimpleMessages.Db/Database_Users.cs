using SimpleMessages.DB.Models;
using SimpleMessages.DB.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace SimpleMessages.DB
{
    // TODO: these methods are obsolete!!! Rewrite callers to use generic procedure calls!!!
    public partial class Database
    {
        public Guid CreateUser(string username, string base64hash, string base64secret)
        {
            using (SqlConnection conn = new SqlConnection())
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "AddUser";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@Secret", base64secret);
                cmd.Parameters.AddWithValue("@Hash", base64hash);

                object result = null;
                try
                {
                    conn.ConnectionString = ConnectionString;
                    conn.Open();

                    result = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        var msg = String.Format("Couldn't create user {0} in the database", username);
                        throw new StorageException(msg, StorageExceptionType.Write, ex);
                    }

                    throw;
                }

                if (result == null)
                {
                    Console.WriteLine("DATABASE: Internal error while reading GUID result from database after create user operation");
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

        public string GetHashForUsername(string username, out string base64secret)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "GetHashForUsername";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Username", username);

                SqlDataReader reader = null;
                string base64hash = null;
                base64secret = null;
                try
                {
                    conn.ConnectionString = ConnectionString;
                    conn.Open();

                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    if (!reader.HasRows)
                    {
                        Console.WriteLine("DATABASE: Internal error while reading hash from database after message write operation");
                        throw new StorageException("Missing hash result for user", StorageExceptionType.ReadResultMissing);
                    }

                    while (reader.Read())
                    {
                        base64hash = reader["Hash"].ToString();
                        base64secret = reader["Secret"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while reading hash for user from database {0}", username);
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't read hash from the database", StorageExceptionType.Read, ex);
                    }

                    throw;
                }

                return base64hash;
            }
        }

        public bool CheckIfUsernameExists(string username, bool isExisting)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "CheckIfUsernameExists";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@IsExisting", isExisting);

                object result = null;
                try
                {
                    conn.ConnectionString = ConnectionString;
                    conn.Open();

                    result = cmd.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while checking user {0} existence", username);
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't check user existence in the database", StorageExceptionType.Read, ex);
                    }

                    throw;
                }

                if (result == null)
                {
                    Console.WriteLine("DATABASE: Internal error while checking user {0} existence");
                    throw new StorageException("Missing check user existence result", StorageExceptionType.ReadResultMissing);
                }

                return result.ToString().Equals("True");
            }
        }

        public string[] FindUsers(string token)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "FindUsers";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@token", token);

                SqlDataReader reader = null;
                var users = new List<string>();
                try
                {
                    conn.ConnectionString = ConnectionString;
                    conn.Open();

                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    while (reader.Read())
                    {
                        users.Add(reader["Name"].ToString());
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

                return users.ToArray();
            }
        }

        public User GetUser(Guid? guid = null, string userName = null)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "GetUser";
                cmd.CommandType = CommandType.StoredProcedure;

                if(guid.HasValue)
                    cmd.Parameters.AddWithValue("@guid", guid);
                else if (!String.IsNullOrEmpty(userName))
                    cmd.Parameters.AddWithValue("@userName", userName);

                SqlDataReader reader = null;
                User user = null;
                try
                {
                    conn.ConnectionString = ConnectionString;
                    conn.Open();

                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                }
                catch (Exception ex)
                {
                    if (ex is SqlException ||
                        ex is IOException ||
                        ex is InvalidOperationException)
                    {
                        Console.WriteLine("DATABASE: Internal error while retrieving user '{0}' from database", userName);
                        Console.WriteLine(ex.Message);

                        throw new StorageException("Couldn't read user from the database", StorageExceptionType.Read, ex);
                    }

                    throw;
                }

                try
                {
                    if (reader.Read())
                    {
                        user = new User
                        {
                            Guid = Guid.Parse(reader["Guid"].ToString()),
                            UserName = reader["Name"].ToString(),
                            PasswordHash = reader["Hash"].ToString(),
                            Secret = reader["Secret"].ToString()
                        };
                    }
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

                return user;
            }
        }
    }
}
