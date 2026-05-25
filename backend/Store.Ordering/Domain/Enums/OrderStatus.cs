namespace Store.Ordering.Domain.Enums;

public enum OrderStatus
{
    AwaitingStock,
    Unpaid,
    Paid,
    Cancelled,
    Rejected
}
