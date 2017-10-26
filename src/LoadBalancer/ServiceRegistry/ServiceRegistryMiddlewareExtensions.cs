using Microsoft.AspNetCore.Builder;

namespace LoadBalancer
{
    public static class ServiceRegistryMiddlewareExtensions
    {
        public static IApplicationBuilder UseRegistryService(
        this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ServiceRegistryMiddleware>();
        }
    }
}