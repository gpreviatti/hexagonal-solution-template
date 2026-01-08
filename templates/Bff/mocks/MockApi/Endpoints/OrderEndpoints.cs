// using Microsoft.AspNetCore.Mvc;

// namespace MockApi.Endpoints;

// internal static class OrderEndpoints
// {
//     public static WebApplication MapOrderEndpoints(this WebApplication app)
//     {
//         var cache = app.Services.GetRequiredService<HybridCacheService>();
        
//         var serviceKey = ServicesKeys.Orders.ToString();

//         var httpService = app.Services.GetRequiredKeyedService<BaseHttpService>(serviceKey);

//         var ordersGroup = app.MapGroup(serviceKey).WithTags(serviceKey);

//         ordersGroup.MapGet("/{id}", async (
//             [FromHeader] Guid correlationId,
//             [FromRoute] int id,
//             CancellationToken cancellationToken,
//             [FromHeader] bool cacheEnabled = true
//         ) => {
//             var response = cacheEnabled switch
//             {
//                 true => await cache.GetOrCreateAsync(
//                     $"{nameof(OrderEndpoints)}-{id}",
//                     async (cancellationToken) => await httpService.SendAsync($"/orders/{id}", HttpMethod.Get, cancellationToken, headers: new()
//                     {
//                         { "CorrelationId", correlationId.ToString() }
//                     }),
//                     cancellationToken
//                 ),
//                 false or _ => await httpService.SendAsync($"/orders/{id}", HttpMethod.Get, cancellationToken, headers: new()
//                 {
//                     { "CorrelationId", correlationId.ToString() }
//                 }),
//             };

//             return response?.Success ? Results.Ok(response) : Results.NotFound(response);
//         });

//         ordersGroup.MapPost("/", async (
//             [FromBody] dynamic request,
//             CancellationToken cancellationToken
//         ) =>
//         {
//             var response = await httpService.SendAsync("orders", HttpMethod.Post, cancellationToken, request);

//             if (!response.Success || response.Data == null)
//                 return Results.BadRequest(response);

//             return Results.Created($"/orders/{response.Data.Id}", response);
//         });

//         return app;
//     }
// }
