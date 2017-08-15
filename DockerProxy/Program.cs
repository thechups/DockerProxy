using System;
using System.Threading;
using System.Threading.Tasks;
using PeterKottas.DotNetCore.WindowsService;
using PeterKottas.DotNetCore.WindowsService.Base;
using PeterKottas.DotNetCore.WindowsService.Interfaces;

namespace DockerProxy
{
    public class Program
    {

        public static void Main(string[] args)
        {
            ServiceRunner<Service>.Run(config =>
            {
                config.SetName("DockerProxy");
                config.SetDisplayName("Docker Proxy");
                config.Service(serviceConfig =>
                {
                    serviceConfig.ServiceFactory((arguments, controller) => new Service());
                    serviceConfig.OnStart((service, extraArguments) =>
                    {                      
                        service.Start();
                    });

                    serviceConfig.OnStop(service =>
                    {
                        service.Stop();
                    });                    
                });
            });
        }
    }

    class Service : MicroService, IMicroService
    {
        readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();
        private Task _task;
        public void Start()
        {
            this.StartBase();
            _task = new Startup("Development").Start(_tokenSource.Token);
            Timers.Start("Poller", 1000, () => {  }, (e) => { });            
        }
      
        public void Stop()
        {
            this.StopBase();
            _tokenSource.Cancel();
            _task.Wait();
        }
    }
}
