using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nameid = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                    Permissions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedByUserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Guid);
                    table.UniqueConstraint("AK_Users_Nameid", x => x.Nameid);
                    table.ForeignKey(
                        name: "FK_Users_Users_UpdatedByUserGuid",
                        column: x => x.UpdatedByUserGuid,
                        principalTable: "Users",
                        principalColumn: "Guid");
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsContainer = table.Column<bool>(type: "bit", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedByUserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Articles_Users_UpdatedByUserGuid",
                        column: x => x.UpdatedByUserGuid,
                        principalTable: "Users",
                        principalColumn: "Guid");
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ArticlesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParentItemGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedByUserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Items_Articles_ArticleGuid",
                        column: x => x.ArticleGuid,
                        principalTable: "Articles",
                        principalColumn: "Guid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Items_Items_ParentItemGuid",
                        column: x => x.ParentItemGuid,
                        principalTable: "Items",
                        principalColumn: "Guid");
                    table.ForeignKey(
                        name: "FK_Items_Users_UpdatedByUserGuid",
                        column: x => x.UpdatedByUserGuid,
                        principalTable: "Users",
                        principalColumn: "Guid");
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateTable(
                name: "Partitions",
                columns: table => new
                {
                    Guid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ArticleGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodEndColumn", true),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false)
                        .Annotation("SqlServer:TemporalIsPeriodStartColumn", true),
                    UserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserGuid1 = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedByUserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partitions", x => x.Guid);
                    table.ForeignKey(
                        name: "FK_Partitions_Articles_ArticleGuid",
                        column: x => x.ArticleGuid,
                        principalTable: "Articles",
                        principalColumn: "Guid");
                    table.ForeignKey(
                        name: "FK_Partitions_Items_ItemGuid",
                        column: x => x.ItemGuid,
                        principalTable: "Items",
                        principalColumn: "Guid");
                    table.ForeignKey(
                        name: "FK_Partitions_Users_UpdatedByUserGuid",
                        column: x => x.UpdatedByUserGuid,
                        principalTable: "Users",
                        principalColumn: "Guid");
                    table.ForeignKey(
                        name: "FK_Partitions_Users_UserGuid",
                        column: x => x.UserGuid,
                        principalTable: "Users",
                        principalColumn: "Guid");
                    table.ForeignKey(
                        name: "FK_Partitions_Users_UserGuid1",
                        column: x => x.UserGuid1,
                        principalTable: "Users",
                        principalColumn: "Guid");
                })
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PartitionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_UpdatedByUserGuid",
                table: "Articles",
                column: "UpdatedByUserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ArticleGuid",
                table: "Items",
                column: "ArticleGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ParentItemGuid",
                table: "Items",
                column: "ParentItemGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Items_UpdatedByUserGuid",
                table: "Items",
                column: "UpdatedByUserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_ArticleGuid",
                table: "Partitions",
                column: "ArticleGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_ItemGuid",
                table: "Partitions",
                column: "ItemGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_UpdatedByUserGuid",
                table: "Partitions",
                column: "UpdatedByUserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_UserGuid",
                table: "Partitions",
                column: "UserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_Partitions_UserGuid1",
                table: "Partitions",
                column: "UserGuid1");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UpdatedByUserGuid",
                table: "Users",
                column: "UpdatedByUserGuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Partitions")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "PartitionsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "Items")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ItemsHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "Articles")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "ArticlesHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");

            migrationBuilder.DropTable(
                name: "Users")
                .Annotation("SqlServer:IsTemporal", true)
                .Annotation("SqlServer:TemporalHistoryTableName", "UsersHistory")
                .Annotation("SqlServer:TemporalHistoryTableSchema", null)
                .Annotation("SqlServer:TemporalPeriodEndColumnName", "PeriodEnd")
                .Annotation("SqlServer:TemporalPeriodStartColumnName", "PeriodStart");
        }
    }
}
