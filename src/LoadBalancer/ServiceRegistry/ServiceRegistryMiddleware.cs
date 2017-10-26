using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace LoadBalancer
{
    public class ServiceRegistryMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IServiceRegistry _registry;

        public ServiceRegistryMiddleware(RequestDelegate next,
            ILogger<ServiceRegistryMiddleware> logger,
            IServiceRegistry registry)
        {
            _next = next;
            _logger = logger;
            _registry = registry;
        }

        /// <summary>
        /// Handles service registry
        /// Accepts request of following format
        /// http://localhost:60000/registry?server=http://localhost:60001
        /// Http verbs are used to add or delete the service
        /// PUT will add, DELETE will remove the service
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task Invoke(HttpContext context)
        {
            //short circuit the request if it is for service registration
            if (context.Request.Path.HasValue && context.Request.Path == "/registry")
            {
                //get the server to register
                string server = context.Request.Query["server"];

                //dont process if server is empty
                if (string.IsNullOrWhiteSpace(server) && context.Request.Method != "GET")
                {
                    context.Response.StatusCode = 400;
                    _logger.LogInformation("Bad request. No server information provided");
                    return;
                }

                switch (context.Request.Method)
                {
                    case "PUT"://Adds service to registry
                        if (_registry.RegisterService(server))
                        {
                            _logger.LogInformation($"Registered service {server}");
                            await context.Response.WriteAsync("Registered service");
                        }
                        else
                        {
                            context.Response.StatusCode = 304;
                            _logger.LogWarning("Service already exists");
                        }
                        break;
                    case "DELETE": // deletes the service from registry
                        if (_registry.RemoveService(server))
                        {
                            _logger.LogInformation($"Removed service {server}");
                            await context.Response.WriteAsync("Removed service");
                        }
                        else
                        {
                            context.Response.StatusCode = 304;
                            _logger.LogWarning("Service does not exist");
                        }
                        break;
                    case "GET"://get the list of registered services
                        await context.Response.WriteAsync(_registry.GetAllServices());
                        break;
                    default:
                        _logger.LogInformation("Not supported");
                        context.Response.StatusCode = 405;
                        await context.Response.WriteAsync("Not supported");
                        break;
                }

            }
            else
            {
                await _next.Invoke(context);
            }
        }
    }
}