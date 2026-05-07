using System;
using System.Net;
using System.Text.Json.Serialization;
using B2BAdmin.ApiDocument.Helpers;
using B2BAdmin.ApiDocument.Infrastructure;
using B2BAdmin.ApiDocument.Services;
using CloudKit.Infrastructure.Data.MongoDb;
using CloudKit.Infrastructure.ValidationModel;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.OpenApi.Models;


namespace ApiPlugin
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            //Register Syncfusion license
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NTkzOTM2QDMxMzkyZTM0MmUzMFltOTZTSnZURjNDcEFFbkVlN1VRUldNVFV5MEdqaEtEZzhMWXFCQklBRE09");
            Configuration = configuration;
            
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.IgnoreNullValues = true;
                options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;                
            });
            IdentityModelEventSource.ShowPII = true; //Add this line
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            services.AddHealthChecks();

            services.Configure<MongoDatabaseSettings>(Configuration.GetSection("MongoDatabaseSettings"));
            services.AddDbContext<sqlDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("sqlTourChain")));
            services.AddCors(options =>
             {
                 options.AddPolicy("AllowAllOriginsPolicy",
                 builder =>
                 {
                     //builder.AllowAnyOrigin();
                     builder.WithOrigins(Configuration.GetSection("CorsOrigins").Get<string[]>())
                         .AllowAnyHeader()
                         .AllowAnyMethod()
                         .AllowCredentials();
                 });
             });
            //  for all 
            services.AddCors();

            services.AddSignalR();
            services.AddValidatorsFromAssemblyContaining(typeof(AuthenticateApiDocumentCommand), ServiceLifetime.Transient);
            services.Scan(scan =>
                scan
                    .FromAssembliesOf(typeof(AuthenticateApiDocumentCommand))
                    .AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)))
                    .AsImplementedInterfaces()
                    .WithTransientLifetime()
            );
            services.AddValidatorsFromAssemblyContaining(typeof(Startup), ServiceLifetime.Transient);
            services.AddHealthChecks();
            services.AddCors();
            services.AddScoped<IUserServiceDocument, UserServiceDocument>();
            services.AddHttpClient();
            services.AddHttpContextAccessor();
            services.AddMediatR(typeof(Startup));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "B2B Admin API",
                    Version = "v1",
                    Description = "For Website Tourchain",
                    TermsOfService = new Uri("http://tourchain.net"),
                    Contact = new OpenApiContact
                    {
                        Name = "Tour Chain",
                        Email = "hung.vu@tourchain.net",
                        Url = new Uri("https://tourchain.net/#who-we-are"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Tourchain.net",
                        Url = new Uri("https://tourchain.net/#who-we-are"),
                    }
                    });
                c.DocInclusionPredicate((_, api) => !string.IsNullOrWhiteSpace(api.GroupName));
                c.TagActionsBy(api => api.GroupName);
                //c.CustomSchemaIds(x => x.FullName);
            });

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration["RedisUrl"];
            });            
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //app.UseMiddleware<ExceptionHandlerMiddleware>();

            app.UsePathBase(Configuration["PathBase"] ?? "");

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            app.UseMiddleware<JwtMiddlewareDocment>();
            app.UseDirectoryBrowser();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            
            app.UseSwaggerUI(c =>
            {
                
                c.SwaggerEndpoint("v1/swagger.json", "B2B Admin API V1");
                c.ValidatorUrl("/api");
                // Additional OAuth settings (See https://github.com/swagger-api/swagger-ui/blob/v3.10.0/docs/usage/oauth2.md)
                c.OAuthClientId("SwaggerClient");
                c.OAuthAppName("Swagger Client");
                c.OAuthScopeSeparator(" ");
            });

            // app.UseHttpsRedirection();

            app.UseCors("AllowAllOriginsPolicy");

              // global cors policy
            app.UseCors(x => x
                .AllowAnyMethod()
                .AllowAnyHeader()
                .SetIsOriginAllowed(origin => true) // allow any origin
                .AllowCredentials()); // allow credentials

            app.UseRouting();
            app.UseWebSockets();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseSignalR(routes =>
            //{
            //    routes.MapHub<messageHub>("/messageHub");
            //});
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapHub<messageHub>("/messageHub");
                endpoints.MapHealthChecks("/healthz", new HealthCheckOptions
                {
                    Predicate = _ => true
                });
                endpoints.MapHealthChecks("/liveness", new HealthCheckOptions
                {
                    Predicate = r => r.Name.Contains("self")
                });
                endpoints.MapControllers();
            });
        }
    }
}
