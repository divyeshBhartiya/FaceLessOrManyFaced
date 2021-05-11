using GreenPipes;
using MassTransit;
using Messaging.InterfacesConstants.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Orders.API.Hubs;
using Orders.API.Messages.Consumers;
using Orders.API.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orders.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // SignalR Setup
            services.AddHttpClient();
            services.AddSignalR()
                .AddJsonProtocol(opts =>
                {
                    opts.PayloadSerializerOptions.PropertyNamingPolicy = null;
                });

            // Faces.API Config
            services.Configure<OrderSettings>(Configuration);

            // SQL Server - EF Core and Repository Setup
            services.AddTransient<IOrderRepository, OrderRepository>();
            services.AddDbContext<OrdersContext>(options => options.UseSqlServer
            (
                Configuration.GetConnectionString("OrdersConnection")
            ));

            // RabbitMQ - MassTransit Setup
            services.AddMassTransit(x =>
            {
                x.AddConsumer<RegisterOrderCommandConsumer>();
                x.AddConsumer<OrderDispatchedEventConsumer>();
                x.AddBus(provider => Bus.Factory.CreateUsingRabbitMq(cfg =>
                {
                    //cfg.UseHealthCheck(provider);
                    cfg.Host(new Uri(RabbitMqMassTransitConstants.RabbitMqUri), h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });
                    cfg.ReceiveEndpoint(RabbitMqMassTransitConstants.RegisterOrderCommandQueue, ep =>
                    {
                        ep.PrefetchCount = 16;
                        ep.UseMessageRetry(r => r.Interval(2, 100));
                        ep.ConfigureConsumer<RegisterOrderCommandConsumer>(provider);
                    });
                    cfg.ReceiveEndpoint(RabbitMqMassTransitConstants.OrderDispatchedServiceQueue, ep =>
                    {
                        ep.PrefetchCount = 16;
                        ep.UseMessageRetry(r => r.Interval(2, 100));
                        ep.ConfigureConsumer<OrderDispatchedEventConsumer>(provider);
                    });
                }));
            });
            services.AddMassTransitHostedService();

            // CORS Policy Setup
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetIsOriginAllowed((host) => true)
                       .AllowCredentials());
            });
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Orders.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders.API v1"));
            }

            app.UseCors("CorsPolicy");
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<OrderHub>("/orderhub");
            });

            using var scope = app.ApplicationServices.
               GetRequiredService<IServiceScopeFactory>().
               CreateScope();
            scope.ServiceProvider.GetService<OrdersContext>().MigrateDB();
        }
    }
}
