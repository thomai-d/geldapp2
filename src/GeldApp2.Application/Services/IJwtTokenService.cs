using GeldApp2.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GeldApp2.Application.Services
{
    public interface IJwtTokenService
    {
        Task<string> GetTokenStringAsync(User user);
    }
}
