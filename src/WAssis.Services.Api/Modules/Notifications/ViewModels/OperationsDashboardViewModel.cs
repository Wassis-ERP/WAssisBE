namespace WAssis.Services.Api.Modules.Notifications.ViewModels;

public sealed record OperationsDashboardViewModel(
    int PendingQuotes,
    int PendingDocumentSearches,
    int FailedImportedDocuments,
    int DivergentReconciliations,
    int PoliciesNeedingReview,
    int WaitingWhatsAppHandoffs,
    int AuditEntriesLast24Hours);
