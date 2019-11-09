using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace GeldApp2.Application.Logging
{
    public class Events
    {
        /* App events 1xxx */

        public const int Startup = 1000;

        public const int HandleRequestFailed = 1002;

        public const int HandleRequestSuccess = 1003;

        public const int HandleRequestUserFailed = 1004;

        public const int IpBlocked = 1005;

        public const int PerformanceStatistics = 1008;

        public const int UsageStatistics = 1009;

        public const int StartService = 1010;

        public const int StopService = 1011;

        public const int LearnCategoriesForAccount = 1012;


        /* Domain commands 2xxx */

        public const int CategoryCommands = 2000;

        public const int ExpenseCommands = 2001;

        public const int UserCommands = 2002;

        public const int AdminCommands = 2003;

        public const int LoginCommand = 2004;

        public const int RefreshTokenCommand = 2005;
        
        public const int ImportCsvFile = 2006;
        
        public const int HandleImportedExpenseCommand = 2007;
    }
}
