﻿using Core;
using Core.ElasticSearch;
using Core.EventStoreDB;
using Microsoft.OpenApi.Models;

namespace MarketBasketAnalytics.Api
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
            services.AddControllers();
            services
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ECommerce.Api", Version = "v1" });
                })
                .AddEventStoreDB(Configuration)
                .AddElasticsearch(Configuration)
                .AddCoreServices()
                .AddMarketBasketAnalytics(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger()
                .UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ECommerce.Api v1"))
                // .UseMiddleware(typeof(ExceptionHandlingMiddleware))
                .UseHttpsRedirection()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // if (env.IsDevelopment())
            // {
            //     app.ApplicationServices.UseMarketBasketAnalytics();
            // }
        }
    }
}
