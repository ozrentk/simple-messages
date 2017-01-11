using System;
using System.Configuration;
using SimpleMessages.Models;

namespace SimpleMessages.DAL
{
    internal partial class Database
    {
        private string _connectionString { get; set; }

        internal Database()
        {
            _connectionString = ConfigurationManager.ConnectionStrings["MessagesDb"].ConnectionString;
        }
    }
}
