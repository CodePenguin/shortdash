using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace ShortDash.Server.Migrations
{
    public partial class InitialSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigurationSections",
                columns: table => new
                {
                    ConfigurationSectionId = table.Column<string>(nullable: false),
                    Data = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigurationSections", x => x.ConfigurationSectionId);
                });

            migrationBuilder.CreateTable(
                name: "DashboardActionTargets",
                columns: table => new
                {
                    DashboardActionTargetId = table.Column<string>(nullable: false),
                    DataSignature = table.Column<string>(nullable: true),
                    LastSeenDateTime = table.Column<DateTime>(nullable: false),
                    LinkedDateTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Platform = table.Column<string>(nullable: true),
                    PublicKey = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardActionTargets", x => x.DashboardActionTargetId);
                });

            migrationBuilder.CreateTable(
                name: "DashboardDevices",
                columns: table => new
                {
                    DashboardDeviceId = table.Column<string>(nullable: false),
                    DataSignature = table.Column<string>(nullable: true),
                    DeviceClaims = table.Column<string>(nullable: true),
                    LastSeenDateTime = table.Column<DateTime>(nullable: false),
                    LinkedDateTime = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardDevices", x => x.DashboardDeviceId);
                });

            migrationBuilder.CreateTable(
                name: "Dashboards",
                columns: table => new
                {
                    DashboardId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BackgroundColor = table.Column<string>(nullable: true),
                    HideLabels = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dashboards", x => x.DashboardId);
                });

            migrationBuilder.CreateTable(
                name: "DashboardActions",
                columns: table => new
                {
                    DashboardActionId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActionTypeName = table.Column<string>(nullable: true),
                    BackgroundColor = table.Column<string>(nullable: true),
                    DashboardActionTargetId = table.Column<string>(nullable: true),
                    DataSignature = table.Column<string>(nullable: true),
                    Icon = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: false),
                    Parameters = table.Column<string>(nullable: true),
                    ToggleBackgroundColor = table.Column<string>(nullable: true),
                    ToggleIcon = table.Column<string>(nullable: true),
                    ToggleLabel = table.Column<string>(nullable: true),
                    ToggleState = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardActions", x => x.DashboardActionId);
                    table.ForeignKey(
                        name: "FK_DashboardActions_DashboardActionTargets_DashboardActionTargetId",
                        column: x => x.DashboardActionTargetId,
                        principalTable: "DashboardActionTargets",
                        principalColumn: "DashboardActionTargetId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DashboardCells",
                columns: table => new
                {
                    DashboardCellId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DashboardActionId = table.Column<int>(nullable: false),
                    DashboardId = table.Column<int>(nullable: false),
                    Sequence = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardCells", x => x.DashboardCellId);
                    table.ForeignKey(
                        name: "FK_DashboardCells_DashboardActions_DashboardActionId",
                        column: x => x.DashboardActionId,
                        principalTable: "DashboardActions",
                        principalColumn: "DashboardActionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DashboardCells_Dashboards_DashboardId",
                        column: x => x.DashboardId,
                        principalTable: "Dashboards",
                        principalColumn: "DashboardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DashboardSubActions",
                columns: table => new
                {
                    DashboardActionChildId = table.Column<int>(nullable: false),
                    DashboardActionParentId = table.Column<int>(nullable: false),
                    Sequence = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardSubActions", x => new { x.DashboardActionChildId, x.DashboardActionParentId });
                    table.ForeignKey(
                        name: "FK_DashboardSubActions_DashboardActions_DashboardActionChildId",
                        column: x => x.DashboardActionChildId,
                        principalTable: "DashboardActions",
                        principalColumn: "DashboardActionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DashboardSubActions_DashboardActions_DashboardActionParentId",
                        column: x => x.DashboardActionParentId,
                        principalTable: "DashboardActions",
                        principalColumn: "DashboardActionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "DashboardActionTargets",
                columns: new[] { "DashboardActionTargetId", "DataSignature", "LastSeenDateTime", "LinkedDateTime", "Name", "Platform", "PublicKey" },
                values: new object[] { "000000", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "ShortDash Server", "Windows", null });

            migrationBuilder.InsertData(
                table: "Dashboards",
                columns: new[] { "DashboardId", "BackgroundColor", "HideLabels", "Name" },
                values: new object[] { 1, "#ffffff", false, "Main" });

            migrationBuilder.CreateIndex(
                name: "IX_DashboardActions_DashboardActionTargetId",
                table: "DashboardActions",
                column: "DashboardActionTargetId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardCells_DashboardActionId",
                table: "DashboardCells",
                column: "DashboardActionId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardCells_DashboardId",
                table: "DashboardCells",
                column: "DashboardId");

            migrationBuilder.CreateIndex(
                name: "IX_DashboardSubActions_DashboardActionParentId",
                table: "DashboardSubActions",
                column: "DashboardActionParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigurationSections");

            migrationBuilder.DropTable(
                name: "DashboardCells");

            migrationBuilder.DropTable(
                name: "DashboardDevices");

            migrationBuilder.DropTable(
                name: "DashboardSubActions");

            migrationBuilder.DropTable(
                name: "Dashboards");

            migrationBuilder.DropTable(
                name: "DashboardActions");

            migrationBuilder.DropTable(
                name: "DashboardActionTargets");
        }
    }
}
