using Fargo.Cli.Output;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Microsoft.Kiota.Abstractions.Serialization;
using System.CommandLine;
using System.Globalization;
using System.Text;

namespace Fargo.Cli.Commands.Audits;

public sealed class GetAuditLogCommand : Command
{
    public GetAuditLogCommand(FargoApiClient client)
        : base("get", """
            Show a single audit log by GUID, including its metadata.

            Examples:
              fargo audit get 809434f3-c3b6-4f74-ad15-74e41d26468e
            """)
    {
        var guidArgument = new Argument<Guid>("guid")
        {
            Description = "The audit log GUID."
        };

        Add(guidArgument);

        SetAction(async parseResult =>
        {
            var guid = parseResult.GetValue(guidArgument);

            var auditLog = await client.AuditLogs[guid].GetAsync();

            if (auditLog is null)
            {
                Console.Error.WriteLine("Audit log not found.");
                return 1;
            }

            new DetailWriter()
                .Title("Audit log")
                .Field("GUID", auditLog.Guid)
                .Field("Type", EnumFormatting.Format<AuditLogType>(auditLog.AuditLogType))
                .Field("Actor", auditLog.ActorGuid)
                .Field("Actor type", EnumFormatting.Format<ActorType>(auditLog.ActorType))
                .Field("Entity", auditLog.EntityGuid)
                .Field("Entity type", EnumFormatting.Format<EntityType>(auditLog.EntityType))
                .Field("Occurred at", auditLog.OccurredAt?.ToString("u"))
                .Write();

            WriteMetadata(auditLog.Metadata);

            return 0;
        });
    }

    private static void WriteMetadata(AuditLogDto_metadata? metadata)
    {
        var data = metadata?.AdditionalData;

        Console.WriteLine();
        Console.WriteLine("Metadata");
        Console.WriteLine("--------");

        if (data is null || data.Count == 0)
        {
            Console.WriteLine("(none)");
            return;
        }

        var labelWidth = data.Keys.Max(k => k.Length);

        foreach (var (key, value) in data.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            Console.WriteLine($"{key.PadRight(labelWidth)}  {Render(value)}");
        }
    }

    /// <summary>
    /// Renders an audit metadata value. Values arrive as Kiota untyped nodes
    /// when parsed from JSON; scalars become their text form and objects/arrays
    /// are rendered compactly.
    /// </summary>
    private static string Render(object? value)
    {
        return value switch
        {
            null => "-",
            UntypedNull => "-",
            UntypedString s => s.GetValue() ?? "-",
            UntypedBoolean b => b.GetValue() ? "true" : "false",
            UntypedInteger i => i.GetValue().ToString(CultureInfo.InvariantCulture),
            UntypedLong l => l.GetValue().ToString(CultureInfo.InvariantCulture),
            UntypedDecimal d => d.GetValue().ToString(CultureInfo.InvariantCulture),
            UntypedDouble d => d.GetValue().ToString(CultureInfo.InvariantCulture),
            UntypedFloat f => f.GetValue().ToString(CultureInfo.InvariantCulture),
            UntypedArray array => RenderArray(array),
            UntypedObject obj => RenderObject(obj),
            _ => value.ToString() ?? "-"
        };
    }

    private static string RenderArray(UntypedArray array)
    {
        var parts = array.GetValue().Select(Render);
        return "[" + string.Join(", ", parts) + "]";
    }

    private static string RenderObject(UntypedObject obj)
    {
        var builder = new StringBuilder();
        builder.Append('{');

        var first = true;

        foreach (var (key, node) in obj.GetValue())
        {
            if (!first)
            {
                builder.Append(", ");
            }

            builder.Append(key).Append(": ").Append(Render(node));
            first = false;
        }

        builder.Append('}');
        return builder.ToString();
    }
}
