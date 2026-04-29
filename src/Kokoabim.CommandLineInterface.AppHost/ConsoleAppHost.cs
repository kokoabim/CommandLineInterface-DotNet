using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Kokoabim.CommandLineInterface;

public interface IConsoleAppHost
{
    IHost Host { get; }
    IHostApplicationBuilder HostApplicationBuilder { get; }
    IHostEnvironment HostEnvironment { get; }
    IServiceProvider ServiceProvider { get; }

    IConsoleAppHost AddScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
        where TService : class
        where TImplementation : class, TService;
    IConsoleAppHost AddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
        where TService : class
        where TImplementation : class, TService;
    IConsoleAppHost AddTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
        where TService : class
        where TImplementation : class, TService;
    IHost Build();
    IConsoleAppHost Configure(Action<IServiceCollection, IConfigurationManager, IHostEnvironment> configure);
    T GetRequiredService<T>() where T : notnull;
    T? GetService<T>();
    IEnumerable<T> GetServices<T>();
}

public class ConsoleAppHost : IConsoleAppHost
{
    public IHost Host => _host is not null ? _host : throw new InvalidOperationException("ConsoleAppHost not built");
    public IHostApplicationBuilder HostApplicationBuilder { get; }
    public IHostEnvironment HostEnvironment { get; }
    public IServiceProvider ServiceProvider => _serviceProvider is not null ? _serviceProvider : throw new InvalidOperationException("ConsoleAppHost not built");

    private IHost? _host;
    private IServiceProvider? _serviceProvider;

    public ConsoleAppHost(
        Action<IServiceCollection, IConfigurationManager, IHostEnvironment>? configure = null,
        Assembly? assemblyForAppSettingsFile = null,
        bool assemblyAppSettingsFileOptional = true,
        bool reloadAssemblyAppSettingsFileOnChange = false,
        string? appSettingsFile = null,
        bool appSettingsFileOptional = true,
        bool reloadAppSettingFilesOnChange = false)
    {
        HostApplicationBuilder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
        HostEnvironment = HostApplicationBuilder.Environment;

        if (configure is not null) configure(HostApplicationBuilder.Services, HostApplicationBuilder.Configuration, HostEnvironment);

        _ = HostApplicationBuilder.Configuration.SetBasePath(HostEnvironment.ContentRootPath);

        if (assemblyForAppSettingsFile is not null)
        {
            var assemblyName = assemblyForAppSettingsFile.GetName().Name ?? "appsettings";

            _ = HostApplicationBuilder.Configuration.AddJsonFile($"{assemblyName}.json", optional: assemblyAppSettingsFileOptional, reloadOnChange: reloadAssemblyAppSettingsFileOnChange);
            _ = HostApplicationBuilder.Configuration.AddJsonFile($"{assemblyName}.{HostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: reloadAssemblyAppSettingsFileOnChange);
        }

        if (!string.IsNullOrWhiteSpace(appSettingsFile))
        {
            _ = HostApplicationBuilder.Configuration.AddJsonFile(appSettingsFile, optional: appSettingsFileOptional, reloadOnChange: reloadAppSettingFilesOnChange);

            if (appSettingsFile.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                _ = HostApplicationBuilder.Configuration.AddJsonFile($"{appSettingsFile[..^5]}.{HostEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: reloadAppSettingFilesOnChange);
            }
        }

        _ = HostApplicationBuilder.Configuration.AddEnvironmentVariables();

        _ = HostApplicationBuilder.Services
            .AddOptions()
            .AddLogging(static builder =>
            {
                _ = builder.AddSimpleConsole(static options =>
                {
                    _ = options.IncludeScopes = true;
                    _ = options.SingleLine = true;
                });
            });
    }

    public IConsoleAppHost AddScoped<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        if (_host is not null) throw new InvalidOperationException("ConsoleAppHost already built");

        _ = HostApplicationBuilder.Services.AddScoped<TService, TImplementation>();

        return this;
    }

    public IConsoleAppHost AddSingleton<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        if (_host is not null) throw new InvalidOperationException("ConsoleAppHost already built");

        _ = HostApplicationBuilder.Services.AddSingleton<TService, TImplementation>();

        return this;
    }

    public IConsoleAppHost AddTransient<TService, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TImplementation>()
        where TService : class
        where TImplementation : class, TService
    {
        if (_host is not null) throw new InvalidOperationException("ConsoleAppHost already built");

        _ = HostApplicationBuilder.Services.AddTransient<TService, TImplementation>();

        return this;
    }

    public IHost Build()
    {
        _host = ((HostApplicationBuilder)HostApplicationBuilder).Build();
        _serviceProvider = _host.Services;

        return _host;
    }

    public IConsoleAppHost Configure(Action<IServiceCollection, IConfigurationManager, IHostEnvironment> configure)
    {
        if (_host is not null) throw new InvalidOperationException("ConsoleAppHost already built");

        configure(HostApplicationBuilder.Services, HostApplicationBuilder.Configuration, HostEnvironment);

        return this;
    }

    public T GetRequiredService<T>() where T : notnull => _serviceProvider is not null ? _serviceProvider.GetRequiredService<T>() : throw new InvalidOperationException("ConsoleAppHost not built");

    public T? GetService<T>() => _serviceProvider is not null ? _serviceProvider.GetService<T>() : throw new InvalidOperationException("ConsoleAppHost not built");

    public IEnumerable<T> GetServices<T>() => _serviceProvider is not null ? _serviceProvider.GetServices<T>() : throw new InvalidOperationException("ConsoleAppHost not built");
}