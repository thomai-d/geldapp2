using FluentAssertions;
using GeldApp2.Application.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace GeldApp2.Application.UnitTests
{
    public class Guidelines
    {
        [Fact]
        public void AllCommandsShouldImplementICommand()
        {
            typeof(AccountRelatedRequest<>)
                    .Assembly.GetTypes()
                    .Where(t => t.Name.EndsWith("Command")
                             && !typeof(ICommand).IsAssignableFrom(t))
                    .Should().BeEmpty();
        }
        
        [Fact]
        public void AllCommandsAndQueriesShouldImplementLogging()
        {
            typeof(AccountRelatedRequest<>)
                    .Assembly.GetTypes()
                    .Where(t => t.Name.EndsWith("Command")
                             && !t.IsInterface
                             && !typeof(ILoggable).IsAssignableFrom(t))
                    .Should().BeEmpty();
        }
    }
}
