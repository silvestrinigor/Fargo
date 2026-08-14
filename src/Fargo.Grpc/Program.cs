using Fargo.Application.Extensions;
using Fargo.Grpc.Interceptors;
using Fargo.Grpc.Services;
using Fargo.Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<GeneralExceptionInterceptor>();
    options.Interceptors.Add<FargoCoreExceptionInterceptor>();
});

builder.Services.AddFargoApplication();

builder.Services.AddFargoInfrastructure(builder.Configuration);

builder.Services
.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapGrpcService<IdentityService>();

app.MapGrpcService<ArticleService>().RequireAuthorization();

app.Run();
