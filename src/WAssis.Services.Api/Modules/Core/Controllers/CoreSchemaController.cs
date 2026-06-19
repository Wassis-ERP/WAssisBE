using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WAssis.Application.Abstractions;
using WAssis.Application.Modules.Core.Interfaces;
using WAssis.Infra.CrossCutting.Identity.Authorization;
using WAssis.Services.Api.Modules.Core.Contracts;

namespace WAssis.Services.Api.Modules.Core.Controllers;

[ApiController]
[Route("api/core")]
[Authorize(Policy = AccessPolicies.AuthenticatedUser)]
public sealed class CoreSchemaController(
    ICurrentUserContext currentUserContext,
    ICoreBranchReadRepository branchReadRepository) : ControllerBase
{
    private static readonly CoreSchemaModuleViewModel[] Modules =
    [
        new("plataforma", ["tenants", "filiais", "profiles", "profile_filiais", "role_permissions"]),
        new("cadastros", ["produtores", "segurados", "pessoa_contato", "seguradoras", "ramos", "origens", "motivos_perda", "coberturas_catalogo"]),
        new("kanban", ["pipelines", "pipeline_stages"]),
        new("comercial", ["oportunidades", "calculos", "calc_auto", "calc_residencia", "calc_condominio", "calc_vida", "calc_empresa", "calc_diversos", "calculo_coberturas", "cotacoes"]),
        new("contratual", ["apolices", "propostas", "apolice_itens", "item_veiculo", "item_imovel", "item_empresa", "item_vida", "item_coberturas", "sinistros", "sinistro_envolvidos", "pos_vendas"]),
        new("financeiro_config", ["recebimento_grades", "recebimento_grade_parcelas", "repasse_regras"]),
        new("financeiro", ["parcelas", "financeiro_cobrancas", "comissoes", "repasses"]),
        new("suporte", ["atividades", "atividade_mencoes", "anexos", "audit_logs", "integracao_logs"]),
        new("campos_personalizados", ["campo_definicoes", "campo_opcoes", "campo_valores", "campo_valor_opcoes"]),
    ];

    [HttpGet("schema")]
    [ProducesResponseType(typeof(CoreSchemaViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetSchema()
    {
        var tenantId = currentUserContext.TenantId?.Trim();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Tenant context is required.",
                detail: "The authenticated identity does not contain a tenant_id claim.");
        }

        var branchIds = currentUserContext.BranchIds
            .Append(currentUserContext.BranchId)
            .Where(static branchId => !string.IsNullOrWhiteSpace(branchId))
            .Select(static branchId => branchId!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return Ok(new CoreSchemaViewModel(
            "1.0.0",
            "erp",
            tenantId,
            currentUserContext.BranchId,
            branchIds,
            currentUserContext.HasAllBranchesAccess,
            Modules));
    }

    [HttpGet("branches")]
    [ProducesResponseType(typeof(IReadOnlyCollection<CoreBranchViewModel>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetBranches(CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(currentUserContext.TenantId, out var tenantId))
        {
            return Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Valid tenant context is required.",
                detail: "The tenant_id claim must contain a UUID from the ERP core schema.");
        }

        var branchIds = currentUserContext.BranchIds
            .Append(currentUserContext.BranchId)
            .Where(static value => Guid.TryParse(value, out _))
            .Select(static value => Guid.Parse(value!))
            .Distinct()
            .ToArray();

        var branches = await branchReadRepository.ListAsync(
            tenantId,
            branchIds,
            currentUserContext.HasAllBranchesAccess,
            cancellationToken);

        return Ok(branches.Select(branch => new CoreBranchViewModel(
            branch.Id,
            branch.ParentBranchId,
            branch.Name,
            branch.DocumentNumber,
            branch.IsActive)));
    }
}
