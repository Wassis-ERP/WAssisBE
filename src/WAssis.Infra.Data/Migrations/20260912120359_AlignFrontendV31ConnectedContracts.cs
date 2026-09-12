using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WAssis.Infra.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlignFrontendV31ConnectedContracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "atividade_economica",
                schema: "public",
                table: "segurados",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "celular",
                schema: "public",
                table: "segurados",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cnh_categoria",
                schema: "public",
                table: "segurados",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cnh_numero",
                schema: "public",
                table: "segurados",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "cnh_vencimento",
                schema: "public",
                table: "segurados",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "inscricao_municipal",
                schema: "public",
                table: "segurados",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "lgpd_autorizado_em",
                schema: "public",
                table: "segurados",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nome_social",
                schema: "public",
                table: "segurados",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "origem_importacao",
                schema: "public",
                table: "segurados",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pais",
                schema: "public",
                table: "segurados",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profissao",
                schema: "public",
                table: "segurados",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "renda_mensal",
                schema: "public",
                table: "segurados",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "rg_ie",
                schema: "public",
                table: "segurados",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telefone2",
                schema: "public",
                table: "segurados",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "whatsapp",
                schema: "public",
                table: "segurados",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "apolice_origem_id",
                schema: "public",
                table: "oportunidades",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "campanha",
                schema: "public",
                table: "oportunidades",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "comissao_estimada_pct",
                schema: "public",
                table: "oportunidades",
                type: "numeric(9,4)",
                precision: 9,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "data_abertura",
                schema: "public",
                table: "oportunidades",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "data_fechamento_prevista",
                schema: "public",
                table: "oportunidades",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "descricao",
                schema: "public",
                table: "oportunidades",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ganha_em",
                schema: "public",
                table: "oportunidades",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lead_documento",
                schema: "public",
                table: "oportunidades",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lead_email",
                schema: "public",
                table: "oportunidades",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lead_nome",
                schema: "public",
                table: "oportunidades",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "lead_telefone",
                schema: "public",
                table: "oportunidades",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_perda_observacao",
                schema: "public",
                table: "oportunidades",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "observacoes_internas",
                schema: "public",
                table: "oportunidades",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "perdida_em",
                schema: "public",
                table: "oportunidades",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "prioridade",
                schema: "public",
                table: "oportunidades",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "titulo",
                schema: "public",
                table: "oportunidades",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_comissao_estimada",
                schema: "public",
                table: "oportunidades",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "valor_premio_estimado",
                schema: "public",
                table: "oportunidades",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "atividade_economica",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "celular",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "cnh_categoria",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "cnh_numero",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "cnh_vencimento",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "inscricao_municipal",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "lgpd_autorizado_em",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "nome_social",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "origem_importacao",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "pais",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "profissao",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "renda_mensal",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "rg_ie",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "telefone2",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "whatsapp",
                schema: "public",
                table: "segurados");

            migrationBuilder.DropColumn(
                name: "apolice_origem_id",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "campanha",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "comissao_estimada_pct",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "data_abertura",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "data_fechamento_prevista",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "descricao",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "ganha_em",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "lead_documento",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "lead_email",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "lead_nome",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "lead_telefone",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "motivo_perda_observacao",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "observacoes_internas",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "perdida_em",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "prioridade",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "titulo",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "valor_comissao_estimada",
                schema: "public",
                table: "oportunidades");

            migrationBuilder.DropColumn(
                name: "valor_premio_estimado",
                schema: "public",
                table: "oportunidades");
        }
    }
}
