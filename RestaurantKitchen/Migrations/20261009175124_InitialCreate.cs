using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RestaurantKitchen.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dishes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CookingTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    PortionWeightGrams = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dishes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    StockQuantity = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    CaloriesPer100g = table.Column<int>(type: "int", nullable: true),
                    Supplier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsAllergen = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DishIngredients",
                columns: table => new
                {
                    DishId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(10,3)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishIngredients", x => new { x.DishId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_DishIngredients_Dishes_DishId",
                        column: x => x.DishId,
                        principalTable: "Dishes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DishIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Dishes",
                columns: new[] { "Id", "Category", "CookingTimeMinutes", "CreatedAt", "Description", "IsAvailable", "Name", "PortionWeightGrams", "Price" },
                values: new object[,]
                {
                    { 1, 2, 90, new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Класичний червоний борщ зі свининою, подається зі сметаною.", true, "Борщ український", 350, 145m },
                    { 2, 3, 40, new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Вареники ручної ліпки з картоплею та цибулевою засмажкою.", true, "Вареники з картоплею", 250, 120m },
                    { 3, 3, 35, new DateTime(2026, 9, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Куряче філе з вершковим маслом у паніровці.", true, "Котлета по-київськи", 230, 210m },
                    { 4, 4, 20, new DateTime(2026, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Сирники з кисломолочного сиру, подаються зі сметаною.", true, "Сирники зі сметаною", 200, 115m },
                    { 5, 5, 30, new DateTime(2026, 9, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Сезонний напій із сухофруктів.", false, "Узвар", 300, 45m },
                    { 6, 1, 25, new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Картопляні деруни, подаються гарячими зі сметаною.", true, "Деруни зі сметаною", 220, 105m }
                });

            migrationBuilder.InsertData(
                table: "Ingredients",
                columns: new[] { "Id", "CaloriesPer100g", "IsAllergen", "Name", "PricePerUnit", "StockQuantity", "Supplier", "Unit" },
                values: new object[,]
                {
                    { 1, 43, false, "Буряк", 18m, 25m, "ФГ «Зелений лан»", 2 },
                    { 2, 77, false, "Картопля", 15m, 60m, "ФГ «Зелений лан»", 2 },
                    { 3, 27, false, "Капуста білокачанна", 14m, 20m, "ФГ «Зелений лан»", 2 },
                    { 4, 242, false, "Свинина", 220m, 15m, "М'ясний двір", 2 },
                    { 5, 113, false, "Куряче філе", 190m, 12m, "М'ясний двір", 2 },
                    { 6, 364, true, "Борошно пшеничне", 25m, 40m, "Млин Галичини", 2 },
                    { 7, 157, true, "Яйце куряче", 5.5m, 180m, "Птахофабрика «Лан»", 5 },
                    { 8, 206, true, "Сметана 20%", 95m, 8m, "Молочна ферма", 2 },
                    { 9, 717, true, "Масло вершкове", 380m, 6m, "Молочна ферма", 2 },
                    { 10, 40, false, "Цибуля ріпчаста", 16m, 25m, "ФГ «Зелений лан»", 2 },
                    { 11, 159, true, "Сир кисломолочний", 140m, 10m, "Молочна ферма", 2 },
                    { 12, 249, false, "Сухофрукти", 160m, 5m, "Карпатські сади", 2 },
                    { 13, 387, false, "Цукор", 30m, 25m, null, 2 },
                    { 14, 41, false, "Морква", 17m, 20m, "ФГ «Зелений лан»", 2 },
                    { 15, 395, true, "Сухарі панірувальні", 60m, 4m, null, 2 }
                });

            migrationBuilder.InsertData(
                table: "DishIngredients",
                columns: new[] { "DishId", "IngredientId", "Note", "Quantity" },
                values: new object[,]
                {
                    { 1, 1, null, 0.08m },
                    { 1, 2, null, 0.06m },
                    { 1, 3, null, 0.06m },
                    { 1, 4, null, 0.08m },
                    { 1, 8, "Для подачі", 0.03m },
                    { 1, 10, null, 0.03m },
                    { 1, 14, null, 0.03m },
                    { 2, 2, "Начинка", 0.15m },
                    { 2, 6, "Тісто", 0.1m },
                    { 2, 7, "Тісто", 0.5m },
                    { 2, 9, null, 0.01m },
                    { 2, 10, "Засмажка", 0.03m },
                    { 3, 5, null, 0.15m },
                    { 3, 6, null, 0.01m },
                    { 3, 7, "Панірування", 1m },
                    { 3, 9, "Начинка", 0.03m },
                    { 3, 15, null, 0.03m },
                    { 4, 6, null, 0.03m },
                    { 4, 7, null, 1m },
                    { 4, 8, "Для подачі", 0.04m },
                    { 4, 11, null, 0.15m },
                    { 4, 13, null, 0.015m },
                    { 5, 12, null, 0.04m },
                    { 5, 13, null, 0.02m },
                    { 6, 2, null, 0.2m },
                    { 6, 6, null, 0.02m },
                    { 6, 7, null, 1m },
                    { 6, 8, "Для подачі", 0.04m },
                    { 6, 10, null, 0.03m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dishes_Name",
                table: "Dishes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DishIngredients_IngredientId",
                table: "DishIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_Name",
                table: "Ingredients",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DishIngredients");

            migrationBuilder.DropTable(
                name: "Dishes");

            migrationBuilder.DropTable(
                name: "Ingredients");
        }
    }
}
