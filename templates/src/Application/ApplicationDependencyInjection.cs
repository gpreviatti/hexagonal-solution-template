using System.Diagnostics.CodeAnalysis;
using Application.Common.Requests;
using Application.Common.UseCases;
using Application.Notifications;
using Application.Orders;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

[ExcludeFromCodeCoverage]
public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //Add validators from assembly
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

        // Orders
        services.AddScoped<IBaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>>, GetOrderUseCase>();
        services.AddScoped<IBaseInOutUseCase<CreateOrderRequest, BaseResponse<OrderDto>>, CreateOrderUseCase>();
        services.AddScoped<IBaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<OrderDto>>, GetAllOrdersUseCase>();

        // Notifications
        services.AddScoped<IBaseInUseCase<CreateNotificationRequest>, CreateNotificationUseCase>();
        services.AddScoped<IBaseInOutUseCase<GetNotificationRequest, BaseResponse<NotificationDto>>, GetNotificationUseCase>();
        services.AddScoped<IBaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<NotificationDto>>, GetAllNotificationsUseCase>();

        return services;
    }
}
