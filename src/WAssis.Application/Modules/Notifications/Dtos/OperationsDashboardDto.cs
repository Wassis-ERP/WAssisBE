namespace WAssis.Application.Modules.Notifications.Dtos;

public sealed record OperationsDashboardDto(
    int PendingQuotes,
    int PendingDocumentSearches,
    int FailedImportedDocuments,
    int DivergentReconciliations,
    int PoliciesNeedingReview,
    int WaitingWhatsAppHandoffs,
    int AuditEntriesLast24Hours);
