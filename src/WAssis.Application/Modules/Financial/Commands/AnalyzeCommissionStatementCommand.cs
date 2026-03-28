using MediatR;
using WAssis.Application.Modules.Financial.Dtos;

namespace WAssis.Application.Modules.Financial.Commands;

public sealed record AnalyzeCommissionStatementCommand(
    string SourceType,
    string RawText) : IRequest<CommissionStatementAnalysisDto>;
