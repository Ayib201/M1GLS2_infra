using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace M1GLS2_infra.Migrations
{
    /// <inheritdoc />
    public partial class add_metier_classe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "projets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nom = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projets_utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "taches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Titre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Statut = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProjetId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_taches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_taches_projets_ProjetId",
                        column: x => x.ProjetId,
                        principalTable: "projets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "commentaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Contenu = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DateCreation = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TacheId = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commentaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_commentaires_taches_TacheId",
                        column: x => x.TacheId,
                        principalTable: "taches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_commentaires_utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_commentaires_TacheId",
                table: "commentaires",
                column: "TacheId");

            migrationBuilder.CreateIndex(
                name: "IX_commentaires_UtilisateurId",
                table: "commentaires",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_projets_UtilisateurId",
                table: "projets",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_taches_ProjetId",
                table: "taches",
                column: "ProjetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "commentaires");

            migrationBuilder.DropTable(
                name: "taches");

            migrationBuilder.DropTable(
                name: "projets");
        }
    }
}
