using ClientUI;
using Microsoft.Extensions.Http.Resilience;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient<SalesClient>(client =>
        client.BaseAddress = new Uri(builder.Configuration["SalesUrl"]!))
    // Combines, from outer to inner: a rate limiter, a total request timeout, retries,
    // a circuit breaker and a timeout per attempt. Retries handle transient HTTP failures:
    // HttpRequestException, 408, 429 and 5xx responses.
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.Delay = TimeSpan.FromMilliseconds(500);

        // Sales retries the database itself, which can take up to ~20 seconds in the
        // exponential back-off scenario. The default attempt timeout of 10 seconds would
        // cancel that request and send a new one, starting a whole new round of retries in Sales.
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
        options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(90);

        // Must be at least twice the attempt timeout
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(60);
    });

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
