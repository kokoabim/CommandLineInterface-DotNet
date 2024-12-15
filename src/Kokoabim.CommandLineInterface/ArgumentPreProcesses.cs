namespace Kokoabim.CommandLineInterface;

[Flags]
public enum ArgumentPreProcesses
{
    None = 0,
    ExpandEnvironmentVariables = 1 << 0,
}