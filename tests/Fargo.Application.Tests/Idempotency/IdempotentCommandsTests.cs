using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Items;
using Fargo.Application.Commands.SupplyOrders;
using Fargo.Application.Idempotency;

namespace Fargo.Application.Tests.Idempotency;

public class IdempotentCommandsTests
{
    private static readonly string[] SensitiveFragments = ["password", "secret", "token", "hash"];

    private static IReadOnlyList<Type> IdempotentCommands()
        => [.. typeof(ICommand).Assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IIdempotentCommand).IsAssignableFrom(t))
            .OrderBy(t => t.Name, StringComparer.Ordinal)];

    [Fact]
    public void OnlyTheCommandsWhoseRetryWouldDoDamage_AreIdempotent()
    {
        // A deliberate list: adding a command here is a decision, not a side effect. These are the ones with no
        // natural guard against a retry: they create stock or physical items.
        Assert.Equal(
            [typeof(ItemCreateCommand), typeof(ItemEventCreateCommand), typeof(SupplyOrderReceiveCommand)],
            IdempotentCommands());
    }

    [Fact]
    public void NoIdempotentCommand_ReturnsAResponseThatCarriesASecret()
    {
        // The response of an idempotent command is stored to be replayed, so it must never hold a secret.
        foreach (var commandType in IdempotentCommands())
        {
            var responseType = commandType
                .GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>))
                .Select(i => i.GetGenericArguments()[0])
                .SingleOrDefault();

            if (responseType is null)
            {
                continue;
            }

            Assert.Empty(SensitiveMembers(responseType, depth: 0));
        }
    }

    private static IEnumerable<string> SensitiveMembers(Type type, int depth)
    {
        if (depth > 3 || type.IsPrimitive || type == typeof(string) || type == typeof(Guid))
        {
            yield break;
        }

        foreach (var property in type.GetProperties())
        {
            if (SensitiveFragments.Any(f => property.Name.Contains(f, StringComparison.OrdinalIgnoreCase)))
            {
                yield return $"{type.Name}.{property.Name}";
            }

            foreach (var nested in SensitiveMembers(property.PropertyType, depth + 1))
            {
                yield return nested;
            }
        }
    }
}
