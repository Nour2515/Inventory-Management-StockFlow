namespace StockFlow.Exceptions;

public class InsufficientStockException : ConflictException
{
    public InsufficientStockException(string message)
        : base(message)
    {
    }
}
