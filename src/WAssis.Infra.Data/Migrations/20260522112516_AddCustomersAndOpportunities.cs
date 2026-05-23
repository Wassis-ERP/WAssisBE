using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WAssis.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomersAndOpportunities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "segurados",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    filial_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    tipo = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    cpf_cnpj = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    telefone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    data_nascimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    nome_fantasia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sexo = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    estado_civil = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    porte = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    cnae = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    site = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    cep = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: true),
                    logradouro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    numero = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    complemento = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    bairro = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    cidade = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    estado = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    produtor_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    gerente_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    chatwoot_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    lgpd_autorizado = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_segurados", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "oportunidades",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    filial_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    responsavel_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    segurado_id = table.Column<Guid>(type: "uuid", nullable: true),
                    pipeline_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    stage_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    ramo_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    seguradora_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    origem_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    motivo_perda_id = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    status = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    tipo_negocio = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    tipo_contato = table.Column<bool>(type: "boolean", nullable: true),
                    premio_liquido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    comissao_percentual = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    agenciamento = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    producao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    vigencia_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    vigencia_fim = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    proximo_followup = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    concluded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    indicador = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    observacoes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    metadata = table.Column<string>(type: "jsonb", nullable: false, defaultValue: "{}"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_oportunidades", x => x.id);
                    table.ForeignKey(
                        name: "FK_oportunidades_segurados_segurado_id",
                        column: x => x.segurado_id,
                        principalSchema: "public",
                        principalTable: "segurados",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_oportunidades_segurado_id",
                schema: "public",
                table: "oportunidades",
                column: "segurado_id");

            migrationBuilder.CreateIndex(
                name: "IX_oportunidades_tenant_id_filial_id",
                schema: "public",
                table: "oportunidades",
                columns: new[] { "tenant_id", "filial_id" });

            migrationBuilder.CreateIndex(
                name: "IX_oportunidades_tenant_id_pipeline_id_stage_id",
                schema: "public",
                table: "oportunidades",
                columns: new[] { "tenant_id", "pipeline_id", "stage_id" });

            migrationBuilder.CreateIndex(
                name: "IX_oportunidades_tenant_id_status",
                schema: "public",
                table: "oportunidades",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_segurados_tenant_id_cpf_cnpj",
                schema: "public",
                table: "segurados",
                columns: new[] { "tenant_id", "cpf_cnpj" });

            migrationBuilder.CreateIndex(
                name: "IX_segurados_tenant_id_filial_id",
                schema: "public",
                table: "segurados",
                columns: new[] { "tenant_id", "filial_id" });

            migrationBuilder.CreateIndex(
                name: "IX_segurados_tenant_id_nome",
                schema: "public",
                table: "segurados",
                columns: new[] { "tenant_id", "nome" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "oportunidades",
                schema: "public");

            migrationBuilder.DropTable(
                name: "segurados",
                schema: "public");
        }
    }
}
