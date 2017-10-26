using System.Threading.Tasks;

namespace LoadBalancer
{
    public interface IServiceRegistry
    {
        bool RegisterService(string service);
        bool RemoveService(string service);
        string GetLeastLoadedService();
        string GetAllServices();
        Task<string> InvokeService(string host, string service, string path);
    }
}