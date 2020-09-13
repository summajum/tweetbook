using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace TweetBook.Installers
{
    public class MisInstaller : IInstaller
    {
        void IInstaller.InstallServices(IServiceCollection services, IConfiguration config)
        {
            services.AddSwaggerGen(x =>
            {
                x.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "TweetBook Api Swagger Doc",
                    Description = "TweetBook Api Swagger Desc",
                    Version = "TweetBook Api Swagger V1"
                });
            });
        }
    }
}
