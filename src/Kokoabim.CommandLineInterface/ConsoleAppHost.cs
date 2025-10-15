using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kokoabim.CommandLineInterface;

public interface IConsoleAppHost
{
    IHost Host { get; }
    ILoggerFactory LoggerFactory { get; }
    IServiceProvider ServiceProvider { get; }

    IConsoleAppHost AddScoped<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;
    IConsoleAppHost AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;
    IConsoleAppHost AddTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;
    void Build();
    T GetRequiredService<T>() where T : notnull;
    T? GetService<T>();
    IEnumerable<T> GetServices<T>();
}

public class ConsoleAppHost : IConsoleAppHost
{
    public IHost Host => _host is not null ? _host : throw new InvalidOperationException("ConsoleAppHost not built.");
    public ILoggerFactory LoggerFactory => _loggerFactory is not null ? _loggerFactory : throw new InvalidOperationException("ConsoleAppHost not built.");
    public IServiceProvider ServiceProvider => _serviceProvider is not null ? _serviceProvider : throw new InvalidOperationException("ConsoleAppHost not built.");

    private IHost? _host;
    private readonly IHostBuilder _hostBuilder;
    private ILoggerFactory? _loggerFactory;
    private IServiceProvider? _serviceProvider;

    public ConsoleAppHost()
    {
        _hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostContext, config) =>
            {
                _ = config
                    .SetBasePath(hostContext.HostingEnvironment.ContentRootPath)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureServices((hostContext, services) =>
            {
                _ = services
                    .AddOptions()
                    .AddSingleton(hostContext.HostingEnvironment)
                    .AddSingleton<IConsoleAppHost>(this)
                    .AddSingleton<ILoggerFactory, LoggerFactory>()
                    .AddLogging(builder =>
                {
                    _ = builder.AddSimpleConsole(options =>
                    {
                        _ = options.IncludeScopes = true;
                        _ = options.SingleLine = true;
                    });
                });
            });
    }

    public IConsoleAppHost AddScoped<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        _ = _hostBuilder.ConfigureServices(static services =>
        {
            _ = services.AddScoped<TService, TImplementation>();
        });

        return this;
    }

    public IConsoleAppHost AddSingleton<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        _ = _hostBuilder.ConfigureServices(static services =>
        {
            _ = services.AddSingleton<TService, TImplementation>();
        });

        return this;
    }

    public IConsoleAppHost AddTransient<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        _ = _hostBuilder.ConfigureServices(static services =>
        {
            _ = services.AddTransient<TService, TImplementation>();
        });

        return this;
    }

    public void Build()
    {
        _host = _hostBuilder.Build();
        _serviceProvider = _host.Services;
        _loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    public T GetRequiredService<T>() where T : notnull => _serviceProvider is not null ? _serviceProvider.GetRequiredService<T>() : throw new InvalidOperationException("ConsoleAppHost not built.");

    public T? GetService<T>() => _serviceProvider is not null ? _serviceProvider.GetService<T>() : throw new InvalidOperationException("ConsoleAppHost not built.");

    public IEnumerable<T> GetServices<T>() => _serviceProvider is not null ? _serviceProvider.GetServices<T>() : throw new InvalidOperationException("ConsoleAppHost not built.");
}