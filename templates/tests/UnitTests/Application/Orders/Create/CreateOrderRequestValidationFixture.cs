﻿using Application.Orders;
using FluentValidation;

namespace UnitTests.Application.Orders.Create;
public class CreateOrderRequestValidationFixture
{
    public IValidator<CreateOrderRequest> validator;

    public CreateOrderRequestValidationFixture()
    {
        validator = new CreateOrderRequestValidator();
    }
}
