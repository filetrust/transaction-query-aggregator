using Flurl.Http;
using Flurl.Http.Configuration;
using System;
using System.Diagnostics.CodeAnalysis;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Configuration;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Http;
using Glasswall.Administration.K8.TransactionQueryAggregator.Business.Services;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Configuration;
using Glasswall.Administration.K8.TransactionQueryAggregator.Common.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Configuration;
using System.Linq;
using System.Net.Http;

namespace Glasswall.Administration.K8.TransactionQueryAggregator
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(logging =>
            {
                logging.AddDebug();
            })
                .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Debug);

            services.AddControllers();
            services.AddCors(options =>
            {
                options.AddPolicy("*",
                    builder =>
                    {
                        builder
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin();
                    });
            });
            
            services.TryAddSingleton(ValidateAndBind(Configuration));

            services.TryAddTransient<ITransactionService, TransactionService>();
            services.TryAddSingleton<IGlasswallHttpClient, GlasswallFlurlClient>();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseRouting();
            app.UseAuthorization();
            
            app.Use((context, next) =>
            {
                context.Response.Headers["Access-Control-Expose-Headers"] = "*";
                context.Response.Headers["Access-Control-Allow-Headers"] = "*";
                context.Response.Headers["Access-Control-Allow-Origin"] = "*";

                if (context.Request.Method != "OPTIONS") return next.Invoke();
                
                context.Response.StatusCode = 200;
                return context.Response.WriteAsync("OK");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseCors("*");
        }

        public ITransactionQueryAggregatorConfiguration ValidateAndBind(IConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration["TransactionQueryServiceEndpointCsv"]))
                throw new ConfigurationErrorsException("TransactionQueryServiceEndpointCsv must be provided");

            var endpoints = configuration["TransactionQueryServiceEndpointCsv"].Split(",");

            if (endpoints.Any(string.IsNullOrWhiteSpace))
                throw new ConfigurationErrorsException($"TransactionQueryServiceEndpointCsv was invalid, got '{configuration["TransactionQueryServiceEndpointCsv"]}'");

            if (string.IsNullOrWhiteSpace(configuration["username"]))
                throw new ConfigurationErrorsException("username was invalid");

            if (string.IsNullOrWhiteSpace(configuration["password"]))
                throw new ConfigurationErrorsException("password was invalid");

            foreach (var endpoint in endpoints)
            {
                DisableSelfSignedCertificateErrors(endpoint);
            }

            var businessConfig = new TransactionQueryAggregatorConfiguration();

            configuration.Bind(businessConfig);

            return businessConfig;
        }


        private static void DisableSelfSignedCertificateErrors(string endpoint)
        {
            FlurlHttp.ConfigureClient(endpoint, cli =>
                cli.Settings.HttpClientFactory = new UntrustedCertClientFactory());
        }
    }

    [ExcludeFromCodeCoverage]
    public class UntrustedCertClientFactory : DefaultHttpClientFactory
    {
        public override HttpMessageHandler CreateMessageHandler()
        {
            return new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (a, b, c, d) => true
            };
        }
    }
}