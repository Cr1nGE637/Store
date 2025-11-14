using Store.Domain.ValueObjects;

namespace Store.Application.DTOs;

public record GetCustomerDto(Guid Id, string Name, Email Email);