using Nelibur.ObjectMapper;
using OrderGateway.Api.Data.Models;
using OrderGateway.Api.DTOs;
namespace OrderGateway.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMappingConfig(this IServiceCollection services)
    {
        TinyMapper.Bind<OrderDto, Order>(config =>
        {
            config.Bind(source => source.Id, dest => dest.Id);
            config.Bind(source => source.FirstName, dest => dest.FirstName);
            config.Bind(source => source.LastName, dest => dest.LastName);
            config.Bind(source => source.City, dest => dest.City);
            config.Bind(source => source.Postalcode, dest => dest.Postalcode);
            config.Bind(source => source.Country, dest => dest.Country);
            config.Bind(source => source.AddressLine1, dest => dest.AddressLine1);
            config.Bind(source => source.Status, dest => dest.Status);
            config.Bind(source => source.TotalAmount, dest => dest.TotalAmount);
            config.Bind(source => source.TransitionHistory, dest => dest.TransitionHistory);
        });
        TinyMapper.Bind<Order, OrderDto>(config =>
        {
            config.Bind(source => source.Id, dest => dest.Id);
            config.Bind(source => source.FirstName, dest => dest.FirstName);
            config.Bind(source => source.LastName, dest => dest.LastName);
            config.Bind(source => source.City, dest => dest.City);
            config.Bind(source => source.Postalcode, dest => dest.Postalcode);
            config.Bind(source => source.Country, dest => dest.Country);
            config.Bind(source => source.AddressLine1, dest => dest.AddressLine1);
            config.Bind(source => source.Status, dest => dest.Status);
            config.Bind(source => source.TotalAmount, dest => dest.TotalAmount);
            config.Bind(source => source.TransitionHistory, dest => dest.TransitionHistory);
        });
        return services;
    }
}