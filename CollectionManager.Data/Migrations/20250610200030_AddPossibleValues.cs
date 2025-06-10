using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xhunter74.CollectionManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPossibleValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PossibleValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CollectionFieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Updated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PossibleValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PossibleValues_CollectionFields_CollectionFieldId",
                        column: x => x.CollectionFieldId,
                        principalTable: "CollectionFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PossibleValues_CollectionFieldId",
                table: "PossibleValues",
                column: "CollectionFieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PossibleValues");
        }
    }
}
