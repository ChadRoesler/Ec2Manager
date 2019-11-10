using Microsoft.Extensions.Hosting;
using HybridScaffolding;
using Ec2Manager.Models.DataManagement;

namespace Ec2Manager
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var ec2ManagerScaffold = new Ec2ManagerScaffold();
            HybridExecutor.DispatchExecutor(ec2ManagerScaffold, args, typeof(HostBuilder));
        }
    }
}
