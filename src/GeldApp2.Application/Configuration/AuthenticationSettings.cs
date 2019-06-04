using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Configuration
{
    public class AuthenticationSettings
    {
        public string JwtTokenSecret { get; set; }

        public TimeSpan TokenExpiry { get; set; }
    }
}
