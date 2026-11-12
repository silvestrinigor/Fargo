namespace Fargo.Application.Operations.Outbox.Dtos;

public sealed record OutboxBatchResultDto(int Claimed, int Published, bool Failed);
