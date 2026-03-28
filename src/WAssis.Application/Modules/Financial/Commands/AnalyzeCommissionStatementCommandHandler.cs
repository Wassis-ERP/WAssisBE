using MediatR;
using WAssis.Application.Modules.Financial.Dtos;
using WAssis.Application.Modules.Financial.Interfaces;

namespace WAssis.Application.Modules.Financial.Commands;

public sealed class AnalyzeCommissionStatementCommandHandler(ICommissionStatementParser parser)
    : IRequestHandler<AnalyzeCommissionStatementCommand, CommissionStatementAnalysisDto>
{
    public Task<CommissionStatementAnalysisDto> Handle(AnalyzeCommissionStatementCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(parser.Analyze(request.SourceType, request.RawText));
    }
}
