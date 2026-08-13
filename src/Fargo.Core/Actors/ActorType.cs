namespace Fargo.Core.Actors;

/// <summary>
/// Represents the type of actor responsible for performing an action
/// within the system.
/// </summary>
public enum ActorType : byte
{
    /// <summary>
    /// Represents a human user authenticated in the system.
    /// </summary>
    User = 1
}
