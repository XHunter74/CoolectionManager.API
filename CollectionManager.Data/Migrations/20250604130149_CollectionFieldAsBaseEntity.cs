﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace xhunter74.CollectionManager.Data.Migrations
{
    /// <inheritdoc />
    public partial class CollectionFieldAsBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "CollectionFields",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "CollectionFields",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "CollectionFields");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "CollectionFields");
        }
    }
}
