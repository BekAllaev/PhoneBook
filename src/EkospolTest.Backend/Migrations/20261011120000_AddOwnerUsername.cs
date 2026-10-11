using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EkospolTest.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "owner_username",
                table: "phone_numbers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "owner_username",
                table: "phone_numbers");
        }
    }
}
