using Microsoft.AspNetCore.Builder;

namespace LoadBalancer
{
    public static class LoadBalancerMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoadBalancer(
        this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoadBalanceMiddleware>();
        }
    }
}