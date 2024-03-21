using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace waterprj.Data.Migrations
{
    public partial class estimationMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Estimation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EstimatedVolume = table.Column<double>(type: "float", nullable: false),
                    NumberOfPeople = table.Column<int>(type: "int", nullable: false),
                    HasPool = table.Column<bool>(type: "bit", nullable: false),
                    UsesDishwasher = table.Column<bool>(type: "bit", nullable: false),
                    LaundryFrequency = table.Column<int>(type: "int", nullable: false),
                    ShowerDuration = table.Column<int>(type: "int", nullable: false),
                    LeakDetection = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Estimation", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Estimation");
        }
    }
}
