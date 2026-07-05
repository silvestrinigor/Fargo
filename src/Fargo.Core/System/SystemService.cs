using Fargo.Core.Constants;

namespace Fargo.Core.System;

public sealed class SystemService
{
    public static Guid SystemGuid => new(GuidConstants.SytemGuidString);
}