using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMessages.Exceptions
{
    public enum StorageExceptionType : int
    {
        Unknown = 0,
        Read = 1,
        ReadResultMissing = 2,
        Write = 3,
        WriteResultMissing = 4,
        ParseResult = 5
    }

    [Serializable]
    public class StorageException : Exception
    {
        private StorageExceptionType _type;

        public StorageException()
        {
        }

        public StorageException(StorageExceptionType type)
        {
            _type = type;
        }

        public StorageException(string message)
            : base(message)
        {
        }

        public StorageException(string message, StorageExceptionType type)
            : base(message)
        {
            _type = type;
        }

        public StorageException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public StorageException(string message, StorageExceptionType type, Exception inner)
            : base(message, inner)
        {
            _type = type;
        }

        protected StorageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _type = (StorageExceptionType)info.GetByte("StorageExceptionType");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue("StorageExceptionType", _type);
            base.GetObjectData(info, context);
        }
    }
}
