using WAssis.Application.Modules.Financial.Dtos;

namespace WAssis.Application.Modules.Financial.Interfaces;

public interface ICommissionStatementParser
{
    CommissionStatementAnalysisDto Analyze(string sourceType, string rawText);
}
