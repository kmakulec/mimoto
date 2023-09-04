using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using IdentityService.Data;
using IdentityService.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using IdentityService.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;
using Quartz;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace IdentityService
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
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(
                    Configuration.GetConnectionString("DefaultConnection"));
                options.UseOpenIddict();

            });

            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 5;

                options.ClaimsIdentity.UserNameClaimType = Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = Claims.Role;
                options.ClaimsIdentity.EmailClaimType = Claims.Email;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Account/Login";
                options.LogoutPath = $"/Account/Logout";
                options.AccessDeniedPath = $"/Account/AccessDenied";
            });
            //services.AddDefaultIdentity<ApplicationUser>(options =>
            //{
            //    options.SignIn.RequireConfirmedAccount = true;
            //    options.Password.RequireNonAlphanumeric = false;
            //    options.Password.RequiredLength = 5;

            //    options.ClaimsIdentity.UserNameClaimType = Claims.Name;
            //    options.ClaimsIdentity.UserIdClaimType = Claims.Subject;
            //    options.ClaimsIdentity.RoleClaimType = Claims.Role;
            //    options.ClaimsIdentity.EmailClaimType = Claims.Email;
            //})
            //    .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
                options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });

            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

            services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                       .UseDbContext<ApplicationDbContext>();

                options.UseQuartz();
            })
            .AddServer(options =>
            {
                options.DisableAccessTokenEncryption(); //USE ONLY TO TEST JWT
                options.SetAuthorizationEndpointUris("/connect/authorize")
                        .SetLogoutEndpointUris("/connect/logout")
                        .SetIntrospectionEndpointUris("/connect/introspect")
                        .SetRevocationEndpointUris("/connect/revocation")
                        .SetTokenEndpointUris("/connect/token")
                        .SetUserinfoEndpointUris("/connect/userinfo")
                        .SetVerificationEndpointUris("/connect/verify");
                

                options.AllowAuthorizationCodeFlow()
                       .AllowHybridFlow()
                       .AllowClientCredentialsFlow()
                       .AllowRefreshTokenFlow();

                options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);


                //options.AddDevelopmentEncryptionCertificate()
                //       .AddDevelopmentSigningCertificate();

                options.AddSigningCertificate(new X509Certificate2("signing-cert.pfx", "gokus1", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable));
                options.AddEncryptionCertificate(new X509Certificate2("encryption-cert.pfx", "gokus2", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.Exportable));


                options.UseAspNetCore()
                       .EnableAuthorizationEndpointPassthrough()
                       .EnableLogoutEndpointPassthrough()
                       .EnableTokenEndpointPassthrough()
                       .EnableUserinfoEndpointPassthrough();
                       //.DisableTransportSecurityRequirement(); //ENABLE AFTER TEST END AND ADD SSL
                       //.EnableStatusCodePagesIntegration();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });
            services.AddHostedService<Worker>();
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)HttpStatusCode.TemporaryRedirect;
                options.HttpsPort = 5001;
            });

            services.AddAuthentication();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder
                            .AllowAnyOrigin()
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    });
            });

            services.AddScoped<ApplicationDbContextDefaultSeed>();
            services.AddSingleton<IEmailSender, EmailSenderService>();

            services.AddControllersWithViews();
            services.AddRazorPages();
            //services.AddSpaStaticFiles();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "IdentityService", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ApplicationDbContextDefaultSeed seeder)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "IdentityService v1"));
                
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            seeder.SeedAsync().Wait();

            var forwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedHeaderOptions.KnownNetworks.Clear();
            forwardedHeaderOptions.KnownProxies.Clear();
            forwardedHeaderOptions.KnownProxies.Add(IPAddress.Parse("145.239.89.97"));
            app.UseForwardedHeaders(forwardedHeaderOptions);

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            //app.UseSpaStaticFiles();           

            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
