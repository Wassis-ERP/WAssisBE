using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Customers.Entities;
using WAssis.Domain.Modules.Opportunities.Entities;

namespace WAssis.Infra.Data.Modules.Opportunities.Mappings;

public sealed class OpportunityMap : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("oportunidades", "public");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(64).IsRequired();
        builder.Property(x => x.OfficeBranchId).HasColumnName("filial_id").HasMaxLength(64);
        builder.Property(x => x.Name).HasColumnName("nome").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ResponsibleId).HasColumnName("responsavel_id").HasMaxLength(64).IsRequired();
        builder.Property(x => x.InsuredPersonId).HasColumnName("segurado_id");
        builder.Property(x => x.PipelineId).HasColumnName("pipeline_id").HasMaxLength(64);
        builder.Property(x => x.StageId).HasColumnName("stage_id").HasMaxLength(64);
        builder.Property(x => x.InsuranceLineId).HasColumnName("ramo_id").HasMaxLength(64);
        builder.Property(x => x.InsurerId).HasColumnName("seguradora_id").HasMaxLength(64);
        builder.Property(x => x.OriginId).HasColumnName("origem_id").HasMaxLength(64);
        builder.Property(x => x.LossReasonId).HasColumnName("motivo_perda_id").HasMaxLength(64);
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
        builder.Property(x => x.BusinessType).HasColumnName("tipo_negocio").HasMaxLength(32);
        builder.Property(x => x.ContactType).HasColumnName("tipo_contato");
        builder.Property(x => x.NetPremium).HasColumnName("premio_liquido").HasPrecision(18, 2);
        builder.Property(x => x.CommissionPercentage).HasColumnName("comissao_percentual").HasPrecision(9, 4);
        builder.Property(x => x.AgencyPercentage).HasColumnName("agenciamento").HasPrecision(9, 4);
        builder.Property(x => x.ProductionAmount).HasColumnName("producao").HasPrecision(18, 2);
        builder.Property(x => x.ValidityStartUtc).HasColumnName("vigencia_inicio");
        builder.Property(x => x.ValidityEndUtc).HasColumnName("vigencia_fim");
        builder.Property(x => x.NextFollowUpUtc).HasColumnName("proximo_followup");
        builder.Property(x => x.ConcludedAtUtc).HasColumnName("concluded_at");
        builder.Property(x => x.Referrer).HasColumnName("indicador").HasMaxLength(200);
        builder.Property(x => x.Notes).HasColumnName("observacoes").HasMaxLength(2000);
        builder.Property(x => x.MetadataJson).HasColumnName("metadata").HasColumnType("jsonb").HasDefaultValue("{}").IsRequired();
        builder.Property(x => x.OriginPolicyId).HasColumnName("apolice_origem_id");
        builder.Property(x => x.LeadName).HasColumnName("lead_nome").HasMaxLength(200);
        builder.Property(x => x.LeadDocumentNumber).HasColumnName("lead_documento").HasMaxLength(32);
        builder.Property(x => x.LeadEmail).HasColumnName("lead_email").HasMaxLength(200);
        builder.Property(x => x.LeadPhoneNumber).HasColumnName("lead_telefone").HasMaxLength(32);
        builder.Property(x => x.Title).HasColumnName("titulo").HasMaxLength(200);
        builder.Property(x => x.Description).HasColumnName("descricao").HasMaxLength(2000);
        builder.Property(x => x.Priority).HasColumnName("prioridade").HasMaxLength(32);
        builder.Property(x => x.EstimatedPremiumAmount).HasColumnName("valor_premio_estimado").HasPrecision(18, 2);
        builder.Property(x => x.EstimatedCommissionAmount).HasColumnName("valor_comissao_estimada").HasPrecision(18, 2);
        builder.Property(x => x.EstimatedCommissionPercentage).HasColumnName("comissao_estimada_pct").HasPrecision(9, 4);
        builder.Property(x => x.OpenedOn).HasColumnName("data_abertura").HasColumnType("date");
        builder.Property(x => x.ExpectedCloseDate).HasColumnName("data_fechamento_prevista").HasColumnType("date");
        builder.Property(x => x.WonAtUtc).HasColumnName("ganha_em");
        builder.Property(x => x.LostAtUtc).HasColumnName("perdida_em");
        builder.Property(x => x.LossReasonNotes).HasColumnName("motivo_perda_observacao").HasMaxLength(2000);
        builder.Property(x => x.Campaign).HasColumnName("campanha").HasMaxLength(200);
        builder.Property(x => x.InternalNotes).HasColumnName("observacoes_internas").HasMaxLength(2000);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.OfficeBranchId });
        builder.HasIndex(x => new { x.TenantId, x.PipelineId, x.StageId });
        builder.HasIndex(x => x.InsuredPersonId);

        builder.HasOne<InsuredPerson>()
            .WithMany()
            .HasForeignKey(x => x.InsuredPersonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
