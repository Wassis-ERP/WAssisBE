namespace WAssis.Application.Modules.Notifications.Dtos;

public sealed record OperationsDashboardDto(
    int PendingQuotes,
    int PendingDocumentSearches,
    int FailedImportedDocuments,
    int DocumentsRequiringReview,
    int DivergentReconciliations,
    int SettledReconciliationsLast24Hours,
    int PoliciesNeedingReview,
    int WaitingWhatsAppHandoffs,
    int HumanActiveConversations,
    int AuditEntriesLast24Hours);
