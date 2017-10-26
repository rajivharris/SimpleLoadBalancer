using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LoadBalancer
{
    /// <summary>
    /// In-Memory resigtry to hold registered services
    /// This class has all helper methods to manage service registry
    /// </summary>
    public class ServiceRegistry : IServiceRegistry
    {
        // a simple in-memory registry
        private readonly ConcurrentDictionary<string, int> registry = new ConcurrentDictionary<string, int>();
        private readonly ILogger<ServiceRegistry> _logger;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="logger"></param>
        public ServiceRegistry(ILogger<ServiceRegistry> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Register service to the service registry
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public bool RegisterService(string service)
        {
            if (!registry.ContainsKey(service))
            {
                registry.AddOrUpdate(service, 0, (key, value) => value + 1);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes service from the service registry
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public bool RemoveService(string service)
        {
            if (registry.ContainsKey(service))
            {
                registry.TryRemove(service, out int load);
                return true;
            }

            return false;
        }

        public bool HasService() =>
            !registry.IsEmpty;


        /// <summary>
        /// Gets the least loaded service from the registry
        /// </summary>
        /// <returns></returns>
        public string GetLeastLoadedService() =>
            registry.Aggregate((l, r) => l.Value < r.Value ? l : r).Key;

        /// <summary>
        /// Gets all services registred with the load details
        /// </summary>
        /// <returns></returns>
        public string GetAllServices() =>
            string.Join("\n", registry.Select(e => $"{e.Key}:{e.Value}"));

        /// <summary>
        /// Invoke service and increment the load
        /// It will retry if it cannot reach a service
        /// </summary>
        /// <param name="host"></param>
        /// <param name="service"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<string> InvokeService(string host, string service, string path)
        {
            var finalServicePath = new StringBuilder();
            finalServicePath.Append(service);
            finalServicePath.Append(path);

            //forward the request
            using (var client = new HttpClient())
            {
                //increment the service load
                registry.AddOrUpdate(service, 0, (key, value) => value + 1);

                var req = new HttpRequestMessage();
                req.Headers.Add("X-Forwarded-Host", host);
                req.RequestUri = new Uri(finalServicePath.ToString());
                req.Headers.Host = host;
                string response = string.Empty;

                try
                {
                    var httpResponseMsg = await client.SendAsync(req);
                    response = await httpResponseMsg.Content.ReadAsStringAsync();

                    //decrement the service load
                    registry.AddOrUpdate(service, 0, (key, value) => value - 1);
                }
                catch (HttpRequestException ex)
                {
                    _logger.LogError($"Service not reachable: {service} : {ex.Message}");
                    //remove the service from the registry
                    RemoveService(service);

                    //retry request
                    if (!registry.IsEmpty)
                    {
                        var leastLoadedSvc = GetLeastLoadedService();
                        _logger.LogInformation($"Retrying with : {leastLoadedSvc}");
                        await InvokeService(host, leastLoadedSvc, path);
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                //return the response
                return response;
            }
        }

    }
}