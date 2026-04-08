using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Base.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBloodInventoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BloodInventories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HospitalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BloodTypeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TotalUnits = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfCreattion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodInventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodInventories_BloodTypes_BloodTypeId",
                        column: x => x.BloodTypeId,
                        principalTable: "BloodTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BloodInventories_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BloodInventoryBatches",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HospitalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BloodTypeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BloodInventoryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Units = table.Column<int>(type: "int", nullable: false),
                    RemainingUnits = table.Column<int>(type: "int", nullable: false),
                    CollectionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfCreattion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodInventoryBatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodInventoryBatches_BloodInventories_BloodInventoryId",
                        column: x => x.BloodInventoryId,
                        principalTable: "BloodInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodInventoryBatches_BloodTypes_BloodTypeId",
                        column: x => x.BloodTypeId,
                        principalTable: "BloodTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodInventoryBatches_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BloodInventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    HospitalId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BloodTypeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BloodInventoryId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChangeAmount = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    RequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedById = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfCreattion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedById = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateOfUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BloodInventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BloodInventoryTransactions_BloodInventories_BloodInventoryId",
                        column: x => x.BloodInventoryId,
                        principalTable: "BloodInventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodInventoryTransactions_BloodTypes_BloodTypeId",
                        column: x => x.BloodTypeId,
                        principalTable: "BloodTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BloodInventoryTransactions_Hospitals_HospitalId",
                        column: x => x.HospitalId,
                        principalTable: "Hospitals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BloodInventories_BloodTypeId",
                table: "BloodInventories",
                column: "BloodTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodInventories_HospitalId_BloodTypeId",
                table: "BloodInventories",
                columns: new[] { "HospitalId", "BloodTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BloodInventoryBatches_BloodInventoryId",
                table: "BloodInventoryBatches",
                column: "BloodInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodInventoryBatches_BloodTypeId",
                table: "BloodInventoryBatches",
                column: "BloodTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodInventoryBatches_HospitalId",
                table: "BloodInventoryBatches",
                column: "HospitalId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodInventoryTransactions_BloodInventoryId",
                table: "BloodInventoryTransactions",
                column: "BloodInventoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodInventoryTransactions_BloodTypeId",
                table: "BloodInventoryTransactions",
                column: "BloodTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BloodInventoryTransactions_HospitalId",
                table: "BloodInventoryTransactions",
                column: "HospitalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BloodInventoryBatches");

            migrationBuilder.DropTable(
                name: "BloodInventoryTransactions");

            migrationBuilder.DropTable(
                name: "BloodInventories");
        }
    }
}
