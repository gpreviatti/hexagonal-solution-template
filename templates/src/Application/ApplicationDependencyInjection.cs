using Application.Common.Requests;
using Application.Common.UseCases;
using Application.Notifications;
using Application.Orders;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;
public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        //Add validators from assembly
        services.AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

        // Orders
        services.AddScoped<IBaseInOutUseCase<GetOrderRequest, BaseResponse<OrderDto>, GetOrderUseCase>, GetOrderUseCase>();
        services.AddScoped<IBaseInOutUseCase<CreateOrderRequest, BaseResponse<OrderDto>, CreateOrderUseCase>, CreateOrderUseCase>();
        services.AddScoped<IBaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<OrderDto>, GetAllOrdersUseCase>, GetAllOrdersUseCase>();

        // Notifications
        services.AddScoped<IBaseInOutUseCase<CreateNotificationRequest, BaseResponse<NotificationDto>, CreateNotificationUseCase>, CreateNotificationUseCase>();
        services.AddScoped<IBaseInOutUseCase<GetNotificationRequest, BaseResponse<NotificationDto>, GetNotificationUseCase>, GetNotificationUseCase>();
        services.AddScoped<IBaseInOutUseCase<BasePaginatedRequest, BasePaginatedResponse<NotificationDto>, GetAllNotificationsUseCase>, GetAllNotificationsUseCase>();
        services.AddScoped<IBaseInOutUseCase<UpdateNotificationRequest, BaseResponse<NotificationDto>, UpdateNotificationUseCase>, UpdateNotificationUseCase>();

        return services;
    }
}
