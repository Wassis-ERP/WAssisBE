namespace WAssis.Domain.Modules.Quotes;

public sealed class QuoteRequestConflictException() : Exception("A chave de idempotência já está em uso por outro pedido.");
