using CSharpFunctionalExtensions;

namespace Store.Carts.Domain.ValueObjects;

public sealed record CheckoutDetails(
    RecipientName RecipientName,
    PhoneNumber Phone,
    DeliveryAddress DeliveryAddress,
    DeliveryMethod DeliveryMethod,
    PaymentMethod PaymentMethod)
{
    public static Result<CheckoutDetails> Create(
        string recipientName,
        string phone,
        string deliveryAddress,
        string deliveryMethod,
        string paymentMethod)
    {
        var recipientNameResult = RecipientName.Create(recipientName);
        if (recipientNameResult.IsFailure)
            return Result.Failure<CheckoutDetails>(recipientNameResult.Error);

        var phoneResult = PhoneNumber.Create(phone);
        if (phoneResult.IsFailure)
            return Result.Failure<CheckoutDetails>(phoneResult.Error);

        var deliveryAddressResult = DeliveryAddress.Create(deliveryAddress);
        if (deliveryAddressResult.IsFailure)
            return Result.Failure<CheckoutDetails>(deliveryAddressResult.Error);

        var deliveryMethodResult = DeliveryMethod.Create(deliveryMethod);
        if (deliveryMethodResult.IsFailure)
            return Result.Failure<CheckoutDetails>(deliveryMethodResult.Error);

        var paymentMethodResult = PaymentMethod.Create(paymentMethod);
        if (paymentMethodResult.IsFailure)
            return Result.Failure<CheckoutDetails>(paymentMethodResult.Error);

        return Result.Success(new CheckoutDetails(
            recipientNameResult.Value,
            phoneResult.Value,
            deliveryAddressResult.Value,
            deliveryMethodResult.Value,
            paymentMethodResult.Value));
    }
}
