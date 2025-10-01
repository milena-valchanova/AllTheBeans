using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllTheBeans.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBeanOfTheDay : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BeansOfTheDay",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BeanId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeansOfTheDay", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeansOfTheDay_Beans_BeanId",
                        column: x => x.BeanId,
                        principalTable: "Beans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BeansOfTheDay_BeanId",
                table: "BeansOfTheDay",
                column: "BeanId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BeansOfTheDay");
        }
    }
}
