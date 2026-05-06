namespace Fargo.Sdk.Tests;

public sealed class ActionTypeTests
{
    [Fact]
    public void ActionType_Should_HaveSameValuesAsDomainActionType()
    {
        var domainValues = Enum.GetValues<Domain.ActionType>()
            .ToDictionary(v => v.ToString(), v => (int)v);

        var sdkValues = Enum.GetValues<ActionType>()
            .ToDictionary(v => v.ToString(), v => (int)v);

        var extraInSdk = sdkValues.Keys.Except(domainValues.Keys).ToList();

        Assert.Empty(extraInSdk);

        foreach (var (name, sdkInt) in sdkValues)
        {
            Assert.Equal(domainValues[name], sdkInt);
        }
    }
}
