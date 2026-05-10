namespace Fargo.GrpcApi;

internal sealed record GrpcProblemDetailsDefinition(
    int StatusCode,
    string Title,
    string Type);
