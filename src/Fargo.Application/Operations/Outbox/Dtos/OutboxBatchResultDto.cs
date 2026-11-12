namespace Fargo.Application.Operations.Outbox.Dtos;

/// <summary>
/// The outcome of publishing one outbox batch.
/// </summary>
/// <param name="Claimed">The number of messages claimed from the outbox.</param>
/// <param name="Published">The number of messages the broker accepted.</param>
/// <param name="Failed">Whether publishing stopped on a failure.</param>
public sealed record OutboxBatchResultDto(int Claimed, int Published, bool Failed);
