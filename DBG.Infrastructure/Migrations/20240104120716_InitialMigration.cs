using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DBG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "db_states");

            migrationBuilder.EnsureSchema(
                name: "core");

            migrationBuilder.EnsureSchema(
                name: "os_states");

            migrationBuilder.CreateTable(
                name: "db_dynamic_states",
                schema: "db_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DbSystemEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionsCount = table.Column<int>(type: "int", nullable: false),
                    DbAndTableSizesAsJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_dynamic_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "db_static_states",
                schema: "db_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DbSystemEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxConnectionsCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_static_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "db_system_entries",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DbType = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_db_system_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "os_dynamic_states",
                schema: "os_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OsSystemEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CpuUsage = table.Column<double>(type: "float", nullable: false),
                    RamUsage = table.Column<double>(type: "float", nullable: false),
                    DiskUsageAsJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_dynamic_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "os_static_states",
                schema: "os_states",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OsSystemEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CpuCount = table.Column<int>(type: "int", nullable: false),
                    RamCount = table.Column<int>(type: "int", nullable: false),
                    Uptime = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_static_states", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "os_system_entries",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OsType = table.Column<int>(type: "int", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_os_system_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "system_entries",
                schema: "core",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DbEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OsEntryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_system_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_system_entries_db_system_entries_DbEntryId",
                        column: x => x.DbEntryId,
                        principalSchema: "core",
                        principalTable: "db_system_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_system_entries_os_system_entries_OsEntryId",
                        column: x => x.OsEntryId,
                        principalSchema: "core",
                        principalTable: "os_system_entries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "core",
                table: "users",
                columns: new[] { "Id", "CreatedOn", "Email", "FirstName", "LastName", "Password", "RefreshToken", "RefreshTokenExpiryTime", "Role", "UpdatedOn" },
                values: new object[,]
                {
                    { new Guid("05988d8d-4ce8-48b9-b524-713d1c0758c0"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@admin.com", null, null, "C7AD44CBAD762A5DA0A452F9E854FDC1E0E7A52A38015F23F3EAB1D80B931DD472634DFAC71CD34EBC35D16AB7FB8A90C81F975113D6C7538DC69DD8DE9077EC", null, null, 0, null },
                    { new Guid("4b65c84c-933a-47e2-87bb-cc85642e3e1e"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "viewer@admin.com", null, null, "A8D73E712D9257A75BCE54754E0AD3074894E29FEEEC1709F9E47B761DC38D7AB923A62F1B4883A19569115E8B68850CC86B27FDA81A0DAA5305538E4D910168", null, null, 1, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_db_dynamic_states_DbSystemEntryId",
                schema: "db_states",
                table: "db_dynamic_states",
                column: "DbSystemEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_db_static_states_DbSystemEntryId",
                schema: "db_states",
                table: "db_static_states",
                column: "DbSystemEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_os_dynamic_states_OsSystemEntryId",
                schema: "os_states",
                table: "os_dynamic_states",
                column: "OsSystemEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_os_static_states_OsSystemEntryId",
                schema: "os_states",
                table: "os_static_states",
                column: "OsSystemEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_system_entries_DbEntryId",
                schema: "core",
                table: "system_entries",
                column: "DbEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_system_entries_OsEntryId",
                schema: "core",
                table: "system_entries",
                column: "OsEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                schema: "core",
                table: "users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "db_dynamic_states",
                schema: "db_states");

            migrationBuilder.DropTable(
                name: "db_static_states",
                schema: "db_states");

            migrationBuilder.DropTable(
                name: "os_dynamic_states",
                schema: "os_states");

            migrationBuilder.DropTable(
                name: "os_static_states",
                schema: "os_states");

            migrationBuilder.DropTable(
                name: "system_entries",
                schema: "core");

            migrationBuilder.DropTable(
                name: "users",
                schema: "core");

            migrationBuilder.DropTable(
                name: "db_system_entries",
                schema: "core");

            migrationBuilder.DropTable(
                name: "os_system_entries",
                schema: "core");
        }
    }
}
