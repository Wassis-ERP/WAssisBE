using WAssis.Application.Modules.Documents.Dtos;

namespace WAssis.Application.Modules.Documents.Interfaces;

public interface IProposalDocumentParser
{
    ProposalDocumentParsingResultDto Parse(string extractedText);
}
