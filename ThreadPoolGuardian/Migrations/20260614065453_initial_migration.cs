using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThreadPoolGuardian.Migrations
{
    /// <inheritdoc />
    public partial class initial_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThreadPoolMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PendingWorkItems = table.Column<int>(type: "int", nullable: false),
                    UsedThreads = table.Column<int>(type: "int", nullable: false),
                    AvailableThreads = table.Column<int>(type: "int", nullable: false),
                    MaxThreads = table.Column<int>(type: "int", nullable: false),
                    UtilizationPercent = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThreadPoolMetrics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ThreadPoolMetrics_Status",
                table: "ThreadPoolMetrics",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ThreadPoolMetrics_Timestamp",
                table: "ThreadPoolMetrics",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ThreadPoolMetrics");
        }
    }
}
