using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace GeldApp2.Application.Logging
{
    public class Events
    {
        public const int Startup = 1000;

        public const int HandleRequestFailed = 1002;

        public const int HandleRequestSuccess = 1003;

        public const int HandleRequestUserFailed = 1004;

        public const int IpBlocked = 1005;

        public const int PerformanceStatistics = 1008;

        public const int UsageStatistics = 1009;


        /* Domain commands 2xxx */

        public const int CategoryCommands = 2000;

        public const int ExpenseCommands = 2001;

        public const int UserCommands = 2002;

        public const int AdminCommands = 2003;

        public const int LoginCommand = 2004;

        public const int RefreshTokenCommand = 2005;
    }
}
