using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TweetBook.Installers
{
    public static class InstallerExtensions
    {
        public static void InstallServicesinAssembly(this IServiceCollection services, IConfiguration configration)
        {
            var derivedInstallers = typeof(Startup).Assembly.ExportedTypes
               .Where(x => !x.IsAbstract && !x.IsInterface && typeof(IInstaller).IsAssignableFrom(x)).Select(Activator.CreateInstance).Cast<IInstaller>().ToList();

            derivedInstallers.ForEach(installer => installer.InstallServices(services, configration));
        }
    }
}
