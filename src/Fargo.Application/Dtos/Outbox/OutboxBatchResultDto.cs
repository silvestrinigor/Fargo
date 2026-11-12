namespace Fargo.Application.Dtos.Outbox;

public sealed record OutboxBatchResultDto(int Claimed, int Published, bool Failed);
