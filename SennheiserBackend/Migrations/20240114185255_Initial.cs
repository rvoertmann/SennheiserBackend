using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SennheiserBackend.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Receivers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Ip = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MicrophoneEntity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    MicGain = table.Column<int>(type: "INTEGER", nullable: false),
                    ReceiverId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MicrophoneEntity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MicrophoneEntity_Receivers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Receivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MicrophoneEntity_Id",
                table: "MicrophoneEntity",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MicrophoneEntity_ReceiverId",
                table: "MicrophoneEntity",
                column: "ReceiverId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Receivers_Id",
                table: "Receivers",
                column: "Id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MicrophoneEntity");

            migrationBuilder.DropTable(
                name: "Receivers");
        }
    }
}
