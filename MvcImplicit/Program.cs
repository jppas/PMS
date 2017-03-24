﻿using System.IO;
using System.Net;
using IdentityModel;
using Microsoft.AspNetCore.Hosting;
namespace MvcImplicit
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://localhost:44077")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();



            host.Run();
        }
    }
}