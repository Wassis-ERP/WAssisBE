namespace WAssis.Domain.Modules.Quotes.ValueObjects;

public class InstallmentSnapshot
{
    public int Number { get; private set; }
    public decimal Amount { get; private set; }
    public decimal? TotalAmount { get; private set; }

    private InstallmentSnapshot()
    {
    }

    public InstallmentSnapshot(int number, decimal amount, decimal? totalAmount)
    {
        Number = number;
        Amount = amount;
        TotalAmount = totalAmount;
    }
}
