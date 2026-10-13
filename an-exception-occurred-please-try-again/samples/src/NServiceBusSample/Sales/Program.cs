using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using Sales.Data;

namespace Sales
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Title = "Sales";

            var builder = Host.CreateApplicationBuilder(args);

            var endpointConfiguration = new EndpointConfiguration("Sales");
            endpointConfiguration.UseSerialization<SystemJsonSerializer>();

            // The outbox requires the transport to run in ReceiveOnly mode
            endpointConfiguration.UseTransport(new LearningTransport
            {
                TransportTransactionMode = TransportTransactionMode.ReceiveOnly
            });

            var connectionString = builder.Configuration.GetConnectionString("Sales");

            // The outbox stores the outgoing messages in the same transaction as the handler's business data,
            // and deduplicates incoming messages
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlDialect<SqlDialect.MsSqlServer>();
            persistence.ConnectionBuilder(() => new SqlConnection(connectionString));
            endpointConfiguration.EnableOutbox();

            // Creates the outbox table when the endpoint starts
            endpointConfiguration.EnableInstallers();

            endpointConfiguration.SendFailedMessagesTo("error");

            // So that when we test recoverability, we don't have to wait so long
            // for the failed message to be sent to the error queue
            var recoverability = endpointConfiguration.Recoverability();
            recoverability.Immediate(immediateRetries => immediateRetries.NumberOfRetries(1));
            recoverability.Delayed(
                delayed =>
                {
                    delayed.NumberOfRetries(4);
                    delayed.TimeIncrease(TimeSpan.FromSeconds(2));
                }
            );
            recoverability.AddUnrecoverableException<NotImplementedException>();
            recoverability.CustomPolicy(JitterRecoverability.JitteredIncrementalBackOff);

            builder.Services.AddNServiceBusEndpoint(endpointConfiguration);

            // The data context shares the connection and transaction NServiceBus opened for the incoming message,
            // so the handler's changes are committed (or rolled back) together with the outgoing messages
            builder.Services.AddScoped(serviceProvider =>
            {
                var storageSession = serviceProvider.GetRequiredService<ISqlStorageSession>();

                var dataContext = new SalesDataContext(new DbContextOptionsBuilder<SalesDataContext>()
                    .UseSqlServer(storageSession.Connection)
                    .Options);
                dataContext.Database.UseTransaction(storageSession.Transaction);

                // Flush the tracked changes right before NServiceBus commits the transaction
                storageSession.OnSaveChanges((_, cancellationToken) => dataContext.SaveChangesAsync(cancellationToken));

                return dataContext;
            });

            // Creates the database and the Orders table on first run, before the endpoint's installers add the outbox table
            await using (var setupContext = new SalesDataContext(new DbContextOptionsBuilder<SalesDataContext>()
                             .UseSqlServer(connectionString)
                             .Options))
            {
                await setupContext.Database.EnsureCreatedAsync();
            }

            await builder.Build().RunAsync();
        }
    }
}
