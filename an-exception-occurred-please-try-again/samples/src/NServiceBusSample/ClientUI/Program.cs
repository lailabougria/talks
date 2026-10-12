using System;
using Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;

namespace ClientUI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "ClientUI";

            var builder = WebApplication.CreateBuilder(args);

            var endpointConfiguration = new EndpointConfiguration("ClientUI");
            endpointConfiguration.UseSerialization<SystemJsonSerializer>();

            var routing = endpointConfiguration.UseTransport(new LearningTransport());
            routing.RouteToEndpoint(typeof(PlaceOrder), "Sales");

            endpointConfiguration.SendFailedMessagesTo("error");

            builder.Services.AddNServiceBusEndpoint(endpointConfiguration);
            builder.Services.AddControllersWithViews();

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
