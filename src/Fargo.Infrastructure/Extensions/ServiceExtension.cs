using Fargo.Application.Models.ArticleModels;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Persistence;
using Fargo.Application.Repositories;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.Application.Requests.Queries.ItemQueries;
using Fargo.Application.Requests.Queries.UserQueries;
using Fargo.Domain.Repositories;
using Fargo.Domain.Security;
using Fargo.Infrastructure.Persistence;
using Fargo.Infrastructure.Persistence.Repositories;
using Fargo.Infrastructure.Security;
using Fargo.Application.Security;
using Fargo.Application.Requests.Commands.AuthCommands;
using Fargo.Application.Models.AuthModels;
using Fargo.Domain.ValueObjects;
using Fargo.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Fargo.Infrastructure.Extensions
{
    public static class ServiceExtension
    {
        extension(IServiceCollection services)
        {
            public IServiceCollection AddInfrastructure(IConfiguration configuration)
            {
                services.AddScoped<IPasswordHasher, IdentityPasswordHasher>();

                services.AddScoped<ITokenGenerator, JwtTokenGenerator>();

                services.AddScoped<ICommandHandler<InitializeSystemCommand>, InitializeSystemCommandHandler>();

                services.AddScoped<ICommandHandler<LoginCommand, AuthResult>, LoginCommandHandler>();

                services.AddScoped<ICommandHandler<ArticleCreateCommand, Guid>, ArticleCreateCommandHandler>();
                services.AddScoped<ICommandHandler<ArticleDeleteCommand>, ArticleDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<ArticleUpdateCommand>, ArticleUpdateCommandHandler>();
                services.AddScoped<IQueryHandler<ArticleSingleQuery, ArticleReadModel?>, ArticleSingleQueryHandler>();
                services.AddScoped<IQueryHandler<ArticleManyQuery, IEnumerable<ArticleReadModel>>, ArticleManyQueryHandler>();

                services.AddScoped<ICommandHandler<ItemCreateCommand, Guid>, ItemCreateCommandHandler>();
                services.AddScoped<ICommandHandler<ItemDeleteCommand>, ItemDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<ItemUpdateCommand>, ItemUpdateCommandHandler>();
                services.AddScoped<IQueryHandler<ItemSingleQuery, ItemReadModel?>, ItemSingleQueryHandler>();
                services.AddScoped<IQueryHandler<ItemManyQuery, IEnumerable<ItemReadModel>>, ItemManyQueryHandler>();

                services.AddScoped<ICommandHandler<UserCreateCommand, Guid>, UserCreateCommandHandler>();
                services.AddScoped<ICommandHandler<UserDeleteCommand>, UserDeleteCommandHandler>();
                services.AddScoped<ICommandHandler<UserUpdateCommand>, UserUpdateCommandHandler>();
                services.AddScoped<IQueryHandler<UserSingleQuery, UserReadModel?>, UserSingleQueryHandler>();
                services.AddScoped<IQueryHandler<UserManyQuery, IEnumerable<UserReadModel>>, UserManyQueryHandler>();

                services.AddScoped<IArticleRepository, ArticleRepository>();
                services.AddScoped<IArticleReadRepository, ArticleReadRepository>();
                services.AddScoped<ArticleService>();

                services.AddScoped<IItemRepository, ItemRepository>();
                services.AddScoped<IItemReadRepository, ItemReadRepository>();

                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IUserReadRepository, UserReadRepository>();

                services.AddScoped<IUnitOfWork, FargoUnitOfWork>();

                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                            {
                                options.TokenValidationParameters = new TokenValidationParameters
                                {
                                    ValidateIssuer = true,
                                    ValidateAudience = true,
                                    ValidateLifetime = true,
                                    ValidateIssuerSigningKey = true,
                                    ValidIssuer = configuration["Jwt:Issuer"],
                                    ValidAudience = configuration["Jwt:Audience"],
                                    IssuerSigningKey = new SymmetricSecurityKey(
                                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                                        )
                                };
                            });

                services.AddAuthorization();

                services.AddHttpContextAccessor();

                services.AddScoped<ICurrentUser, CurrentUser>();

                return services;
            }
        }

        extension(IServiceProvider services)
        {
            public async Task<IServiceProvider> InitInfrastructureAsync(
                    string? defaultAdminNameid,
                    string? defaultAdminPassword
                    )
            {
                using (var scope = services.CreateScope())
                {
                    var initializeSystem = scope.ServiceProvider
                        .GetRequiredService<ICommandHandler<InitializeSystemCommand>>();

                    var initializeSystemCommand = new InitializeSystemCommand(
                            defaultAdminNameid != null ? new Nameid(defaultAdminNameid) : null,
                            defaultAdminPassword != null ? new Password(defaultAdminPassword) : null
                            );

                    await initializeSystem.Handle(initializeSystemCommand);
                }

                return services;
            }
        }
    }
}