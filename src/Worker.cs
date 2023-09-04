using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using IdentityService.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityService
{
    public class Worker : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public Worker(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            await RegisterApplicationsAsync(scope.ServiceProvider);
            await RegisterScopesAsync(scope.ServiceProvider);

            static async Task RegisterApplicationsAsync(IServiceProvider provider)
            {
                var manager = provider.GetRequiredService<IOpenIddictApplicationManager>();

                // Angular UI client
                if (await manager.FindByClientIdAsync("angularclient") is null)
                {
                    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                    {
                        ClientId = "angularclient",
                        ConsentType = ConsentTypes.Explicit,
                        DisplayName = "angular client PKCE",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("fr-FR")] = "Application cliente MVC"
                        },
                        PostLogoutRedirectUris =
                        {
                            new Uri("https://checkit4you.makulec.pl") //TODO: Change to https
                        },
                        RedirectUris =
                        {
                            new Uri("https://checkit4you.makulec.pl") //TODO: Change to https
                        },
                        Permissions =
                        {
                            Permissions.Endpoints.Authorization,
                            Permissions.Endpoints.Logout,
                            Permissions.Endpoints.Token,
                            Permissions.Endpoints.Revocation,
                            Permissions.GrantTypes.AuthorizationCode,
                            Permissions.GrantTypes.RefreshToken,
                            Permissions.ResponseTypes.Code,
                            Permissions.Scopes.Email,
                            Permissions.Scopes.Profile,
                            Permissions.Scopes.Roles,
                            Permissions.Prefixes.Scope + "fileManagementService"
                        },
                        Requirements =
                        {
                            Requirements.Features.ProofKeyForCodeExchange
                        }
                    });
                }

                //// API application CC
                //if (await manager.FindByClientIdAsync("CC") == null)
                //{
                //    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                //    {
                //        ClientId = "CC",
                //        ClientSecret = "cc_secret",
                //        DisplayName = "CC for protected API",
                //        Permissions =
                //    {
                //        Permissions.Endpoints.Authorization,
                //        Permissions.Endpoints.Token,
                //        Permissions.GrantTypes.ClientCredentials,
                //        Permissions.Prefixes.Scope + "dataEventRecords"
                //    }
                //    });
                //}

                //// API
                if (await manager.FindByClientIdAsync("FileManagementServiceApi") == null)
                {
                    var descriptor = new OpenIddictApplicationDescriptor
                    {
                        ClientId = "FileManagementServiceApi",
                        ClientSecret = "FileManagementServiceSercret_651e8004-f09c-4138-a141-f78beace77a4",
                        Permissions =
                        {
                            Permissions.Endpoints.Introspection
                        }
                    };

                    await manager.CreateAsync(descriptor);
                }

                //// Blazor Hosted
                //if (await manager.FindByClientIdAsync("blazorcodeflowpkceclient") is null)
                //{
                //    await manager.CreateAsync(new OpenIddictApplicationDescriptor
                //    {
                //        ClientId = "blazorcodeflowpkceclient",
                //        ConsentType = ConsentTypes.Explicit,
                //        DisplayName = "Blazor code PKCE",
                //        DisplayNames =
                //        {
                //            [CultureInfo.GetCultureInfo("fr-FR")] = "Application cliente MVC"
                //        },
                //        PostLogoutRedirectUris =
                //        {
                //            new Uri("https://localhost:44348/signout-callback-oidc"),
                //            new Uri("https://localhost:5001/signout-callback-oidc")
                //        },
                //        RedirectUris =
                //        {
                //            new Uri("https://localhost:44348/signin-oidc"),
                //            new Uri("https://localhost:5001/signin-oidc")
                //        },
                //        ClientSecret = "codeflow_pkce_client_secret",
                //        Permissions =
                //        {
                //            Permissions.Endpoints.Authorization,
                //            Permissions.Endpoints.Logout,
                //            Permissions.Endpoints.Token,
                //            Permissions.Endpoints.Revocation,
                //            Permissions.GrantTypes.AuthorizationCode,
                //            Permissions.GrantTypes.RefreshToken,
                //            Permissions.ResponseTypes.Code,
                //            Permissions.Scopes.Email,
                //            Permissions.Scopes.Profile,
                //            Permissions.Scopes.Roles,
                //            Permissions.Prefixes.Scope + "dataEventRecords"
                //        },
                //        Requirements =
                //        {
                //            Requirements.Features.ProofKeyForCodeExchange
                //        }
                //    });
                //}
            }

            static async Task RegisterScopesAsync(IServiceProvider provider)
            {
                var manager = provider.GetRequiredService<IOpenIddictScopeManager>();

                if (await manager.FindByNameAsync("fileManagementService") is null)
                {
                    await manager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        DisplayName = "FileManagement API access",
                        DisplayNames =
                        {
                            [CultureInfo.GetCultureInfo("en-EN")] = "Acces to File Management API"
                        },
                        Name = "fileManagementService",
                        Resources =
                        {
                            "FileManagementServiceApi"
                        }
                    });
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
