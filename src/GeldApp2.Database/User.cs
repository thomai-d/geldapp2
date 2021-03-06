﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GeldApp2.Database
{
    public class User
    {
        public static User CreateAdmin(string user, string pass)
        {
            return new User(user, pass) { IsAdmin = true };
        }

        public User()
        {
        }

        public User(string user, string pass)
        {
            this.Name = user;
            this.SetPassword(pass);
        }

        public long Id { get; set; }

        public string Name { get; set; }

        public string Password { get; private set; }

        public string RefreshToken { get; set; }

        public DateTimeOffset LastLogin { get; set; }

        public DateTimeOffset LastPasswordChange { get; set; }

        public ICollection<UserAccount> UserAccounts { get; set; } = new List<UserAccount>();

        public bool IsAdmin { get; private set; }

        public void SetPassword(string password)
        {
            this.Password = this.Hash(password);
        }

        public Account GetAccount(string accountName)
        {
            var userAccount = this.UserAccounts
                        .SingleOrDefault(ua => ua.Account.Name == accountName);

            if (userAccount == null)
                throw new AuthenticationException($"Unknown account");

            return userAccount.Account;
        }

        public bool HasPassword(string password)
        {
            var hash = this.Hash(password);
            return this.Password.Equals(hash);
        }

        public override string ToString() => $"User '{this.Name}', id: {this.Id}";

        public void AddAccount(Account account)
        {
            this.UserAccounts.Add(new UserAccount(this, account));
        }

        public void GenerateRefreshToken()
        {
            using (var provider = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[16];
                provider.GetBytes(bytes);
                this.RefreshToken = new Guid(bytes).ToString();
            }
        }

        private string Hash(string input)
        {
            using (var csp = SHA256.Create())
            {
                var hash = csp.ComputeHash(Encoding.UTF8.GetBytes("salty" + input));
                return Convert.ToBase64String(hash);
            }
        }
    }
}
