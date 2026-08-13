using Fargo.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<IdentityService>();

app.MapGrpcService<ArticleService>().RequireAuthorization();

app.Run();
