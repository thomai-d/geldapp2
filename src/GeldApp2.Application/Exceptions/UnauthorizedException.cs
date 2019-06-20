using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GeldApp2.Application.Exceptions
{
    /// <summary>
    /// Exception that is raised if the user is authenticated
    /// and requested an action for which he is unauthorized.
    /// </summary>

    [Serializable]
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message)
          : base(message)
        {
        }

        public UnauthorizedException(string message, Exception inner)
          : base(message, inner)
        {
        }

        protected UnauthorizedException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}
