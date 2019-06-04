using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GeldApp2.Application.Exceptions
{
    /// <summary>
    /// Exception which's can safely be presented to the user.
    /// The exception will generate a 400-Bad-Request response.
    /// It should:
    /// - not contain security relevant information.
    /// - be helpful to the user.
    /// </summary>
    [Serializable]
    public class UserException : Exception
    {
        public UserException(string message)
          : base(message)
        {
        }

        public UserException(string message, Exception inner)
          : base(message, inner)
        {
        }

        protected UserException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}
