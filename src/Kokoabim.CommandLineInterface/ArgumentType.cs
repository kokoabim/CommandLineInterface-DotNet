namespace Kokoabim.CommandLineInterface;

public enum ArgumentType
{
    /// <summary>
    /// A positional argument.
    /// </summary>
    Positional,

    /// <summary>
    /// An option argument with a value. The value is required.
    /// </summary>
    Option,

    /// <summary>
    /// A switch argument without a value. If the switch is present, the value is true; otherwise, the value is false.
    /// </summary>
    Switch
}