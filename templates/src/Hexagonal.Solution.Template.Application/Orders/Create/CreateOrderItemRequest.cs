﻿namespace Hexagonal.Solution.Template.Application.Orders.Create;
public sealed record CreateOrderItemRequest(string Name, string Description, decimal Value);
