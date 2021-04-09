using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContactDetailsApi.V1.Domain
{
    public class InvalidQueryParameterException : Exception
    {
        public InvalidQueryParameterException(string message)
            : base(message)
        {
        }
    }
}
