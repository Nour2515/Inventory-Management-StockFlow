namespace StockFlow.Models.Enums;

public enum InventoryTransactionType
{
    StockIn = 1,
    Sale = 2,
    Return = 3,
    TransferIn = 4,
    TransferOut = 5,
    Adjustment = 6,
    Reservation = 7,
    ReservationReleased = 8
}
