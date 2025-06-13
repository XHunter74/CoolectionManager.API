using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xhunter74.CollectionManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class RedesignedCollectionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "CollectionFields");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "CollectionFields",
                newName: "DisplayName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DisplayName",
                table: "CollectionFields",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "CollectionFields",
                type: "text",
                nullable: true);
        }
    }
}
