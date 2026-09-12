using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WAssis.Domain.Modules.Customers.Entities;

namespace WAssis.Infra.Data.Modules.Customers.Mappings;

public sealed class InsuredPersonMap : IEntityTypeConfiguration<InsuredPerson>
{
    public void Configure(EntityTypeBuilder<InsuredPerson> builder)
    {
        builder.ToTable("segurados", "public");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(64).IsRequired();
        builder.Property(x => x.OfficeBranchId).HasColumnName("filial_id").HasMaxLength(64);
        builder.Property(x => x.Name).HasColumnName("nome").HasMaxLength(200).IsRequired();
        builder.Property(x => x.SocialName).HasColumnName("nome_social").HasMaxLength(200);
        builder.Property(x => x.PersonType).HasColumnName("tipo").HasMaxLength(8).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasMaxLength(32).IsRequired();
        builder.Property(x => x.DocumentNumber).HasColumnName("cpf_cnpj").HasMaxLength(32);
        builder.Property(x => x.IdentityDocument).HasColumnName("rg_ie").HasMaxLength(32);
        builder.Property(x => x.MunicipalRegistration).HasColumnName("inscricao_municipal").HasMaxLength(64);
        builder.Property(x => x.Email).HasColumnName("email").HasMaxLength(200);
        builder.Property(x => x.PhoneNumber).HasColumnName("telefone").HasMaxLength(32);
        builder.Property(x => x.MobilePhoneNumber).HasColumnName("celular").HasMaxLength(32);
        builder.Property(x => x.SecondaryPhoneNumber).HasColumnName("telefone2").HasMaxLength(32);
        builder.Property(x => x.WhatsAppNumber).HasColumnName("whatsapp").HasMaxLength(32);
        builder.Property(x => x.BirthDateUtc).HasColumnName("data_nascimento");
        builder.Property(x => x.TradeName).HasColumnName("nome_fantasia").HasMaxLength(200);
        builder.Property(x => x.Gender).HasColumnName("sexo").HasMaxLength(16);
        builder.Property(x => x.MaritalStatus).HasColumnName("estado_civil").HasMaxLength(32);
        builder.Property(x => x.CompanySize).HasColumnName("porte").HasMaxLength(32);
        builder.Property(x => x.Cnae).HasColumnName("cnae").HasMaxLength(32);
        builder.Property(x => x.EconomicActivity).HasColumnName("atividade_economica").HasMaxLength(200);
        builder.Property(x => x.Profession).HasColumnName("profissao").HasMaxLength(120);
        builder.Property(x => x.MonthlyIncome).HasColumnName("renda_mensal").HasPrecision(18, 2);
        builder.Property(x => x.DriverLicenseNumber).HasColumnName("cnh_numero").HasMaxLength(32);
        builder.Property(x => x.DriverLicenseCategory).HasColumnName("cnh_categoria").HasMaxLength(16);
        builder.Property(x => x.DriverLicenseExpirationDate).HasColumnName("cnh_vencimento").HasColumnType("date");
        builder.Property(x => x.Website).HasColumnName("site").HasMaxLength(200);
        builder.Property(x => x.PostalCode).HasColumnName("cep").HasMaxLength(16);
        builder.Property(x => x.Street).HasColumnName("logradouro").HasMaxLength(200);
        builder.Property(x => x.Number).HasColumnName("numero").HasMaxLength(32);
        builder.Property(x => x.Complement).HasColumnName("complemento").HasMaxLength(120);
        builder.Property(x => x.Neighborhood).HasColumnName("bairro").HasMaxLength(120);
        builder.Property(x => x.City).HasColumnName("cidade").HasMaxLength(120);
        builder.Property(x => x.State).HasColumnName("estado").HasMaxLength(2);
        builder.Property(x => x.Country).HasColumnName("pais").HasMaxLength(100);
        builder.Property(x => x.Notes).HasColumnName("observacoes").HasMaxLength(2000);
        builder.Property(x => x.ProducerId).HasColumnName("produtor_id").HasMaxLength(64);
        builder.Property(x => x.ManagerId).HasColumnName("gerente_id").HasMaxLength(64);
        builder.Property(x => x.ChatwootId).HasColumnName("chatwoot_id").HasMaxLength(64);
        builder.Property(x => x.LgpdAuthorized).HasColumnName("lgpd_autorizado").IsRequired();
        builder.Property(x => x.LgpdAuthorizedAtUtc).HasColumnName("lgpd_autorizado_em");
        builder.Property(x => x.ImportOrigin).HasColumnName("origem_importacao").HasMaxLength(120);
        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(64);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at").IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasColumnName("updated_at").IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.Name });
        builder.HasIndex(x => new { x.TenantId, x.DocumentNumber });
        builder.HasIndex(x => new { x.TenantId, x.OfficeBranchId });
    }
}
