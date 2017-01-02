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
        internal Guid CreateUser(string username, string base64hash, string base64secret)
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

        internal string GetHashForUsername(string username, out string base64secret)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
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
                    conn.ConnectionString = _connectionString;
                    conn.Open();

                    reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);

                    if (!reader.HasRows)
                    {
                        Console.WriteLine("DATABASE: Internal error while reading hash from database after message write operation");
                        throw new StorageException("Missing hash result for user", StorageExceptionType.WriteResultMissing);
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

        internal bool CheckIfUsernameExists(string username, bool isExisting)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "CheckIfUsernameExists";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@IsExisting", isExisting);

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

        internal string[] FindUsers(string token)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = "FindUsers";
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@token", token);

                SqlDataReader reader = null;
                var users = new List<string>();
                try
                {
                    conn.ConnectionString = _connectionString;
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


    }
}
