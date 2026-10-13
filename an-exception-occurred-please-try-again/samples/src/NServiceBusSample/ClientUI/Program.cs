using System;
using ClientUI.Data;
using Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using NServiceBus.TransactionalSession;
using Polly;
using Polly.Retry;

namespace ClientUI
{
    public class Program
    {
        public const string SendPipeline = "send";

        public static void Main(string[] args)
        {
            Console.Title = "ClientUI";

            var builder = WebApplication.CreateBuilder(args);

            var endpointConfiguration = new EndpointConfiguration("ClientUI");
            endpointConfiguration.UseSerialization<SystemJsonSerializer>();

            // The outbox requires the transport to run in ReceiveOnly mode
            var routing = endpointConfiguration.UseTransport(new LearningTransport
            {
                TransportTransactionMode = TransportTransactionMode.ReceiveOnly
            });
            routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");

            endpointConfiguration.SendFailedMessagesTo("error");

            var connectionString = builder.Configuration.GetConnectionString("Orders");

            // The transactional session uses the outbox to make sure messages are only sent
            // when the session is committed, together with any business data stored in it
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(() => new SqlConnection(connectionString));
            persistence.EnableTransactionalSession();
            endpointConfiguration.EnableOutbox();

            // Creates the outbox table when the endpoint starts
            endpointConfiguration.EnableInstallers();

            builder.Services.AddNServiceBusEndpoint(endpointConfiguration);
            builder.Services.AddControllersWithViews();

            // The data context shares the connection and transaction of the session's synchronized storage session,
            // so the order is stored atomically with the outgoing messages. Resolve it only after the session is opened.
            builder.Services.AddScoped(serviceProvider =>
            {
                var storageSession = serviceProvider.GetRequiredService<ISqlStorageSession>();

                var dataContext = new OrdersDataContext(new DbContextOptionsBuilder<OrdersDataContext>()
                    .UseSqlServer(storageSession.Connection)
                    .Options);
                dataContext.Database.UseTransaction(storageSession.Transaction);

                // Flush the tracked changes right before the session commits the transaction
                storageSession.OnSaveChanges((_, cancellationToken) => dataContext.SaveChangesAsync(cancellationToken));

                return dataContext;
            });

            // Sending a message from outside a message handler isn't covered by NServiceBus recoverability,
            // so we protect it with a retry pipeline ourselves.
            builder.Services.AddResiliencePipeline(SendPipeline, pipeline => pipeline
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 3,
                    Delay = TimeSpan.FromMilliseconds(200),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                }));

            // Creates the database and the Orders table on first run, before the endpoint's installers add the outbox table
            using (var setupContext = new OrdersDataContext(new DbContextOptionsBuilder<OrdersDataContext>()
                       .UseSqlServer(connectionString)
                       .Options))
            {
                setupContext.Database.EnsureCreated();
            }

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
