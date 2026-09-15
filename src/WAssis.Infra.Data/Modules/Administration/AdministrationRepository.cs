using System.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using WAssis.Application.Modules.Administration.Dtos;
using WAssis.Application.Modules.Administration.Interfaces;
using WAssis.Infra.Data.Context;

namespace WAssis.Infra.Data.Modules.Administration;

public sealed class AdministrationRepository(WAssisDbContext dbContext) : IAdministrationRepository
{
    private async Task<IDbConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync(cancellationToken);
        return connection;
    }

    public async Task<bool> HasAdministrationPermissionAsync(Guid tenantId, Guid userId, bool manage, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT EXISTS (
              SELECT 1 FROM erp.profiles p
              JOIN erp.profile_filiais pf ON pf.profile_id = p.id AND pf.ativo
              JOIN erp.filiais f ON f.id = pf.filial_id AND f.tenant_id = p.tenant_id AND f.ativo
              JOIN erp.perfis pe ON pe.id = pf.perfil_id AND pe.tenant_id = p.tenant_id AND pe.ativo
              JOIN erp.role_permissions rp ON rp.perfil_id = pe.id
              WHERE p.id = @UserId AND p.tenant_id = @TenantId AND p.ativo AND p.status = 'ATIVO'
                AND (p.convite_status IS NULL OR p.convite_status = 'ACEITO')
                AND (pf.data_inicio IS NULL OR pf.data_inicio <= current_date)
                AND (pf.data_fim IS NULL OR pf.data_fim >= current_date)
                AND rp.modulo = 'configuracoes' AND rp.escopo = 'GRUPO'
                AND ((@Manage AND rp.can_manage) OR (NOT @Manage AND rp.can_read))
            )
            """;
        return await (await OpenAsync(cancellationToken)).ExecuteScalarAsync<bool>(new CommandDefinition(
            sql, new { TenantId = tenantId, UserId = userId, Manage = manage }, cancellationToken: cancellationToken));
    }

    public async Task<OrganizationDto?> GetOrganizationAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS Id, razao_social AS LegalName, nome_fantasia AS TradeName,
                   cnpj_cpf AS DocumentNumber, email AS Email, telefone AS Phone,
                   celular AS Mobile, home_page AS Website, timezone AS TimeZone,
                   moeda_padrao AS DefaultCurrency, status AS Status,
                   COALESCE(ativo, false) AS IsActive
            FROM erp.tenants WHERE id = @TenantId
            """;
        return await (await OpenAsync(cancellationToken)).QuerySingleOrDefaultAsync<OrganizationDto>(
            new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken));
    }

    public async Task<OrganizationDto?> UpdateOrganizationAsync(Guid tenantId, OrganizationUpdateDto update, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE erp.tenants
               SET razao_social = @LegalName, nome_fantasia = @TradeName,
                   cnpj_cpf = @DocumentNumber, email = @Email, telefone = @Phone,
                   celular = @Mobile, home_page = @Website, timezone = @TimeZone,
                   moeda_padrao = @DefaultCurrency, atualizado_em = now()
             WHERE id = @TenantId
            RETURNING id AS Id, razao_social AS LegalName, nome_fantasia AS TradeName,
                      cnpj_cpf AS DocumentNumber, email AS Email, telefone AS Phone,
                      celular AS Mobile, home_page AS Website, timezone AS TimeZone,
                      moeda_padrao AS DefaultCurrency, status AS Status,
                      COALESCE(ativo, false) AS IsActive
            """;
        return await (await OpenAsync(cancellationToken)).QuerySingleOrDefaultAsync<OrganizationDto>(
            new CommandDefinition(sql, new
            {
                TenantId = tenantId,
                update.LegalName,
                update.TradeName,
                update.DocumentNumber,
                update.Email,
                update.Phone,
                update.Mobile,
                update.Website,
                update.TimeZone,
                update.DefaultCurrency,
            }, cancellationToken: cancellationToken));
    }

    public async Task<OrganizationStatisticsDto> GetStatisticsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
              (SELECT count(*)::int FROM erp.filiais WHERE tenant_id = @TenantId AND ativo) AS ActiveBranches,
              (SELECT count(*)::int FROM erp.profiles WHERE tenant_id = @TenantId AND ativo
                 AND status = 'ATIVO' AND convite_status IS DISTINCT FROM 'PENDENTE') AS ActiveUsers,
              (SELECT count(*)::int FROM erp.profiles WHERE tenant_id = @TenantId AND status = 'INATIVO') AS InactiveUsers,
              (SELECT count(*)::int FROM erp.profiles WHERE tenant_id = @TenantId AND convite_status = 'PENDENTE'
                 AND status <> 'INATIVO') AS PendingInvitations,
              (SELECT count(*)::int FROM public.segurados WHERE tenant_id = CAST(@TenantId AS text)) AS InsuredPeople,
              (SELECT count(*)::int FROM public.oportunidades WHERE tenant_id = CAST(@TenantId AS text) AND status = 'pending') AS OpenOpportunities,
              (SELECT count(*)::int FROM public.oportunidades WHERE tenant_id = CAST(@TenantId AS text) AND status = 'won') AS WonOpportunities,
              (SELECT count(*)::int FROM public.oportunidades WHERE tenant_id = CAST(@TenantId AS text) AND status = 'lost') AS LostOpportunities
            """;
        return await (await OpenAsync(cancellationToken)).QuerySingleAsync<OrganizationStatisticsDto>(
            new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<AdministrationBranchDto>> ListBranchesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS Id, tenant_id AS TenantId, matriz_id AS ParentBranchId,
                   razao_social AS LegalName, fantasia AS TradeName, cnpj_cpf AS DocumentNumber,
                   susep AS Susep, percentual_imposto AS TaxPercentage,
                   COALESCE(lgpd_aceito, false) AS LgpdAccepted, lgpd_aceito_em AS LgpdAcceptedAt,
                   gerente AS Manager, gerente_id AS ManagerId, contato AS Contact, home_page AS Website,
                   email AS Email, telefone AS Phone, celular AS Mobile, telefone2 AS SecondaryPhone,
                   inscricao_estadual AS StateRegistration, inscricao_municipal AS MunicipalRegistration,
                   regime_tributario AS TaxRegime, percentual_iss AS IssPercentage,
                   codigo_corretora AS BrokerageCode, codigo_externo AS ExternalCode,
                   municipio_ibge AS IbgeCityCode, pais AS Country, horario_atendimento AS BusinessHours,
                   observacoes AS Notes, cep AS PostalCode, endereco AS Address, numero AS Number,
                   complemento AS Complement, bairro AS District, cidade AS City, uf AS State,
                   COALESCE(ativo, false) AS IsActive
            FROM erp.filiais WHERE tenant_id = @TenantId
            ORDER BY ativo DESC, matriz_id NULLS FIRST, COALESCE(fantasia, razao_social), id
            """;
        var rows = await (await OpenAsync(cancellationToken)).QueryAsync<AdministrationBranchDto>(
            new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<AdministrationBranchDto> CreateBranchAsync(Guid tenantId, AdministrationBranchWriteDto create, CancellationToken cancellationToken) =>
        await WriteBranchAsync(tenantId, Guid.NewGuid(), create, true, cancellationToken)
        ?? throw new InvalidOperationException("Não foi possível criar a corretora.");

    public Task<AdministrationBranchDto?> UpdateBranchAsync(Guid tenantId, Guid branchId, AdministrationBranchWriteDto update, CancellationToken cancellationToken) =>
        WriteBranchAsync(tenantId, branchId, update, false, cancellationToken);

    private async Task<AdministrationBranchDto?> WriteBranchAsync(
        Guid tenantId, Guid branchId, AdministrationBranchWriteDto input, bool create, CancellationToken cancellationToken)
    {
        var connection = await OpenAsync(cancellationToken);
        if (input.ParentBranchId.HasValue)
        {
            const string parentScopeSql = """
                SELECT EXISTS (SELECT 1 FROM erp.filiais
                               WHERE id = @ParentBranchId AND tenant_id = @TenantId AND ativo
                                 AND matriz_id IS NULL)
                """;
            var validParent = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                parentScopeSql,
                new { input.ParentBranchId, TenantId = tenantId },
                cancellationToken: cancellationToken));
            if (!validParent) throw new InvalidOperationException("A matriz informada não pertence ao grupo ativo.");
        }
        if (input.ManagerId.HasValue)
        {
            var validManager = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                "SELECT EXISTS (SELECT 1 FROM erp.produtores WHERE id = @ManagerId AND tenant_id = @TenantId AND ativo)",
                new { input.ManagerId, TenantId = tenantId }, cancellationToken: cancellationToken));
            if (!validManager) throw new InvalidOperationException("O gerente informado não pertence ao grupo ativo.");
        }

        const string insert = """
            INSERT INTO erp.filiais
              (id, tenant_id, matriz_id, razao_social, fantasia, cnpj_cpf, susep, percentual_imposto,
               lgpd_aceito, lgpd_aceito_em, gerente, gerente_id, contato, home_page, email, telefone,
               celular, telefone2, inscricao_estadual, inscricao_municipal, regime_tributario,
               percentual_iss, codigo_corretora, codigo_externo, municipio_ibge, pais,
               horario_atendimento, observacoes, cep, endereco, numero, complemento, bairro, cidade, uf, ativo)
            VALUES
              (@BranchId, @TenantId, @ParentBranchId, @LegalName, @TradeName, @DocumentNumber, @Susep, @TaxPercentage,
               @LgpdAccepted, @LgpdAcceptedAt, @Manager, @ManagerId, @Contact, @Website, @Email, @Phone,
               @Mobile, @SecondaryPhone, @StateRegistration, @MunicipalRegistration, @TaxRegime,
               @IssPercentage, @BrokerageCode, @ExternalCode, @IbgeCityCode, @Country,
               @BusinessHours, @Notes, @PostalCode, @Address, @Number, @Complement, @District, @City, @State, @IsActive)
            RETURNING id AS Id, tenant_id AS TenantId, matriz_id AS ParentBranchId,
              razao_social AS LegalName, fantasia AS TradeName, cnpj_cpf AS DocumentNumber, susep AS Susep,
              percentual_imposto AS TaxPercentage, lgpd_aceito AS LgpdAccepted, lgpd_aceito_em AS LgpdAcceptedAt,
              gerente AS Manager, gerente_id AS ManagerId, contato AS Contact, home_page AS Website,
              email AS Email, telefone AS Phone, celular AS Mobile, telefone2 AS SecondaryPhone,
              inscricao_estadual AS StateRegistration, inscricao_municipal AS MunicipalRegistration,
              regime_tributario AS TaxRegime, percentual_iss AS IssPercentage, codigo_corretora AS BrokerageCode,
              codigo_externo AS ExternalCode, municipio_ibge AS IbgeCityCode, pais AS Country,
              horario_atendimento AS BusinessHours, observacoes AS Notes, cep AS PostalCode, endereco AS Address,
              numero AS Number, complemento AS Complement, bairro AS District, cidade AS City, uf AS State, ativo AS IsActive
            """;
        const string update = """
            UPDATE erp.filiais SET matriz_id = @ParentBranchId, razao_social = @LegalName,
              fantasia = @TradeName, cnpj_cpf = @DocumentNumber, susep = @Susep,
              percentual_imposto = @TaxPercentage, lgpd_aceito = @LgpdAccepted,
              lgpd_aceito_em = @LgpdAcceptedAt, gerente = @Manager, gerente_id = @ManagerId,
              contato = @Contact, home_page = @Website, email = @Email, telefone = @Phone,
              celular = @Mobile, telefone2 = @SecondaryPhone, inscricao_estadual = @StateRegistration,
              inscricao_municipal = @MunicipalRegistration, regime_tributario = @TaxRegime,
              percentual_iss = @IssPercentage, codigo_corretora = @BrokerageCode,
              codigo_externo = @ExternalCode, municipio_ibge = @IbgeCityCode, pais = @Country,
              horario_atendimento = @BusinessHours, observacoes = @Notes, cep = @PostalCode,
              endereco = @Address, numero = @Number, complemento = @Complement, bairro = @District,
              cidade = @City, uf = @State, ativo = @IsActive
            WHERE id = @BranchId AND tenant_id = @TenantId
            RETURNING id AS Id, tenant_id AS TenantId, matriz_id AS ParentBranchId,
              razao_social AS LegalName, fantasia AS TradeName, cnpj_cpf AS DocumentNumber, susep AS Susep,
              percentual_imposto AS TaxPercentage, lgpd_aceito AS LgpdAccepted, lgpd_aceito_em AS LgpdAcceptedAt,
              gerente AS Manager, gerente_id AS ManagerId, contato AS Contact, home_page AS Website,
              email AS Email, telefone AS Phone, celular AS Mobile, telefone2 AS SecondaryPhone,
              inscricao_estadual AS StateRegistration, inscricao_municipal AS MunicipalRegistration,
              regime_tributario AS TaxRegime, percentual_iss AS IssPercentage, codigo_corretora AS BrokerageCode,
              codigo_externo AS ExternalCode, municipio_ibge AS IbgeCityCode, pais AS Country,
              horario_atendimento AS BusinessHours, observacoes AS Notes, cep AS PostalCode, endereco AS Address,
              numero AS Number, complemento AS Complement, bairro AS District, cidade AS City, uf AS State, ativo AS IsActive
            """;
        var parameters = new
        {
            TenantId = tenantId,
            BranchId = branchId,
            input.ParentBranchId,
            input.LegalName,
            input.TradeName,
            input.DocumentNumber,
            input.Susep,
            input.TaxPercentage,
            input.LgpdAccepted,
            input.LgpdAcceptedAt,
            input.Manager,
            input.ManagerId,
            input.Contact,
            input.Website,
            input.Email,
            input.Phone,
            input.Mobile,
            input.SecondaryPhone,
            input.StateRegistration,
            input.MunicipalRegistration,
            input.TaxRegime,
            input.IssPercentage,
            input.BrokerageCode,
            input.ExternalCode,
            input.IbgeCityCode,
            input.Country,
            input.BusinessHours,
            input.Notes,
            input.PostalCode,
            input.Address,
            input.Number,
            input.Complement,
            input.District,
            input.City,
            input.State,
            input.IsActive,
        };
        return await connection.QuerySingleOrDefaultAsync<AdministrationBranchDto>(
            new CommandDefinition(create ? insert : update, parameters, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<AdministrationUserDto>> ListUsersAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT p.id AS Id, COALESCE(p.nome_completo, '') AS Name, COALESCE(p.email, '') AS Email,
                   p.avatar_url AS AvatarUrl, COALESCE(p.status, CASE WHEN p.ativo THEN 'ATIVO' ELSE 'INATIVO' END) AS Status,
                   COALESCE(p.ativo, false) AS IsActive, p.convite_status AS InvitationStatus,
                   p.convite_enviado_em AS InvitationSentAt, p.ultimo_acesso_em AS LastAccessAt,
                   count(pf.id) FILTER (WHERE pf.ativo AND f.ativo AND pe.ativo
                     AND (pf.data_inicio IS NULL OR pf.data_inicio <= current_date)
                     AND (pf.data_fim IS NULL OR pf.data_fim >= current_date))::int AS BranchCount,
                   max(pe.nome) FILTER (WHERE pf.principal AND pf.ativo AND pe.ativo
                     AND (pf.data_inicio IS NULL OR pf.data_inicio <= current_date)
                     AND (pf.data_fim IS NULL OR pf.data_fim >= current_date)) AS PrimaryAccessProfile
            FROM erp.profiles p
            LEFT JOIN erp.profile_filiais pf ON pf.profile_id = p.id
            LEFT JOIN erp.filiais f ON f.id = pf.filial_id AND f.tenant_id = p.tenant_id
            LEFT JOIN erp.perfis pe ON pe.id = pf.perfil_id AND pe.tenant_id = p.tenant_id
            WHERE p.tenant_id = @TenantId
            GROUP BY p.id
            ORDER BY p.nome_completo NULLS LAST, p.email
            """;
        var rows = await (await OpenAsync(cancellationToken)).QueryAsync<AdministrationUserDto>(
            new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<AdministrationUserDto> InviteUserAsync(Guid tenantId, UserInvitationDto invitation, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO erp.profiles
                (id, tenant_id, nome_completo, email, status, ativo, convite_status, convite_enviado_em)
            VALUES
                (@Id, @TenantId, @Name, lower(@Email), 'PENDENTE', false, 'PENDENTE', NULL)
            RETURNING id AS Id, nome_completo AS Name, email AS Email, avatar_url AS AvatarUrl,
                      status AS Status, ativo AS IsActive, convite_status AS InvitationStatus,
                      convite_enviado_em AS InvitationSentAt, ultimo_acesso_em AS LastAccessAt,
                      0 AS BranchCount, NULL::text AS PrimaryAccessProfile
            """;
        return await (await OpenAsync(cancellationToken)).QuerySingleAsync<AdministrationUserDto>(
            new CommandDefinition(sql, new
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = invitation.Name.Trim(),
                Email = invitation.Email.Trim(),
            }, cancellationToken: cancellationToken));
    }

    public async Task<bool> SetUserStatusAsync(Guid tenantId, Guid userId, bool isActive, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (!isActive && userId == actorUserId)
            throw new InvalidOperationException("O administrador não pode inativar o próprio acesso.");

        var connection = await OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
        await LockTenantAdministrationAsync(connection, transaction, tenantId, cancellationToken);
        if (isActive && await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                "SELECT EXISTS (SELECT 1 FROM erp.profiles WHERE id = @UserId AND tenant_id = @TenantId AND convite_status = 'PENDENTE')",
                new { TenantId = tenantId, UserId = userId }, transaction, cancellationToken: cancellationToken)))
            throw new InvalidOperationException("O convite ainda não foi aceito no provedor de identidade.");
        if (!isActive)
        {
            const string lastMasterSql = """
                SELECT EXISTS (
                  SELECT 1 FROM erp.profile_filiais pf
                  JOIN erp.profiles p ON p.id = pf.profile_id
                  JOIN erp.perfis pe ON pe.id = pf.perfil_id
                  JOIN erp.filiais f ON f.id = pf.filial_id AND f.tenant_id = p.tenant_id AND f.ativo
                  WHERE p.id = @UserId AND p.tenant_id = @TenantId AND p.ativo AND pf.ativo AND pe.ativo
                    AND (pf.data_inicio IS NULL OR pf.data_inicio <= current_date)
                    AND (pf.data_fim IS NULL OR pf.data_fim >= current_date)
                    AND pe.sistema AND lower(pe.nome) = 'master'
                ) AND (
                  SELECT count(DISTINCT p.id) FROM erp.profiles p
                  JOIN erp.profile_filiais pf ON pf.profile_id = p.id AND pf.ativo
                  JOIN erp.perfis pe ON pe.id = pf.perfil_id AND pe.ativo
                  JOIN erp.filiais f ON f.id = pf.filial_id AND f.tenant_id = p.tenant_id AND f.ativo
                  WHERE p.tenant_id = @TenantId AND p.ativo AND p.status = 'ATIVO'
                    AND (pf.data_inicio IS NULL OR pf.data_inicio <= current_date)
                    AND (pf.data_fim IS NULL OR pf.data_fim >= current_date)
                    AND pe.sistema AND lower(pe.nome) = 'master'
                ) <= 1
                """;
            if (await connection.ExecuteScalarAsync<bool>(new CommandDefinition(lastMasterSql,
                    new { TenantId = tenantId, UserId = userId }, transaction, cancellationToken: cancellationToken)))
                throw new InvalidOperationException("O último usuário Master ativo não pode ser inativado.");
        }

        const string sql = """
            UPDATE erp.profiles SET ativo = @IsActive, status = CASE WHEN @IsActive THEN 'ATIVO' ELSE 'INATIVO' END
             WHERE id = @UserId AND tenant_id = @TenantId
            """;
        var changed = await connection.ExecuteAsync(new CommandDefinition(sql,
            new { TenantId = tenantId, UserId = userId, IsActive = isActive }, transaction, cancellationToken: cancellationToken));
        transaction.Commit();
        return changed == 1;
    }

    public async Task<IReadOnlyCollection<UserBranchAccessDto>> ListUserBranchAccessAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT pf.id AS Id, pf.profile_id AS UserId, pf.filial_id AS BranchId,
                   pf.perfil_id AS AccessProfileId, COALESCE(pf.principal, false) AS IsPrimary,
                   COALESCE(pf.ativo, false) AS IsActive, pf.data_inicio AS StartsOn, pf.data_fim AS EndsOn
            FROM erp.profile_filiais pf
            JOIN erp.profiles p ON p.id = pf.profile_id
            WHERE p.tenant_id = @TenantId AND p.id = @UserId
            ORDER BY pf.principal DESC, pf.filial_id
            """;
        var rows = await (await OpenAsync(cancellationToken)).QueryAsync<UserBranchAccessDto>(
            new CommandDefinition(sql, new { TenantId = tenantId, UserId = userId }, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<UserBranchAccessDto?> UpsertUserBranchAccessAsync(Guid tenantId, Guid userId, Guid branchId, UserBranchAccessUpdateDto update, CancellationToken cancellationToken)
    {
        if (update.StartsOn.HasValue && update.EndsOn.HasValue && update.StartsOn > update.EndsOn)
            throw new InvalidOperationException("A data inicial do acesso não pode ser posterior à data final.");

        var connection = await OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
        await LockTenantAdministrationAsync(connection, transaction, tenantId, cancellationToken);
        const string scopeSql = """
            SELECT EXISTS (SELECT 1 FROM erp.profiles WHERE id = @UserId AND tenant_id = @TenantId)
               AND EXISTS (SELECT 1 FROM erp.filiais WHERE id = @BranchId AND tenant_id = @TenantId)
               AND EXISTS (SELECT 1 FROM erp.perfis WHERE id = @AccessProfileId AND tenant_id = @TenantId AND ativo)
            """;
        var scoped = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(scopeSql, new
        {
            TenantId = tenantId,
            UserId = userId,
            BranchId = branchId,
            update.AccessProfileId,
        }, transaction, cancellationToken: cancellationToken));
        if (!scoped) return null;

        if (update.IsPrimary && (!update.IsActive || update.StartsOn > DateOnly.FromDateTime(DateTime.UtcNow)
            || update.EndsOn < DateOnly.FromDateTime(DateTime.UtcNow)))
            throw new InvalidOperationException("A corretora principal deve ter vínculo ativo e vigente.");

        const string activeMasterSql = """
            SELECT count(DISTINCT p.id)::int FROM erp.profiles p
            JOIN erp.profile_filiais pf ON pf.profile_id = p.id AND pf.ativo
            JOIN erp.perfis pe ON pe.id = pf.perfil_id AND pe.ativo
            JOIN erp.filiais f ON f.id = pf.filial_id AND f.tenant_id = p.tenant_id AND f.ativo
            WHERE p.tenant_id = @TenantId AND p.ativo AND p.status = 'ATIVO'
              AND pe.sistema AND lower(pe.nome) = 'master'
              AND (pf.data_inicio IS NULL OR pf.data_inicio <= current_date)
              AND (pf.data_fim IS NULL OR pf.data_fim >= current_date)
            """;
        var mastersBefore = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            activeMasterSql, new { TenantId = tenantId }, transaction, cancellationToken: cancellationToken));

        if (update.IsPrimary)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                "UPDATE erp.profile_filiais SET principal = false WHERE profile_id = @UserId",
                new { UserId = userId }, transaction, cancellationToken: cancellationToken));
        }

        const string sql = """
            INSERT INTO erp.profile_filiais
                (id, profile_id, filial_id, perfil_id, principal, ativo, data_inicio, data_fim)
            VALUES (@Id, @UserId, @BranchId, @AccessProfileId, @IsPrimary, @IsActive, @StartsOn, @EndsOn)
            ON CONFLICT (profile_id, filial_id) DO UPDATE SET
                perfil_id = EXCLUDED.perfil_id, principal = EXCLUDED.principal,
                ativo = EXCLUDED.ativo, data_inicio = EXCLUDED.data_inicio, data_fim = EXCLUDED.data_fim
            RETURNING id AS Id, profile_id AS UserId, filial_id AS BranchId,
                      perfil_id AS AccessProfileId, principal AS IsPrimary, ativo AS IsActive,
                      data_inicio AS StartsOn, data_fim AS EndsOn
            """;
        var result = await connection.QuerySingleAsync<UserBranchAccessDto>(new CommandDefinition(sql, new
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BranchId = branchId,
            update.AccessProfileId,
            update.IsPrimary,
            update.IsActive,
            update.StartsOn,
            update.EndsOn,
        }, transaction, cancellationToken: cancellationToken));
        if (mastersBefore > 0 && await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                activeMasterSql, new { TenantId = tenantId }, transaction, cancellationToken: cancellationToken)) == 0)
            throw new InvalidOperationException("O último usuário Master ativo não pode perder o acesso.");
        transaction.Commit();
        return result;
    }

    private static async Task LockTenantAdministrationAsync(IDbConnection connection, IDbTransaction transaction,
        Guid tenantId, CancellationToken cancellationToken)
    {
        await connection.ExecuteAsync(new CommandDefinition(
            "SELECT pg_advisory_xact_lock(hashtextextended(CAST(@TenantId AS text), 0))",
            new { TenantId = tenantId }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<AccessProfileDto>> ListAccessProfilesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS Id, tenant_id AS TenantId, nome AS Name, descricao AS Description, COALESCE(sistema, false) AS IsSystem,
                   nivel_acesso AS AccessLevel, ordem AS "Order", COALESCE(ativo, false) AS IsActive
            FROM erp.perfis WHERE tenant_id = @TenantId ORDER BY ordem NULLS LAST, nome
            """;
        var rows = await (await OpenAsync(cancellationToken)).QueryAsync<AccessProfileDto>(
            new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<AccessProfileDto> CreateAccessProfileAsync(Guid tenantId, AccessProfileCreateDto create, CancellationToken cancellationToken)
    {
        var connection = await OpenAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
        var id = Guid.NewGuid();
        const string sql = """
            INSERT INTO erp.perfis (id, tenant_id, nome, descricao, sistema, nivel_acesso, ordem, ativo)
            VALUES (@Id, @TenantId, @Name, @Description, false, @AccessLevel,
                    COALESCE((SELECT max(ordem) + 1 FROM erp.perfis WHERE tenant_id = @TenantId), 1), true)
            RETURNING id AS Id, tenant_id AS TenantId, nome AS Name, descricao AS Description, sistema AS IsSystem,
                      nivel_acesso AS AccessLevel, ordem AS "Order", ativo AS IsActive
            """;
        var result = await connection.QuerySingleAsync<AccessProfileDto>(new CommandDefinition(sql, new
        {
            Id = id,
            TenantId = tenantId,
            Name = create.Name.Trim(),
            Description = create.Description?.Trim(),
            AccessLevel = create.AccessLevel?.Trim(),
        }, transaction, cancellationToken: cancellationToken));

        const string permissionsSql = """
            INSERT INTO erp.role_permissions
                (id, perfil_id, modulo, escopo, can_read, can_create, can_update, can_delete, can_export, can_manage)
            SELECT gen_random_uuid(), @ProfileId, modulo, 'CORRETORA', false, false, false, false, false, false
            FROM (SELECT DISTINCT modulo FROM erp.role_permissions rp
                  JOIN erp.perfis p ON p.id = rp.perfil_id WHERE p.tenant_id = @TenantId) modules
            WHERE modulo IS NOT NULL
            """;
        await connection.ExecuteAsync(new CommandDefinition(permissionsSql,
            new { ProfileId = id, TenantId = tenantId }, transaction, cancellationToken: cancellationToken));
        transaction.Commit();
        return result;
    }

    public async Task<AccessProfileDto?> UpdateAccessProfileAsync(Guid tenantId, Guid profileId, AccessProfileUpdateDto update, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE erp.perfis SET nome = @Name, descricao = @Description,
                   nivel_acesso = @AccessLevel, ativo = @IsActive
             WHERE id = @ProfileId AND tenant_id = @TenantId AND NOT sistema
            RETURNING id AS Id, tenant_id AS TenantId, nome AS Name, descricao AS Description, sistema AS IsSystem,
                      nivel_acesso AS AccessLevel, ordem AS "Order", ativo AS IsActive
            """;
        return await (await OpenAsync(cancellationToken)).QuerySingleOrDefaultAsync<AccessProfileDto>(
            new CommandDefinition(sql, new
            {
                TenantId = tenantId,
                ProfileId = profileId,
                Name = update.Name.Trim(),
                Description = update.Description?.Trim(),
                AccessLevel = update.AccessLevel?.Trim(),
                update.IsActive,
            }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<AccessPermissionDto>> ListPermissionsAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT rp.id AS Id, rp.perfil_id AS AccessProfileId, COALESCE(rp.modulo, '') AS Module,
                   COALESCE(rp.escopo, 'CORRETORA') AS Scope, COALESCE(rp.can_read, false) AS CanRead,
                   COALESCE(rp.can_create, false) AS CanCreate, COALESCE(rp.can_update, false) AS CanUpdate,
                   COALESCE(rp.can_delete, false) AS CanDelete, COALESCE(rp.can_export, false) AS CanExport,
                   COALESCE(rp.can_manage, false) AS CanManage
            FROM erp.role_permissions rp JOIN erp.perfis p ON p.id = rp.perfil_id
            WHERE p.tenant_id = @TenantId ORDER BY rp.modulo, p.ordem, p.nome
            """;
        var rows = await (await OpenAsync(cancellationToken)).QueryAsync<AccessPermissionDto>(
            new CommandDefinition(sql, new { TenantId = tenantId }, cancellationToken: cancellationToken));
        return rows.AsList();
    }

    public async Task<AccessPermissionDto?> UpdatePermissionAsync(Guid tenantId, Guid permissionId, AccessPermissionUpdateDto update, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE erp.role_permissions rp SET escopo = @Scope, can_read = @CanRead,
                   can_create = @CanCreate, can_update = @CanUpdate, can_delete = @CanDelete,
                   can_export = @CanExport, can_manage = @CanManage
              FROM erp.perfis p
             WHERE rp.id = @PermissionId AND p.id = rp.perfil_id AND p.tenant_id = @TenantId
               AND NOT (p.sistema AND lower(p.nome) = 'master')
            RETURNING rp.id AS Id, rp.perfil_id AS AccessProfileId, rp.modulo AS Module,
                      rp.escopo AS Scope, rp.can_read AS CanRead, rp.can_create AS CanCreate,
                      rp.can_update AS CanUpdate, rp.can_delete AS CanDelete,
                      rp.can_export AS CanExport, rp.can_manage AS CanManage
            """;
        return await (await OpenAsync(cancellationToken)).QuerySingleOrDefaultAsync<AccessPermissionDto>(
            new CommandDefinition(sql, new
            {
                TenantId = tenantId,
                PermissionId = permissionId,
                update.Scope,
                update.CanRead,
                update.CanCreate,
                update.CanUpdate,
                update.CanDelete,
                update.CanExport,
                update.CanManage,
            }, cancellationToken: cancellationToken));
    }
}
