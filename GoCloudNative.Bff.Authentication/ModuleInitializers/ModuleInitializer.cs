using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public static class ModuleInitializer
{
    public static IServiceCollection AddSecurityBff(this IServiceCollection serviceCollection, 
        Action<BffOptions>? configureOptions = null)
    {
        var options = new BffOptions();
        configureOptions?.Invoke(options);
        
        var proxyBuilder = serviceCollection
            .AddReverseProxy()
            .AddTransforms<HttpHeaderTransformation>();

        options?.ApplyReverseProxyConfiguration(proxyBuilder);

        options?.ApplyDistributedCache(serviceCollection);
        
        options?.IdentityProviderFactory(serviceCollection);

        return serviceCollection
            .AddMemoryCache()
            .AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(15);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
    }

    public static WebApplication UseSecurityBff(this WebApplication app)
    {
        app.MapAuthenticationEndpoints("account");
        app.MapReverseProxy();
        
        app.UseSession();

        return app;
    }
}