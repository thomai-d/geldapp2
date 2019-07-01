using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace GeldApp2.Application.Logging
{
    public class Events
    {
        public const int Startup = 1000;

        public const int HandleRequest = 1001;

        public const int HandleRequestFailed = 1002;

        public const int HandleRequestSuccess = 1003;

        public const int HandleRequestUserFailed = 1004;

        public const int IpBlocked = 1005;

        public const int Login = 1006;

        public const int RefreshToken = 1007;

        public const int PerformanceStatistics = 1008;

        public const int UsageStatistics = 1009;
    }
}
