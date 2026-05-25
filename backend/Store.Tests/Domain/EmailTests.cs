using Store.Identity.Domain.ValueObjects;

namespace Store.Tests.Domain;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_NormalizesValue()
    {
        var result = Email.Create(" Customer@Demo.Local ");

        Assert.True(result.IsSuccess);
        Assert.Equal("customer@demo.local", result.Value.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@demo.local")]
    public void Create_WithInvalidEmail_ReturnsFailure(string email)
    {
        var result = Email.Create(email);

        Assert.True(result.IsFailure);
    }
}
