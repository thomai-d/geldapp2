using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GeldApp2.Application.Exceptions
{
    [Serializable]
    public class FilterParseException : UserException
    {
        public FilterParseException(string message)
          : base(message)
        {
        }

        protected FilterParseException(SerializationInfo info, StreamingContext context)
          : base(info, context)
        {
        }
    }
}
