﻿using Kledex.Commands;
using Microsoft.Extensions.Configuration;
using MultiTenant.SaaS.DatabaseTenancy.Pattern.Sample.Domain.Commands;
using MultiTenant.SaaS.DatabaseTenancy.Pattern.Sample.Infrastructure.DBContexts;
using MultiTenant.SaaS.DatabaseTenancy.Pattern.Sample.Infrastructure.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiTenant.SaaS.DatabaseTenancy.Pattern.Sample.Domain.CommandHandlers
{
    public class RegisterUserCommandHandler : ICommandHandlerAsync<RegisterUserCommand>
    {
        private readonly UserManagementDbContext userManagementDbContext;
        private readonly SharedDbContext sharedDbContext;
        private readonly IConfiguration configuration;
        public RegisterUserCommandHandler(UserManagementDbContext userManagementDbContext, SharedDbContext sharedDbContext, IConfiguration configuration)
        {
            this.userManagementDbContext = userManagementDbContext;
            this.sharedDbContext = sharedDbContext;
            this.configuration = configuration;
        }

        public async Task<CommandResponse> HandleAsync(RegisterUserCommand command)
        {
            await this.userManagementDbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
            CommandResponse commandResponse = new CommandResponse();

            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = command.Email,
                FirstName = command.FirstName,
                LastName = command.LastName,
                CreationDateTime = DateTime.UtcNow,
                IsArchived = false,
                IsPaidUser = false,
                LastUpdateDateTime = DateTime.UtcNow,
            };
            user.Contents = RegisterUserCommandHandler.GetDummyContents(user);


            UserAndDatabaseNameMapping userDatabaseMapping = new UserAndDatabaseNameMapping
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TenantId = user.TenantId,
                Email = user.Email,
                DatabaseName = this.configuration.GetValue<string>("Cosmos:SharedDatabaseName"), // unpaid user goes to shared database
                IsArchived = false,
                CreationDateTime = DateTime.UtcNow,
                LastUpdateDateTime = DateTime.UtcNow,
            };

            await this.userManagementDbContext.UserDatabaseMappings.AddAsync(userDatabaseMapping).ConfigureAwait(false);
            await this.userManagementDbContext.SaveChangesAsync().ConfigureAwait(false);


            await this.sharedDbContext.Users.AddAsync(user).ConfigureAwait(false);
            await this.sharedDbContext.SaveChangesAsync().ConfigureAwait(false);

            commandResponse.Result = new { user.Id, user.Email, user.FirstName, user.LastName, Contents = user.Contents.ToArray() };
            return commandResponse;
        }

        private static List<DummyContent> GetDummyContents(User user)
        {
            List<DummyContent> contents = new List<DummyContent>
            {
                new DummyContent()
                {
                    CreationDateTime = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    User = user,
                    UserId = user.Id,
                    Item1 = "item-1",
                    Item2 = "item-2",
                    Item3 = "item-3",
                    IsArchived = false,
                    LastUpdateDateTime = DateTime.UtcNow,
                },
                new DummyContent()
                {
                    CreationDateTime = DateTime.UtcNow,
                    Id = Guid.NewGuid(),
                    User = user,
                    UserId = user.Id,
                    Item1 = "item-1",
                    Item2 = "item-2",
                    Item3 = "item-3",
                    IsArchived = false,
                    LastUpdateDateTime = DateTime.UtcNow,
                },
            };

            return contents;
        }
    }
}
