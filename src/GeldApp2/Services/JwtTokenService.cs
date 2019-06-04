using GeldApp2.Application.Services;
using GeldApp2.Configuration;
using GeldApp2.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GeldApp2.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly AuthenticationSettings authSettings;
        private readonly GeldAppContext db;

        public JwtTokenService(
            GeldAppContext db,
            IOptions<AuthenticationSettings> authSettingOptions)
        {
            this.authSettings = authSettingOptions.Value;
            this.db = db;
        }

        public async Task<string> GetTokenStringAsync(User user)
        {
            var accounts = await this.db.Accounts
                .AsNoTracking()
                .Where(a => a.UserAccounts.Any(ua => ua.UserId == user.Id))
                .ToListAsync();

            var accountNames = accounts.Select(a => a.Name).ToArray();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.authSettings.JwtTokenSecret);
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim("userid", user.Id.ToString()),
                new Claim("username", user.Name)
            };

            claims.AddRange(accountNames.Select(n => new Claim("accounts", n)));

            var tokenOptions = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.Add(this.authSettings.TokenExpiry),
                signingCredentials: signingCredentials
                );

            return tokenHandler.WriteToken(tokenOptions);
        }
    }
}
