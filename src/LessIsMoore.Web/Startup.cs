using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace LessIsMoore.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(System.IO.Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }

    public class Startup
    {

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            //===========================================================================
            // Add identity types
            //services.AddIdentity<ApplicationUser, ApplicationRole>()
            //    .AddDefaultTokenProviders();

            // Identity Services
            services.AddTransient<IUserStore<IdentityUser>, UserStore>();
            //services.AddTransient<IRoleStore<ApplicationRole>, CustomRoleStore>();
            //string connectionString = Configuration.GetConnectionString("DefaultConnection");
            //services.AddTransient<SqlConnection>(e => new SqlConnection(connectionString));
            //services.AddTransient<DapperUsersTable>();
            //===========================================================================

            services.AddMvc();
           //services.Configure<MvcOptions>(options =>
           // {
           //     var formatter = new JsonInputFormatter();
           //     formatter.SupportedMediaTypes.Add(MediaTypeHeaderValue.Parse("application/csp-report"));
           //     options.InputFormatters.RemoveType<JsonInputFormatter>();
           //     options.InputFormatters.Add(formatter);
           // });

            services.AddAuthentication(opts => opts.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

            services.AddMemoryCache();
            //services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                //options.IdleTimeout = TimeSpan.FromSeconds(10);
                options.Cookie.HttpOnly= true;
            });

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.TryAddSingleton<LIM.TextTranslator.Interfaces.ISelectedLanguage, LIM.TextTranslator.Models.SelectedLanguage>();
            services.TryAddSingleton<LIM.TextTranslator.Interfaces.ITextTranslator, LIM.TextTranslator.TextTranslator>();

            services.AddApplicationInsightsTelemetry(Configuration);


            // Add functionality to inject IOptions<T>
            services.AddOptions();

            // Add our Config object so it can be injected
            services.Configure< Models.AppSettings>(Configuration.GetSection("AppSettings"));

            // *If* you need access to generic IConfiguration this is **required**
            services.AddSingleton<IConfiguration>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, 
            LIM.TextTranslator.Interfaces.ITextTranslator textTranslator, Microsoft.Extensions.Options.IOptions<Models.AppSettings> settings)
        {
            textTranslator.SetSettings = settings.Value.TranslatorSettings;

            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();
            app.UseApplicationInsightsRequestTelemetry();
            app.UseApplicationInsightsExceptionTelemetry();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseSession();

            ConfigureAuthentication();

            app.Use(async (context, next) =>
            {
                try
                {
                    //Add content security policies
                    System.Text.StringBuilder sbCSP = new System.Text.StringBuilder();
                    sbCSP.Append("img-src 'self' data: ; ");
                    //sbCSP.Append("script-src 'self' 'unsafe-inline'; ");
                    //sbCSP.Append("script-src 'self' 'unsafe-eval'; ");
                    sbCSP.Append("report-uri /cspreport; ");

                    context.Response.Headers.Add("Content-Security-Policy", sbCSP.ToString());

                    if (context.Request.Cookies["language"] != null)
                    {
                        string strCulture = context.Request.Cookies["language"];

                        System.Globalization.CultureInfo.DefaultThreadCurrentCulture = new System.Globalization.CultureInfo(strCulture);
                        System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = new System.Globalization.CultureInfo(strCulture);
                    }

                    await next();
                }
                catch (Exception e)
                {
                    new BLL.VSTS(settings.Value.VSTSToken).SaveWorkItem(
                        new Models.VSTSWorkItem() {
                            Comments = "", Title = e.Message, Steps = e.ToString(), Type = "Bug", id= -1
                        }
                    );

                    await context.Response.WriteAsync(string.Format(@"You got an error, Chief!! {0}========================{1}{2}", 
                        Environment.NewLine, Environment.NewLine, e.ToString()));
                }

            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

            });

        }


        private void ConfigureAuthentication()
        {
            //// Configure the OWIN pipeline to use cookie auth.
            //app.UseCookieAuthentication(new CookieAuthenticationOptions());

            //// Configure the OWIN pipeline to use OpenID Connect auth.
            //app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
            //{
            //    ClientId = Configuration["AzureAD:ClientId"],
            //    Authority = System.String.Format(Configuration["AzureAd:AadInstance"], Configuration["AzureAd:Tenant"]),
            //    ResponseType = OpenIdConnectResponseType.IdToken,
            //    PostLogoutRedirectUri = Configuration["AzureAd:PostLogoutRedirectUri"],
            //    //API = Configuration["AzureAd:APIKey"],
            //    Events = new OpenIdConnectEvents
            //    {
            //        OnRemoteFailure = OnAuthenticationFailed,
            //        //OnAuthenticationFailed = async (context) =>
            //        //{
            //        //    await context.Response.WriteAsync("Failed authentication");
            //        //    context.Response.StatusCode = 403;
            //        //    context.HandleResponse();
            //        //},
            //    },

            //    TokenValidationParameters = new TokenValidationParameters
            //    {
            //         ValidateIssuer = false,
            //        RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
            //    }
            //});
        }

        // Handle sign-in errors differently than generic errors.
        private Task OnAuthenticationFailed(Microsoft.AspNetCore.Authentication.RemoteFailureContext context)
        {
            context.HandleResponse();
            context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
            return Task.FromResult(0);
        }

    }
}
