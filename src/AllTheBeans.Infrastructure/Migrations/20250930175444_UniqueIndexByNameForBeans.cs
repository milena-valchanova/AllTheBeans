using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AllTheBeans.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UniqueIndexByNameForBeans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM "Beans" a USING "Beans" b
                WHERE
                    a."Id" < b."Id"
                    AND a."Name" = b."Name";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Beans_Name",
                table: "Beans",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Beans_Name",
                table: "Beans");
        }
    }
}
