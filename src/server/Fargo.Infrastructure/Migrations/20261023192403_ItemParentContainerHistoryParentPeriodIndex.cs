using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations;

/// <summary>
/// Adds a GiST index on <c>item_parent_container_history
/// (parent_item_container_guid, valid_period)</c> to accelerate the temporal
/// container-walk used by the point-in-time inventory and location queries.
/// </summary>
public partial class ItemParentContainerHistoryParentPeriodIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateItemParentContainerHistoryParentPeriodIndex();
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropItemParentContainerHistoryParentPeriodIndex();
    }
}
