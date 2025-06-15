using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xhunter74.CollectionManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToCollectionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionFields_CollectionId",
                table: "CollectionFields");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionFields_CollectionId_Order",
                table: "CollectionFields",
                columns: new[] { "CollectionId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CollectionFields_CollectionId_Order",
                table: "CollectionFields");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionFields_CollectionId",
                table: "CollectionFields",
                column: "CollectionId");
        }
    }
}
