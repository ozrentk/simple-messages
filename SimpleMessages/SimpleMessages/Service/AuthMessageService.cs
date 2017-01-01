using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleMessages.Models;
using System.ServiceModel.Web;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Net;
using SimpleMessages.Helpers;
using SimpleMessages.DAL;

namespace SimpleMessages.Service
{
    public class AuthMessageService : IAuthMessages
    {
        private readonly Database _database;

        public AuthMessageService()
        {
            _database = new Database();
        }


        /*public void Send(MessageContainer container)
        {
            throw new NotImplementedException();
        }

        public MessageContainer Receive()
        {
            throw new NotImplementedException();
        }

        public Message[] FindMessages(string from, string to, string searchText)
        {
            throw new NotImplementedException();
        }

        public User[] FindUsers(string username)
        {
            throw new NotImplementedException();
        }

        public Message[] RemoveMessage(string id)
        {
            throw new NotImplementedException();
        }

        public Message[] RemoveMessageHistory(string from, string to, string timeFrom, string timeTo)
        {
            throw new NotImplementedException();
        }

        public Message[] UpdateMessage(string id, string content)
        {
            throw new NotImplementedException();
        }*/
    }
}
