using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Abstrakt.AspNetCore
{
    public static class Runtime
    {
        public static bool IsIntegrationTesting = Environment.GetEnvironmentVariable("INTEGRATION_TEST") == "1";
    }
}
