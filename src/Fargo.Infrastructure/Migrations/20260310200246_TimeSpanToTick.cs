using Microsoft.EntityFrameworkCore.Migrations;
using System;

#nullable disable

namespace Fargo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TimeSpanToTick : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultPasswordExpirationTimeSpan",
                table: "Users");

            migrationBuilder.AddColumn<long>(
                name: "DefaultPasswordExpirationPeriod",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultPasswordExpirationPeriod",
                table: "Users");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "DefaultPasswordExpirationTimeSpan",
                table: "Users",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}