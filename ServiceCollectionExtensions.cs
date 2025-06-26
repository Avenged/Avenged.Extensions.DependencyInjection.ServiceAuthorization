using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace Avenged.Extensions.DependencyInjection.ServiceAuthorization;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuthorizedService<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.AddScoped<TImplementation>();
        services.AddScoped<TInterface>(provider =>
        {
            var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
            var interceptor = provider.GetRequiredService<AuthorizationInterceptor>();
            var target = provider.GetRequiredService<TImplementation>();

            return proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(target, interceptor);
        });

        return services;
    }

    public static IServiceCollection AddServiceAuthorization(this IServiceCollection services)
    {
        services.AddScoped<AuthorizationInterceptor>();
        services.AddSingleton<ProxyGenerator>();
        return services;
    }
}
