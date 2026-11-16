using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegaciesPrintersApi.Migrations
{
    /// <inheritdoc />
    public partial class AddWebOrderType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_orders_order_type",
                table: "orders");

            migrationBuilder.AddCheckConstraint(
                name: "CK_orders_order_type",
                table: "orders",
                sql: "[order_type] IN ('mobile', 'web', 'walk_in')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_orders_order_type",
                table: "orders");

            migrationBuilder.AddCheckConstraint(
                name: "CK_orders_order_type",
                table: "orders",
                sql: "[order_type] IN ('mobile', 'walk_in')");
        }
    }
}
