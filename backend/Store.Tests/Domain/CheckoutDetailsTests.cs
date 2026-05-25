namespace Store.Tests.Domain;

public class CheckoutDetailsTests
{
    [Fact]
    public void CartsCheckoutDetails_CreateWithValidValues_TrimsAndCreatesDetails()
    {
        var result = Store.Carts.Domain.ValueObjects.CheckoutDetails.Create(
            " Ivan Petrov ",
            " +7 999 000-00-00 ",
            " Tomsk, Lenina 1 ",
            " Courier ",
            " Card ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Ivan Petrov", result.Value.RecipientName.Value);
        Assert.Equal("+7 999 000-00-00", result.Value.Phone.Value);
        Assert.Equal("Tomsk, Lenina 1", result.Value.DeliveryAddress.Value);
        Assert.Equal("Courier", result.Value.DeliveryMethod.Value);
        Assert.Equal("Card", result.Value.PaymentMethod.Value);
    }

    [Fact]
    public void OrderingCheckoutDetails_CreateWithValidValues_TrimsAndCreatesDetails()
    {
        var result = Store.Ordering.Domain.ValueObjects.CheckoutDetails.Create(
            " Ivan Petrov ",
            " +7 999 000-00-00 ",
            " Card ");

        Assert.True(result.IsSuccess);
        Assert.Equal("Ivan Petrov", result.Value.RecipientName.Value);
        Assert.Equal("+7 999 000-00-00", result.Value.Phone.Value);
        Assert.Equal("Card", result.Value.PaymentMethod.Value);
    }

    [Theory]
    [InlineData("", "+79990000000", "Tomsk, Lenina 1", "Courier", "Card", "Recipient name is required")]
    [InlineData("Ivan Petrov", "", "Tomsk, Lenina 1", "Courier", "Card", "Phone is required")]
    [InlineData("Ivan Petrov", "phone", "Tomsk, Lenina 1", "Courier", "Card", "Phone format is invalid")]
    [InlineData("Ivan Petrov", "+79990000000", "", "Courier", "Card", "Delivery address is required")]
    [InlineData("Ivan Petrov", "+79990000000", "Tomsk, Lenina 1", "", "Card", "Delivery method is required")]
    [InlineData("Ivan Petrov", "+79990000000", "Tomsk, Lenina 1", "Courier", "", "Payment method is required")]
    public void CartsCheckoutDetails_CreateWithInvalidValues_Fails(
        string recipientName,
        string phone,
        string deliveryAddress,
        string deliveryMethod,
        string paymentMethod,
        string expectedError)
    {
        var result = Store.Carts.Domain.ValueObjects.CheckoutDetails.Create(
            recipientName,
            phone,
            deliveryAddress,
            deliveryMethod,
            paymentMethod);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }

    [Theory]
    [InlineData("", "+79990000000", "Card", "Recipient name is required")]
    [InlineData("Ivan Petrov", "", "Card", "Phone is required")]
    [InlineData("Ivan Petrov", "phone", "Card", "Phone format is invalid")]
    [InlineData("Ivan Petrov", "+79990000000", "", "Payment method is required")]
    public void OrderingCheckoutDetails_CreateWithInvalidValues_Fails(
        string recipientName,
        string phone,
        string paymentMethod,
        string expectedError)
    {
        var result = Store.Ordering.Domain.ValueObjects.CheckoutDetails.Create(
            recipientName,
            phone,
            paymentMethod);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedError, result.Error);
    }

    [Fact]
    public void CartsCheckoutDetails_WhenRecipientNameIsTooLong_Fails()
    {
        var result = Store.Carts.Domain.ValueObjects.CheckoutDetails.Create(
            new string('a', Store.Carts.Domain.ValueObjects.RecipientName.MaxLength + 1),
            "+79990000000",
            "Tomsk, Lenina 1",
            "Courier",
            "Card");

        Assert.True(result.IsFailure);
        Assert.Equal("Recipient name cannot exceed 200 characters", result.Error);
    }

    [Fact]
    public void OrderingCheckoutDetails_WhenRecipientNameIsTooLong_Fails()
    {
        var result = Store.Ordering.Domain.ValueObjects.CheckoutDetails.Create(
            new string('a', Store.Ordering.Domain.ValueObjects.RecipientName.MaxLength + 1),
            "+79990000000",
            "Card");

        Assert.True(result.IsFailure);
        Assert.Equal("Recipient name cannot exceed 200 characters", result.Error);
    }
}
