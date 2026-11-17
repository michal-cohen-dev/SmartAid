using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddHelpTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HelpTypeId",
                table: "HelpRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HelpTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VolunteerHelpTypes",
                columns: table => new
                {
                    VolunteerId = table.Column<int>(type: "int", nullable: false),
                    HelpTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VolunteerHelpTypes", x => new { x.VolunteerId, x.HelpTypeId });
                    table.ForeignKey(
                        name: "FK_VolunteerHelpTypes_HelpTypes_HelpTypeId",
                        column: x => x.HelpTypeId,
                        principalTable: "HelpTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VolunteerHelpTypes_Volunteers_VolunteerId",
                        column: x => x.VolunteerId,
                        principalTable: "Volunteers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "HelpTypes",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "התנעת רכב" },
                    { 2, "החלפת גלגל" },
                    { 3, "פתיחת רכב נעול" },
                    { 4, "חילוץ רכב תקוע" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_HelpRequests_HelpTypeId",
                table: "HelpRequests",
                column: "HelpTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpTypes_Name",
                table: "HelpTypes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VolunteerHelpTypes_HelpTypeId",
                table: "VolunteerHelpTypes",
                column: "HelpTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_HelpRequests_HelpTypes_HelpTypeId",
                table: "HelpRequests",
                column: "HelpTypeId",
                principalTable: "HelpTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HelpRequests_HelpTypes_HelpTypeId",
                table: "HelpRequests");

            migrationBuilder.DropTable(
                name: "VolunteerHelpTypes");

            migrationBuilder.DropTable(
                name: "HelpTypes");

            migrationBuilder.DropIndex(
                name: "IX_HelpRequests_HelpTypeId",
                table: "HelpRequests");

            migrationBuilder.DropColumn(
                name: "HelpTypeId",
                table: "HelpRequests");
        }
    }
}
