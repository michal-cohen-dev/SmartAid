using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreHelpTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "HelpTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "התנעת רכב וסיוע במצבר");

            migrationBuilder.InsertData(
                table: "HelpTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 5, "אספקת דלק" },
                    { 6, "בדיקת וניפוח לחץ אוויר בצמיגים" },
                    { 7, "סיוע במילוי נוזלים ברכב" },
                    { 8, "תיאום גרר" },
                    { 9, "תקלה מכנית ברכב" },
                    { 10, "תקלה במערכת החשמל של הרכב" },
                    { 11, "חשד להתחממות הרכב" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "HelpTypes",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "HelpTypes",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "HelpTypes",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "HelpTypes",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "HelpTypes",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "HelpTypes",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "HelpTypes",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.UpdateData(
                table: "HelpTypes",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "התנעת רכב");
        }
    }
}
