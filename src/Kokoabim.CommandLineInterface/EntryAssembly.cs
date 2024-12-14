using System.Reflection;

namespace Kokoabim.CommandLineInterface;

public static class EntryAssembly
{
    #region properties
    public static AssemblyName AssemblyName { get; } = Assembly.GetEntryAssembly()?.GetName() ?? throw new Exception("Entry assembly name not found");
    public static Version AssemblyVersion { get; } = AssemblyName.Version ?? new Version(0, 0, 0, 0);
    public static string Name { get; } = AssemblyName.Name ?? throw new Exception("Entry assembly name not found");
    public static string Version => $"{AssemblyVersion.Major}.{AssemblyVersion.Minor}{(AssemblyVersion.Build > 0 ? $".{AssemblyVersion.Build}" : null)}";
    #endregion 
}