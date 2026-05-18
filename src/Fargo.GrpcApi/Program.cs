using Fargo.GrpcApi.Interceptors;
using Fargo.GrpcApi.Services;
using Fargo.Infrastructure.Extensions;
using Fargo.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<FargoGrpcExceptionInterceptor>();
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddFargoInfrastructure(builder.Configuration);

builder.Services.AddFargoAuthentication(builder.Configuration);

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapGrpcService<FargoWorkspaceGrpcService>()
    .RequireAuthorization();

app.MapGet("/", () => "Fargo gRPC API. Use a gRPC client to call services.");

app.MapDefaultEndpoints();

app.Run();

public partial class Program;
