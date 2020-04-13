using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Web.Components.EditableList;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ReSharper disable once AssignmentIsFullyDiscarded
            _ = Common.ContraCoreClient.Connected;

            Tests.Test();

            Console.WriteLine(Directory.GetCurrentDirectory());
            Console.WriteLine(Path.GetDirectoryName(typeof(Program).Assembly.Location));

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                 {
                     webBuilder.UseStartup<Startup>();
                 });
    }
}