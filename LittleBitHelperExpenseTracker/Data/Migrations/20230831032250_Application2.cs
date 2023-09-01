using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LittleBitHelperExpenseTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class Application2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumber4",
                table: "AspNetUsers",
                newName: "PhoneNumber4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumber4",
                table: "AspNetUsers",
                newName: "PhoneNumber4");
        }
    }
}
