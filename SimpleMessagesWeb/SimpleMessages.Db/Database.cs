using SimpleMessages.DB.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace SimpleMessages.DB
{
    public partial class Database
    {
        public string ConnectionString { get; set; }

        public Database(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        public IEnumerable<T> ExecuteAndGetRows<T>(string procName, Dictionary<string, object> parameters, Func<SqlDataReader, T> mapper)
            where T : class
        {
            return ExecuteAndGetRowsInternal(procName, parameters, mapper);
        }

        public T ExecuteAndGetRow<T>(string procName, Dictionary<string, object> parameters, Func<SqlDataReader, T> mapper)
            where T : class
        {
            var rows = ExecuteAndGetRowsInternal(procName, parameters, mapper);

            return rows.SingleOrDefault();
        }

        public object ExecuteAndGetObject(string procName, Dictionary<string, object> parameters)
        {
            // TODO: implement using ExecuteScalar
            var value0 = ExecuteAndGetRowsInternal(procName, parameters, (reader) => reader[0]);

            return value0.SingleOrDefault();
        }

        public void Execute(string procName, Dictionary<string, object> parameters)
        {
            // TODO: implement using ExecuteNonQuery
            ExecuteAndGetRowsInternal(procName, parameters, (reader) => new object());
        }

        private IEnumerable<T> ExecuteAndGetRowsInternal<T>(string procName, Dictionary<string, object> parameters, Func<SqlDataReader, T> mapper)
            where T : class
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                var cmd = conn.CreateCommand();
                cmd.CommandText = procName;
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (var key in parameters.Keys)
                {
                    cmd.Parameters.AddWithValue(String.Format("@{0}", key), parameters[key] ?? DBNull.Value);
                }

                SqlDataReader reader = null;
                var objs = new List<T>();
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
                        var msg = String.Format("DATABASE: Internal error while executing reader on database procedure '{0}'", procName);
                        Console.WriteLine(msg);
                        Console.WriteLine(ex.Message);

                        throw new StorageException(msg, StorageExceptionType.Read, ex);
                    }

                    throw;
                }

                try
                {
                    while (reader.Read())
                    {
                        var obj = mapper(reader);
                        objs.Add(obj);
                    }
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentNullException ||
                        ex is FormatException)
                    {
                        var msg = String.Format("DATABASE: Internal error while mapping row output from database procedure '{0}'", procName);
                        Console.WriteLine(msg);
                        Console.WriteLine(ex.Message);

                        throw new StorageException(msg, StorageExceptionType.ParseResult, ex);
                    }

                    throw;
                }

                return objs;
            }
        }
    }
}
