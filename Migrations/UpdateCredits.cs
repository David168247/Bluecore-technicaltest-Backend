using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BluecoreApi.Migrations
{
    public partial class UpdateCredits : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE esquema_c.credit_cases RENAME CONSTRAINT "PK_credit_cases" TO pk_credit_cases;""");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Comment",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "comment");

            migrationBuilder.RenameColumn(
                name: "Amount",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "TermMonths",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "term_months");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ApplicantId",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "applicant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER TABLE esquema_c.credit_cases RENAME CONSTRAINT pk_credit_cases TO "PK_credit_cases";""");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "comment",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "amount",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "term_months",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "TermMonths");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "applicant_id",
                schema: "esquema_c",
                table: "credit_cases",
                newName: "ApplicantId");
        }
    }
}
