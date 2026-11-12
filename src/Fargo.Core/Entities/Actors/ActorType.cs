namespace Fargo.Core.Entities.Actors;

public enum ActorType : byte
{
    User = 1,

    /// <summary>
    /// Non-interactive machine identity authenticated through the client-credentials flow.
    /// </summary>
    Service = 2
}
