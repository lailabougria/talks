using System;
using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;
using ClientUI.Data;
using Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NServiceBus.TransactionalSession;
using Polly;

namespace ClientUI.Controllers
{
    [Route("/")]
    public class HomeController : Controller
    {
        static int messagesSent;
        private readonly ILogger<HomeController> _log;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ResiliencePipeline _sendPipeline;

        public HomeController(
            IServiceScopeFactory scopeFactory,
            // AddResiliencePipeline also registers every pipeline as a keyed service, using its name as the key
            [FromKeyedServices(Program.SendPipeline)] ResiliencePipeline sendPipeline,
            ILogger<HomeController> logger)
        {
            _scopeFactory = scopeFactory;
            _sendPipeline = sendPipeline;
            _log = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> PlaceOrder()
        {
            string orderId = Guid.NewGuid().ToString().Substring(0, 8);

            // Retry the whole unit of work: open, send and commit.
            await _sendPipeline.ExecuteAsync(async ct =>
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var session = scope.ServiceProvider.GetRequiredService<ITransactionalSession>();

                    // Opening the session starts a SQL transaction on the storage session
                    await session.Open(new SqlPersistenceOpenSessionOptions(), ct).ConfigureAwait(false);

                    // The data context shares that connection and transaction, so resolve it after opening the session
                    var dataContext = scope.ServiceProvider.GetRequiredService<OrdersDataContext>();
                    dataContext.Orders.Add(new Order { OrderId = orderId, PlacedAt = DateTimeOffset.UtcNow });

                    // Messages aren't dispatched yet: they're stored in the outbox as part of the same transaction
                    await session.Send(new PlaceOrder { OrderId = orderId }, ct).ConfigureAwait(false);
                    await session.SendLocal(new SendOrderConfirmation { OrderId = orderId }, ct).ConfigureAwait(false);

                    // Saves the order and the outgoing messages atomically, then dispatches the messages
                    await session.Commit(ct).ConfigureAwait(false);
                }, HttpContext.RequestAborted)
                .ConfigureAwait(false);

            _log.LogInformation($"Sending PlaceOrder, OrderId = {orderId}");

            dynamic model = new ExpandoObject();
            model.OrderId = orderId;
            model.MessagesSent = Interlocked.Increment(ref messagesSent);

            return View(model);
        }
    }
}
