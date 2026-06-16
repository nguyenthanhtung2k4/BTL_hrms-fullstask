using System;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hrms.Shared.Messaging;

public static class MassTransitExtensions
{
    public static IServiceCollection AddSharedMassTransit(
        this IServiceCollection services, 
        IConfiguration configuration, 
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        services.AddMassTransit(x =>
        {
            // Register consumers if passed
            configureConsumers?.Invoke(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                var host = configuration["RabbitMQ:Host"] ?? "localhost";
                var username = configuration["RabbitMQ:Username"] ?? "guest";
                var password = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(host, "/", h =>
                {
                    h.Username(username);
                    h.Password(password);
                });

                // Configure standard message retry policy
                cfg.UseMessageRetry(retry => retry.Interval(3, TimeSpan.FromSeconds(5)));

                // Configure endpoints automatically for registered consumers
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
