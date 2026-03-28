namespace WAssis.Services.Api.Modules.Notifications.ViewModels;

public sealed record OperationsDashboardViewModel(
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
