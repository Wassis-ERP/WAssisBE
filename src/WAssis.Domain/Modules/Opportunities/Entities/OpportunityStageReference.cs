namespace WAssis.Domain.Modules.Opportunities.Entities;

// Read-only lookup: a stage determines the pipeline; the opportunity stores only stage_id.
public sealed class OpportunityStageReference
{
    public string Id { get; private set; } = string.Empty;
    public string PipelineId { get; private set; } = string.Empty;

    private OpportunityStageReference() { }
    public OpportunityStageReference(string id, string pipelineId) { Id = id; PipelineId = pipelineId; }
}
