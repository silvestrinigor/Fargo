using Fargo.Core.Events;
using UnitsNet;

namespace Fargo.Application.Operations.Items.Dtos;

/// <param name="Moviment">
/// Set only when <paramref name="EventType"/> is <see cref="ItemEventType.Moviment"/>.
/// </param>
/// <param name="Lifecycle">
/// Set only when <paramref name="EventType"/> is <see cref="ItemEventType.Creation"/> or
/// <see cref="ItemEventType.Deletion"/>.
/// </param>
public sealed record ItemEventDto(
    Guid Guid,
    Guid ArticleGuid,
    ItemEventType EventType,
    DateTimeOffset OccuredAt,
    ItemMovimentEventDto? Moviment,
    ItemLifecycleEventDto? Lifecycle
);

/// <param name="FromItemContainerGuid">
/// <see langword="null"/> when there was no origin container or it has since been deleted; see
/// <paramref name="FromNoContainer"/>.
/// </param>
/// <param name="ToItemContainerGuid">
/// <see langword="null"/> when there was no destination container or it has since been deleted; see
/// <paramref name="ToNoContainer"/>.
/// </param>
public sealed record ItemMovimentEventDto(
    Scalar Quantity,
    Guid? FromItemContainerGuid,
    bool FromNoContainer,
    Guid? ToItemContainerGuid,
    bool ToNoContainer
);

/// <param name="ItemContainerGuid">
/// <see langword="null"/> when there was no container or it has since been deleted; see
/// <paramref name="NoContainer"/>.
/// </param>
public sealed record ItemLifecycleEventDto(
    Scalar Quantity,
    Guid? ItemContainerGuid,
    bool NoContainer
);

public static class ItemEventDtoMapping
{
    public static ItemEventDto ToDto(this ItemEvent itemEvent)
    {
        var moviment = itemEvent.Moviment;
        var lifecycle = itemEvent.Lifecycle;

        return new ItemEventDto(
            itemEvent.Guid,
            itemEvent.ArticleGuid,
            itemEvent.ItemEventType,
            itemEvent.OccuredAt,
            moviment is null
                ? null
                : new ItemMovimentEventDto(
                    moviment.Quantity,
                    moviment.FromItemContainerGuid,
                    moviment.FromNoContainer,
                    moviment.ToItemContainerGuid,
                    moviment.ToNoContainer),
            lifecycle is null
                ? null
                : new ItemLifecycleEventDto(
                    lifecycle.Quantity,
                    lifecycle.ItemContainerGuid,
                    lifecycle.NoContainer));
    }
}
