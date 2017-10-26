using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LoadBalancer
{
    /// <summary>
    /// Load balancer middeware
    /// </summary>
    public class LoadBalanceMiddleware
    {
        //get a handle of service registry
        private readonly IServiceRegistry _registry;
        public LoadBalanceMiddleware(RequestDelegate next, IServiceRegistry serviceRegistry)
        {
            _registry = serviceRegistry;
        }


        public async Task Invoke(HttpContext context)
        {
            //get least loaded service from service registry
            var service = _registry.GetLeastLoadedService();

            //pass the request to the least loaded server
            var resp = await _registry.InvokeService(context.Request.Host.ToString(),
                service, context.Request.Path);
            await context.Response.WriteAsync(resp);
        }

    }
}