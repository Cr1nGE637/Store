using Store.Identity.Contracts.Events;
using Store.Identity.Domain.Aggregates;
using Store.Identity.Domain.Enums;
using Store.Identity.Domain.ValueObjects;

namespace Store.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_CreatesCustomerAndRaisesRegisteredEvent()
    {
        var email = Email.Create("customer@demo.local").Value;

        var result = User.Create("Demo Customer", email, "hashed-password");

        Assert.True(result.IsSuccess);
        Assert.Equal("Demo Customer", result.Value.Name);
        Assert.Equal(email, result.Value.Email);
        Assert.Equal(UserRole.Customer, result.Value.Role);

        var domainEvent = Assert.IsType<UserRegisteredEvent>(Assert.Single(result.Value.DomainEvents));
        Assert.Equal(result.Value.Id, domainEvent.UserId);
        Assert.Equal("customer@demo.local", domainEvent.Email);
    }

    [Fact]
    public void Create_WithManagerRole_CreatesManager()
    {
        var email = Email.Create("manager@demo.local").Value;

        var result = User.Create("Demo Manager", email, "hashed-password", UserRole.Manager);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserRole.Manager, result.Value.Role);
    }

    [Theory]
    [InlineData("")]
    [InlineData("                                                   ")]
    public void Create_WithInvalidName_ReturnsFailure(string name)
    {
        var email = Email.Create("customer@demo.local").Value;

        var result = User.Create(name, email, "hashed-password");

        Assert.True(result.IsFailure);
    }
}
