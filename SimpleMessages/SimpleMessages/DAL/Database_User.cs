using SimpleMessages.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace SimpleMessages.DAL
{
    internal partial class Database
    {
        internal CreateUserResponseType CreateUser(string username, string base64hash, string base64secret, out Guid guid)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("AddUser", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Secret", base64secret);
                    cmd.Parameters.AddWithValue("@Hash", base64hash);

                    conn.Open();
                    try
                    {
                        var result = cmd.ExecuteScalar();

                        if (result == null)
                            throw new InvalidOperationException("Internal error");

                        guid = Guid.Parse(result.ToString());

                        return CreateUserResponseType.OK;
                    }
                    catch (SqlException ex)
                    {
                        guid = new Guid();
                        if (ex.Number == 2601)
                            return CreateUserResponseType.DuplicateUser;
                        else
                            return CreateUserResponseType.Error;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("DATABASE: Internal error while creating user {0}", username);
                        Console.WriteLine(ex.Message);

                        guid = new Guid();
                        return CreateUserResponseType.Error;
                    }
                }
            }
        }

        internal string GetHashForUsername(string username, out string base64secret)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("GetHashForUsername", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Username", username);

                    conn.Open();

                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        var base64hash = reader["Hash"].ToString();
                        base64secret = reader["Secret"].ToString();

                        return base64hash;
                    }
                    else
                    {
                        throw new InvalidOperationException("Internal error");
                    }
                }
            }
        }

        internal bool UsernameExists(string username)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("CheckIfUsernameExists", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Username", username);

                    conn.Open();
                    try
                    {
                        var result = cmd.ExecuteScalar();

                        if (result == null)
                            throw new InvalidOperationException("Internal error");

                        var exists = result.ToString().Equals("1");

                        return exists;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("DATABASE: Internal error while checking if user {0} exists", username);
                        Console.WriteLine(ex.Message);

                        return false; // TODO: very bad
                    }
                }
            }
        }
    }
}
