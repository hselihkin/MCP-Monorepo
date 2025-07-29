using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RegistryServer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Server",
                columns: table => new
                {
                    Endpoint = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ServerDesc = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Tools = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Arguments = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Server", x => x.Endpoint);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Server");
        }
    }
}
