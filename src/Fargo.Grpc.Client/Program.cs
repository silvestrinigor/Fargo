using Fargo.Grpc.Client;
using Fargo.Grpc.Client.Interceptors;
using Fargo.Grpc.Client.Services;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

// The port number must match the port of the gRPC server.
using var channel = GrpcChannel.ForAddress("https://localhost:5255");

var tokenStore = new TokenStore();

var authenticatedChannel = channel.Intercept(new AuthenticationInterceptor(tokenStore));

var client = new Greeter.GreeterClient(channel);

var reply = await client.SayHelloAsync(
    new HelloRequest { Name = "GreeterClient" });

Console.WriteLine("Greeting: " + reply.Message);
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
