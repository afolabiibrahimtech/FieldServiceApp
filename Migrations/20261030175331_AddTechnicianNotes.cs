using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FieldServiceApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateCompleted",
                table: "WorkOrders",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechnicianNotes",
                table: "WorkOrders",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateCompleted",
                table: "WorkOrders");

            migrationBuilder.DropColumn(
                name: "TechnicianNotes",
                table: "WorkOrders");
        }
    }
}
