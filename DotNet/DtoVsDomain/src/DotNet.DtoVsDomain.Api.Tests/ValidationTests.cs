using DotNet.DtoVsDomain.Api.Contracts.Requests;
using DotNet.DtoVsDomain.Api.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace DotNet.DtoVsDomain.Api.Tests;

public class ValidationTests
{
    [Fact]
    public void CreateOrderRequestValidator_Fails_OnEmptyItems()
    {
        var validator = new CreateOrderRequestValidator();
        var req = new CreateOrderRequest { CustomerId = Guid.NewGuid(), Items = new List<CreateOrderItemRequest>() };
        var result = validator.TestValidate(req);
        result.ShouldHaveValidationErrorFor(r => r.Items);
    }

    [Fact]
    public void CreateOrderItemRequestValidator_Fails_OnNonPositiveQuantityOrEmptyProductId()
    {
        var validator = new CreateOrderItemRequestValidator();
        var item = new CreateOrderItemRequest { ProductId = Guid.Empty, Quantity = 0 };
        var result = validator.TestValidate(item);
        result.ShouldHaveValidationErrorFor(i => i.ProductId);
        result.ShouldHaveValidationErrorFor(i => i.Quantity);
    }
}