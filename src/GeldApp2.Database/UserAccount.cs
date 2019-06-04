using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database
{
    public class UserAccount
    {
        protected UserAccount()
        {
        }

        public UserAccount(long userId, long accountId)
        {
            this.UserId = userId;
            this.AccountId = accountId;
        }

        public UserAccount(User user, Account account)
        {
            this.User = user;
            this.Account = account;
        }

        public long UserId { get; set; }

        public User User { get; set; }

        public long AccountId { get; set; }

        public Account Account { get; set; }
    }
}
