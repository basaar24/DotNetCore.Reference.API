using DotNetCore.API.Common.Exceptions;
using System;
using System.Runtime.Serialization;

namespace DotNetCore.Reference.API.Dapper
{
    [Serializable]
    public class DotNetCoreRefDapperException : DotNetCoreAPICommonException
    {
        public DotNetCoreRefDapperException()
        {
        }

        public DotNetCoreRefDapperException(string message) : base(message)
        {
        }

        public DotNetCoreRefDapperException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DotNetCoreRefDapperException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
