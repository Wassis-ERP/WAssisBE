using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Customers.Entities;
using WAssis.Domain.Modules.Opportunities.Entities;
using WAssis.Infra.Data.Modules.Core;

namespace WAssis.Infra.Data.Modules.Opportunities.Mappings;

public sealed class OpportunityMap : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("oportunidades", "erp");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasConversion(CanonicalValueConverters.Uuid).IsRequired();
        builder.Property(x => x.OfficeBranchId).HasColumnName("filial_id").HasConversion(CanonicalValueConverters.NullableUuid).IsRequired();
        builder.Property(x => x.ResponsibleId).HasColumnName("responsavel_id").HasConversion(CanonicalValueConverters.Uuid).IsRequired();
        builder.Property(x => x.InsuredPersonId).HasColumnName("segurado_id");
        builder.Property(x => x.StageId).HasColumnName("stage_id").HasConversion(CanonicalValueConverters.NullableUuid).IsRequired();
        builder.Property(x => x.InsuranceLineId).HasColumnName("ramo_id").HasConversion(CanonicalValueConverters.NullableUuid);
        builder.Property(x => x.OriginId).HasColumnName("origem_id").HasConversion(CanonicalValueConverters.NullableUuid);
        builder.Property(x => x.LossReasonId).HasColumnName("motivo_perda_id").HasConversion(CanonicalValueConverters.NullableUuid);
        builder.Property(x => x.OriginPolicyId).HasColumnName("apolice_origem_id");
        builder.Property(x => x.LeadName).HasColumnName("lead_nome");
        builder.Property(x => x.LeadDocumentNumber).HasColumnName("lead_documento");
        builder.Property(x => x.LeadEmail).HasColumnName("lead_email");
        builder.Property(x => x.LeadPhoneNumber).HasColumnName("lead_telefone");
        builder.Property(x => x.Title).HasColumnName("titulo");
        builder.Property(x => x.Description).HasColumnName("descricao");
        builder.Property(x => x.Priority).HasColumnName("prioridade");
        builder.Property(x => x.EstimatedPremiumAmount).HasColumnName("valor_premio_estimado");
        builder.Property(x => x.EstimatedCommissionAmount).HasColumnName("valor_comissao_estimada");
        builder.Property(x => x.EstimatedCommissionPercentage).HasColumnName("comissao_estimada_pct");
        builder.Property(x => x.AgencyPercentage).HasColumnName("agenciamento_pct");
        builder.Property(x => x.OpenedOn).HasColumnName("data_abertura");
        builder.Property(x => x.ExpectedCloseDate).HasColumnName("data_fechamento_prevista");
        builder.Property(x => x.WonAtUtc).HasColumnName("ganha_em");
        builder.Property(x => x.LostAtUtc).HasColumnName("perdida_em");
        builder.Property(x => x.LossReasonNotes).HasColumnName("motivo_perda_observacao");
        builder.Property(x => x.Campaign).HasColumnName("campanha");
        builder.Property(x => x.InternalNotes).HasColumnName("observacoes");
        // Operational metadata, not additional business facts.
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at");

        builder.Ignore(x => x.Name);
        builder.Ignore(x => x.PipelineId);
        builder.Ignore(x => x.Status);
        builder.Ignore(x => x.NetPremium);
        builder.Ignore(x => x.CommissionPercentage);
        builder.Ignore(x => x.ConcludedAtUtc);
        builder.Ignore(x => x.Notes);
        builder.Ignore(x => x.MetadataJson);
        builder.Ignore(x => x.InsurerId);
        builder.Ignore(x => x.BusinessType);
        builder.Ignore(x => x.ContactType);
        builder.Ignore(x => x.ProductionAmount);
        builder.Ignore(x => x.ValidityStartUtc);
        builder.Ignore(x => x.ValidityEndUtc);
        builder.Ignore(x => x.NextFollowUpUtc);
        builder.Ignore(x => x.Referrer);

        builder.HasIndex(x => new { x.TenantId, x.OfficeBranchId });
        builder.HasOne<InsuredPerson>().WithMany().HasForeignKey(x => x.InsuredPersonId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Stage).WithMany().HasForeignKey(x => x.StageId).OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(x => x.Stage).AutoInclude();
    }
}

public sealed class OpportunityStageReferenceMap : IEntityTypeConfiguration<OpportunityStageReference>
{
    public void Configure(EntityTypeBuilder<OpportunityStageReference> builder)
    {
        builder.ToTable("pipeline_stages", "erp", table => table.ExcludeFromMigrations());
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").HasConversion(CanonicalValueConverters.Uuid);
        builder.Property(x => x.PipelineId).HasColumnName("pipeline_id").HasConversion(CanonicalValueConverters.Uuid);
    }
}
