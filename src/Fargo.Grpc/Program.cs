using Fargo.Application.Extensions;
using Fargo.Grpc.Services;
using Fargo.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGrpcService<IdentityService>();

app.MapGrpcService<ArticleService>().RequireAuthorization();

app.Run();
