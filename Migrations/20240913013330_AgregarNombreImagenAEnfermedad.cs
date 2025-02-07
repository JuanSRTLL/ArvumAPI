using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNombreImagenAEnfermedad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Datos_Cultivos_SemilleroId",
                table: "Datos");

            migrationBuilder.DropForeignKey(
                name: "FK_Enfermedades_Cultivos_SemilleroId",
                table: "Enfermedades");

            migrationBuilder.DropColumn(
                name: "Rol",
                table: "Usuarios");

            migrationBuilder.RenameColumn(
                name: "SemilleroId",
                table: "Enfermedades",
                newName: "CultivoId");

            migrationBuilder.RenameIndex(
                name: "IX_Enfermedades_SemilleroId",
                table: "Enfermedades",
                newName: "IX_Enfermedades_CultivoId");

            migrationBuilder.RenameColumn(
                name: "SemilleroId",
                table: "Datos",
                newName: "CultivoId");

            migrationBuilder.RenameIndex(
                name: "IX_Datos_SemilleroId",
                table: "Datos",
                newName: "IX_Datos_CultivoId");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "EnfermedadesExternas",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<string>(
                name: "NombreImagen",
                table: "Enfermedades",
                type: "longtext",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Cultivos",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AddForeignKey(
                name: "FK_Datos_Cultivos_CultivoId",
                table: "Datos",
                column: "CultivoId",
                principalTable: "Cultivos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enfermedades_Cultivos_CultivoId",
                table: "Enfermedades",
                column: "CultivoId",
                principalTable: "Cultivos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Datos_Cultivos_CultivoId",
                table: "Datos");

            migrationBuilder.DropForeignKey(
                name: "FK_Enfermedades_Cultivos_CultivoId",
                table: "Enfermedades");

            migrationBuilder.DropColumn(
                name: "NombreImagen",
                table: "Enfermedades");

            migrationBuilder.RenameColumn(
                name: "CultivoId",
                table: "Enfermedades",
                newName: "SemilleroId");

            migrationBuilder.RenameIndex(
                name: "IX_Enfermedades_CultivoId",
                table: "Enfermedades",
                newName: "IX_Enfermedades_SemilleroId");

            migrationBuilder.RenameColumn(
                name: "CultivoId",
                table: "Datos",
                newName: "SemilleroId");

            migrationBuilder.RenameIndex(
                name: "IX_Datos_CultivoId",
                table: "Datos",
                newName: "IX_Datos_SemilleroId");

            migrationBuilder.AddColumn<string>(
                name: "Rol",
                table: "Usuarios",
                type: "longtext",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "EnfermedadesExternas",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Cultivos",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AddForeignKey(
                name: "FK_Datos_Cultivos_SemilleroId",
                table: "Datos",
                column: "SemilleroId",
                principalTable: "Cultivos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enfermedades_Cultivos_SemilleroId",
                table: "Enfermedades",
                column: "SemilleroId",
                principalTable: "Cultivos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
