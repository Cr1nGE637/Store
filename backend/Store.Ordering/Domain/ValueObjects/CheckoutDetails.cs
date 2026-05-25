using CSharpFunctionalExtensions;

namespace Store.Ordering.Domain.ValueObjects;

public sealed record CheckoutDetails(
    RecipientName RecipientName,
    PhoneNumber Phone,
    PaymentMethod PaymentMethod)
{
    public static Result<CheckoutDetails> Create(
        string recipientName,
        string phone,
        string paymentMethod)
    {
        var recipientNameResult = RecipientName.Create(recipientName);
        if (recipientNameResult.IsFailure)
            return Result.Failure<CheckoutDetails>(recipientNameResult.Error);

        var phoneResult = PhoneNumber.Create(phone);
        if (phoneResult.IsFailure)
            return Result.Failure<CheckoutDetails>(phoneResult.Error);

        var paymentMethodResult = PaymentMethod.Create(paymentMethod);
        if (paymentMethodResult.IsFailure)
            return Result.Failure<CheckoutDetails>(paymentMethodResult.Error);

        return Result.Success(new CheckoutDetails(
            recipientNameResult.Value,
            phoneResult.Value,
            paymentMethodResult.Value));
    }
}
