using GeldApp2.Database;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands
{
    public abstract class AccountRelatedRequest<T> : IRequest<T>
    {
        public AccountRelatedRequest()
        {
        }

        public AccountRelatedRequest(string accountName)
        {
            this.AccountName = accountName;
        }

        public string AccountName { get; set; }
        
        public Account Account { get; set; }
    }
}
