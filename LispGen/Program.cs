using CliFx;

using LispGen.Lib;

using Microsoft.Extensions.DependencyInjection;

namespace LispGen;

public static class Program
{
    public static async Task<int> Main()
    {
        return await new CliApplicationBuilder()
            .AddCommandsFromThisAssembly()
            .UseTypeActivator((cmds) =>
            {
                var services = new ServiceCollection();

                services.AddTransient<Parser>();

                foreach (var cmd in cmds)
                {
                    services.AddTransient(cmd);
                }

                return services.BuildServiceProvider();
            })
            .Build()
            .RunAsync();
    }
}
