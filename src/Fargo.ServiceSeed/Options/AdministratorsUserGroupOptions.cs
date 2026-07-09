namespace Fargo.Application.System;

public sealed class AdministratorsUserGroupOptions
{
    public const string SectionName = "Administrators";

    public required string Nameid { get; init; }

    public string Description { get; init; } = string.Empty;
}
