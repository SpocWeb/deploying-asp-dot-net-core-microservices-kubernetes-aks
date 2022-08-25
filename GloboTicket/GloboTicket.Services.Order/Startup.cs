using AutoMapper;
using GloboTicket.Integration.MessagingBus;
using GloboTicket.Services.Ordering.DbContexts;
using GloboTicket.Services.Ordering.Extensions;
using GloboTicket.Services.Ordering.Messaging;
using GloboTicket.Services.Ordering.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using org.SpocWeb.Micro.Health;

namespace GloboTicket.Services.Ordering
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var loggerFactory = LoggerFactory.Create(builder => { /*configure*/ });
            logger = loggerFactory.CreateLogger<Startup>();
        }

        public IConfiguration Configuration { get; }
        public ILogger<Startup> logger;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddDbContext<OrderDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddScoped<IOrderRepository, OrderRepository>();

            //Specific DbContext for use from singleton AzServiceBusConsumer
            var optionsBuilder = new DbContextOptionsBuilder<OrderDbContext>();
            optionsBuilder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));

            services.AddSingleton(new OrderRepository(optionsBuilder.Options));

            services.AddSingleton<IMessageBus, AzServiceBusMessageBus>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Ordering API", Version = "v1" });
            });

            services.AddSingleton<IAzServiceBusConsumer, AzServiceBusConsumer>();

            services.AddControllers();

            try
            {
	            services.AddHealthChecks()
	                .AddAzureServiceBusTopicHealthCheck(Configuration["ServiceBusConnectionString"],
	                    Configuration["OrderPaymentRequestMessageTopic"], "Order Payment Request Topic", HealthStatus.Unhealthy)
	                .AddAzureServiceBusTopicHealthCheck(Configuration["ServiceBusConnectionString"],
	                    Configuration["OrderPaymentUpdatedMessageTopic"], "Order Payment Updated Topic", HealthStatus.Unhealthy);
            }
            catch (Exception e)
            {
	            logger.LogError(e, " on calling HealthChecks");
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();


            app.UseRouting();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ordering API V1");

            });

            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultHealthChecks();
                endpoints.MapControllers();
            });

            try
            {
            app.UseAzServiceBusConsumer();
            }
            catch (Exception e)
            {
	            logger.LogError(e, " on subscribing Azure Service Bus");
            }
        }
    }
}
