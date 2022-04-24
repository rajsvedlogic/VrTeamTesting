using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace VrTeamTesting.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string message, Exception innerException = null) : base(HttpStatusCode.Unauthorized, message, innerException)
        {
        }
    }
}
