using Fargo.Application.Articles;
using Fargo.Application.Articles.Commands;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Articles;
using System.Reflection;

namespace Fargo.Application.Tests;

public sealed class CommandHandlerArchitectureTests
{
    [Fact]
    public void ApplicationHandlers_Should_OwnTheirFlow()
    {
        var handlerTypes = typeof(ICommand).Assembly
            .GetTypes()
            .Where(static type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.GetInterfaces().Any(IsHandlerInterface))
            .ToArray();

        var baseTypeViolations = handlerTypes
            .Where(static type => type.BaseType != typeof(object))
            .Select(static type => $"{type.FullName} inherits from {type.BaseType!.FullName}")
            .ToArray();

        var dependencyViolations = handlerTypes
            .SelectMany(static type => type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .SelectMany(ctor => ctor.GetParameters()
                    .Where(parameter => IsForbiddenHandlerDependency(parameter.ParameterType))
                    .Select(parameter => $"{type.FullName} depends on {parameter.ParameterType.FullName}")))
            .ToArray();

        var violations = baseTypeViolations
            .Concat(dependencyViolations)
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Application handlers should not inherit handler base flows or depend on other handlers/dispatchers:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    [Fact]
    public void ArticleCreateCommands_Should_ExposeOnlySpecificInputs()
    {
        Type[] commandTypes =
        [
            typeof(ArticleCreateCommand),
            typeof(ArticleCreateVariationCommand),
            typeof(ArticleCreatePackCommand),
            typeof(ArticleCreateKitCommand),
            typeof(ArticleCreateContainerCommand)
        ];

        Type[] forbiddenInputTypes =
        [
            typeof(ArticleCreateDto),
            typeof(ArticleType),
            typeof(ArticleCreateVariationDto),
            typeof(ArticleCreatePackDto),
            typeof(ArticleCreateKitDto),
            typeof(ArticleCreateContainerDto)
        ];

        var violations = commandTypes
            .SelectMany(commandType => commandType.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .SelectMany(ctor => ctor.GetParameters()
                    .Where(parameter => ContainsForbiddenType(parameter.ParameterType, forbiddenInputTypes))
                    .Select(parameter => $"{commandType.FullName} constructor input {parameter.Name} uses {parameter.ParameterType.FullName}")))
            .Concat(commandTypes.SelectMany(commandType => commandType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(property => ContainsForbiddenType(property.PropertyType, forbiddenInputTypes))
                .Select(property => $"{commandType.FullName} property {property.Name} uses {property.PropertyType.FullName}")))
            .ToArray();

        Assert.True(
            violations.Length == 0,
            "Article create commands should not expose the generic create DTO, article type switch, or sibling type payload DTOs:" +
            Environment.NewLine +
            string.Join(Environment.NewLine, violations));
    }

    private static bool IsHandlerInterface(Type type)
        => IsClosedTypeOf(type, typeof(ICommandHandler<>)) ||
            IsClosedTypeOf(type, typeof(ICommandHandler<,>)) ||
            IsClosedTypeOf(type, typeof(IQueryHandler<,>));

    private static bool IsForbiddenHandlerDependency(Type type)
        => type == typeof(ICommandDispatcher) ||
            type.GetInterfaces().Any(IsHandlerInterface) ||
            IsHandlerInterface(type);

    private static bool IsClosedTypeOf(Type type, Type openGenericType)
        => type.IsGenericType && type.GetGenericTypeDefinition() == openGenericType;

    private static bool ContainsForbiddenType(Type type, IReadOnlyCollection<Type> forbiddenTypes)
    {
        if (forbiddenTypes.Contains(type))
        {
            return true;
        }

        return type.IsGenericType &&
            type.GetGenericArguments().Any(argument => ContainsForbiddenType(argument, forbiddenTypes));
    }
}
