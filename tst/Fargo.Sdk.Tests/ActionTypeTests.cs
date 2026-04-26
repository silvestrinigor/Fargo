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

        var missingInSdk = domainValues.Keys.Except(sdkValues.Keys).ToList();
        var extraInSdk = sdkValues.Keys.Except(domainValues.Keys).ToList();

        Assert.Empty(missingInSdk);
        Assert.Empty(extraInSdk);

        foreach (var (name, domainInt) in domainValues)
        {
            Assert.Equal(domainInt, sdkValues[name]);
        }
    }
}
