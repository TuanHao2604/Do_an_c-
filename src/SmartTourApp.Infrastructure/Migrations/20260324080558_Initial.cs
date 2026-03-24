using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartTourApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "heatmap_cells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GridLat = table.Column<double>(type: "double precision", nullable: false),
                    GridLng = table.Column<double>(type: "double precision", nullable: false),
                    HourBucket = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    HitCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_heatmap_cells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "languages",
                columns: table => new
                {
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_languages", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "service_packages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "pois",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<Point>(type: "geometry(Point, 4326)", nullable: false),
                    GeofenceRadius = table.Column<double>(type: "double precision", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    QrValue = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pois", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pois_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "audio_files",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FileUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsTts = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audio_files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audio_files_languages_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "languages",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_audio_files_pois_PoiId",
                        column: x => x.PoiId,
                        principalTable: "pois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "poi_contents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    LanguageCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OperatingHours = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poi_contents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_poi_contents_languages_LanguageCode",
                        column: x => x.LanguageCode,
                        principalTable: "languages",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_poi_contents_pois_PoiId",
                        column: x => x.PoiId,
                        principalTable: "pois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "poi_images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsThumbnail = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poi_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_poi_images_pois_PoiId",
                        column: x => x.PoiId,
                        principalTable: "pois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "poi_operating_hours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    CloseTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poi_operating_hours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_poi_operating_hours_pois_PoiId",
                        column: x => x.PoiId,
                        principalTable: "pois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceUuid = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DeviceModel = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastActive = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_devices_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "poi_managers",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poi_managers", x => new { x.UserId, x.PoiId });
                    table.ForeignKey(
                        name: "FK_poi_managers_pois_PoiId",
                        column: x => x.PoiId,
                        principalTable: "pois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_poi_managers_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "poi_reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_poi_reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_poi_reviews_pois_PoiId",
                        column: x => x.PoiId,
                        principalTable: "pois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_poi_reviews_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_favorites",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_favorites", x => new { x.UserId, x.PoiId });
                    table.ForeignKey(
                        name: "FK_user_favorites_pois_PoiId",
                        column: x => x.PoiId,
                        principalTable: "pois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_favorites_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_user_subscriptions_service_packages_PackageId",
                        column: x => x.PackageId,
                        principalTable: "service_packages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_subscriptions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "location_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Location = table.Column<Point>(type: "geometry(Point, 4326)", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_location_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_location_logs_devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "visit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PoiId = table.Column<Guid>(type: "uuid", nullable: false),
                    TriggerType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    VisitedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_visit_logs_devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_visit_logs_pois_PoiId",
                        column: x => x.PoiId,
                        principalTable: "pois",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("c1111111-1111-1111-1111-111111111111"), "Điểm tham quan" },
                    { new Guid("c2222222-2222-2222-2222-222222222222"), "Nhà hàng" },
                    { new Guid("c3333333-3333-3333-3333-333333333333"), "Khách sạn" },
                    { new Guid("c4444444-4444-4444-4444-444444444444"), "Bảo tàng" }
                });

            migrationBuilder.InsertData(
                table: "languages",
                columns: new[] { "Code", "IsDefault", "Name" },
                values: new object[,]
                {
                    { "en", false, "English" },
                    { "vi", true, "Tiếng Việt" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "User" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Manager" }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "PasswordHash", "RoleId", "Username" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "admin@smarttour.com", "System Admin", true, "$2a$11$I3nakc3XFIJYOLpogz.bBO8nGKX1QLNA/wvF3nUBRVPZwli/SEVr2", new Guid("11111111-1111-1111-1111-111111111111"), "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_audio_files_LanguageCode",
                table: "audio_files",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_audio_files_PoiId",
                table: "audio_files",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_devices_DeviceUuid",
                table: "devices",
                column: "DeviceUuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_devices_UserId",
                table: "devices",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_heatmap_cells_GridLat_GridLng_HourBucket",
                table: "heatmap_cells",
                columns: new[] { "GridLat", "GridLng", "HourBucket" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_location_logs_DeviceId",
                table: "location_logs",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_location_logs_LoggedAt",
                table: "location_logs",
                column: "LoggedAt");

            migrationBuilder.CreateIndex(
                name: "IX_poi_contents_LanguageCode",
                table: "poi_contents",
                column: "LanguageCode");

            migrationBuilder.CreateIndex(
                name: "IX_poi_contents_PoiId_LanguageCode",
                table: "poi_contents",
                columns: new[] { "PoiId", "LanguageCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_poi_images_PoiId",
                table: "poi_images",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_poi_managers_PoiId",
                table: "poi_managers",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_poi_operating_hours_PoiId_DayOfWeek",
                table: "poi_operating_hours",
                columns: new[] { "PoiId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_poi_reviews_CreatedAt",
                table: "poi_reviews",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_poi_reviews_PoiId",
                table: "poi_reviews",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_poi_reviews_UserId",
                table: "poi_reviews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_pois_CategoryId",
                table: "pois",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_pois_Location",
                table: "pois",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Name",
                table: "roles",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_service_packages_Code",
                table: "service_packages",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_favorites_PoiId",
                table: "user_favorites",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_PackageId",
                table: "user_subscriptions",
                column: "PackageId");

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_UserId",
                table: "user_subscriptions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Email",
                table: "users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_RoleId",
                table: "users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_users_Username",
                table: "users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_visit_logs_DeviceId",
                table: "visit_logs",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_visit_logs_PoiId",
                table: "visit_logs",
                column: "PoiId");

            migrationBuilder.CreateIndex(
                name: "IX_visit_logs_VisitedAt",
                table: "visit_logs",
                column: "VisitedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audio_files");

            migrationBuilder.DropTable(
                name: "heatmap_cells");

            migrationBuilder.DropTable(
                name: "location_logs");

            migrationBuilder.DropTable(
                name: "poi_contents");

            migrationBuilder.DropTable(
                name: "poi_images");

            migrationBuilder.DropTable(
                name: "poi_managers");

            migrationBuilder.DropTable(
                name: "poi_operating_hours");

            migrationBuilder.DropTable(
                name: "poi_reviews");

            migrationBuilder.DropTable(
                name: "user_favorites");

            migrationBuilder.DropTable(
                name: "user_subscriptions");

            migrationBuilder.DropTable(
                name: "visit_logs");

            migrationBuilder.DropTable(
                name: "languages");

            migrationBuilder.DropTable(
                name: "service_packages");

            migrationBuilder.DropTable(
                name: "devices");

            migrationBuilder.DropTable(
                name: "pois");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
