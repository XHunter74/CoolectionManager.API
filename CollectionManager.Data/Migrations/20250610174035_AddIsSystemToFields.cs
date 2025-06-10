using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xhunter74.CollectionManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsSystemToFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "CollectionFields",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "CollectionFields");
        }
    }
}
