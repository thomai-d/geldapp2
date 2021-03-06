﻿using GeldApp2.Application.Exceptions;
using GeldApp2.Application.Logging;
using GeldApp2.Database;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GeldApp2.Application.Commands.Users
{
    public class CreateUserCommand : IRequest<bool>, ILoggable, ICommand
    {
        private CreateUserCommand()
        {
        }

        public CreateUserCommand(string name, string password, bool createDefaultAccount)
        {
            this.Name = name;
            this.Password = password;
            this.CreateDefaultAccount = createDefaultAccount;
        }

        public string Name { get; set; }

        public string Password { get; set; }

        public bool CreateDefaultAccount { get; set; }

        public void EmitLog(LogEventDelegate log, bool success)
        {
            if (success)
                log(Events.AdminCommands, "Admin created the user {Username}", this.Name);
            else
                log(Events.AdminCommands, "Creating the user {Username} failed", this.Name);
        }
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, bool>
    {
        public const int MinPasswordLength = 5;

        private readonly GeldAppContext db;

        public CreateUserCommandHandler(GeldAppContext db)
        {
            this.db = db;
        }

        public async Task<bool> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.Name))
                throw new UserException("Username empty");
            if (string.IsNullOrEmpty(request.Password) || request.Password.Length < MinPasswordLength)
                throw new UserException("Password does not satisfy server requirements");

            var userExists = this.db.Users.Any(u => u.Name == request.Name);
            if (userExists)
                throw new UserException("Username is already in use");

            var users = this.db.Users.ToList();

            var user = new User(request.Name, request.Password);
            user.GenerateRefreshToken();

            if (request.CreateDefaultAccount)
                user.UserAccounts.Add(new UserAccount(user, new Account(request.Name)));

            this.db.Users.Add(user);
            await this.db.SaveChangesAsync();
            return true;
        }
    }
}
