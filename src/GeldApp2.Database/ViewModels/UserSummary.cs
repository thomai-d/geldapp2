using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeldApp2.Database.ViewModels
{
    public class UserSummary
    {
        public static UserSummary FromUser(User user)
        {
            return new UserSummary
            {
                Name = user.Name,
                IsAdmin = user.IsAdmin,
                LastLogin = user.LastLogin,
                Accounts = user.UserAccounts.Select(ua => ua.Account.Name).ToArray()
            };
        }

        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty]
        public bool IsAdmin { get; private set; }

        [JsonProperty]
        public DateTimeOffset LastLogin { get; private set; }

        [JsonProperty]
        public IReadOnlyCollection<string> Accounts { get; private set; }

        public override string ToString() => $"{this.Name}{(this.IsAdmin ? " (Admin)" : string.Empty)}";
    }
}
