namespace Kokoabim.CommandLineInterface;

public class ConsoleContext
{
    public IReadOnlyList<ConsoleArgument> Arguments => _consoleAppCommand.Arguments;
    public CancellationToken CancellationToken { get; }
    public IConsoleEvents Events { get; }
    public string HelpText => _consoleAppCommand.HelpText();

    /// <summary>
    /// Returns true if no arguments exist or arguments exist but none have values (built-in arguments are ignored).
    /// </summary>
    public bool NoArgumentValuesExist => Arguments.All(a => !a.Exists || a.IsBuiltIn);

    private readonly IConsoleAppCommand _consoleAppCommand;

    public ConsoleContext(IConsoleAppCommand consoleAppCommand, IConsoleEvents consoleEvents, CancellationToken cancellationToken)
    {
        _consoleAppCommand = consoleAppCommand;
        CancellationToken = cancellationToken;
        Events = consoleEvents;
    }

    #region methods

    /// <summary>
    /// Gets the positional argument with the specified name.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public ConsoleArgument Get(string name) => Arguments.FirstOrDefault(a => a.Name == name && a.Type == ArgumentType.Positional) ?? throw new ArgumentException($"Argument '{name}' not found");

    /// <summary>
    /// Gets the positional argument with the specified index.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public ConsoleArgument Get(int index) => Arguments.FirstOrDefault(a => a.Index == index && a.Type == ArgumentType.Positional) ?? throw new ArgumentException($"Argument at index {index} not found");

    /// <summary>
    /// Gets the value as a integer of the positional argument with the specified name.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public int GetInt(string name) => Get(name).AsInt();

    /// <summary>
    /// Gets the option argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public ConsoleArgument GetOption(string name, bool compareId = false) => Arguments.FirstOrDefault(a => (a.Name == name || (compareId && a.Identifier == name)) && a.Type == ArgumentType.Option) ?? throw new ArgumentException($"Option '{name}' not found");

    /// <summary>
    /// Gets the value as an integer of the option argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public int GetOptionInt(string name, bool compareId = false) => GetOption(name, compareId).AsInt();

    /// <summary>
    /// Gets the value as an integer of the option argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    public int? GetOptionIntOrDefault(string name, bool compareId = false) => GetOptionOrDefault(name, compareId)?.AsInt();

    /// <summary>
    /// Gets the option argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    public ConsoleArgument? GetOptionOrDefault(string name, bool compareId = false) => Arguments.FirstOrDefault(a => (a.Name == name || (compareId && a.Identifier == name)) && a.Type == ArgumentType.Option);

    /// <summary>
    /// Gets the value as a string of the option argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public string GetOptionString(string name, bool compareId = false) => GetOption(name, compareId).AsString();

    /// <summary>
    /// Gets the values as a string array of the option argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    public string[] GetOptionStrings(string name, bool compareId = false) => GetOptionOrDefault(name, compareId)?.Values.Cast<string>().ToArray() ?? [];

    /// <summary>
    /// Gets the value of the option argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public object GetOptionValue(string name, bool compareId = false) => GetOption(name, compareId).GetValue();

    /// <summary>
    /// Gets the value of the option argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    public object? GetOptionValueOrDefault(string name, bool compareId = false) => GetOptionOrDefault(name, compareId)?.GetValueOrDefault();

    /// <summary>
    /// Gets the positional argument with the specified name.
    /// </summary>
    public ConsoleArgument? GetOrDefault(string name) => Arguments.FirstOrDefault(a => a.Name == name && a.Type == ArgumentType.Positional);

    /// <summary>
    /// Gets the positional argument with the specified index.
    /// </summary>
    public ConsoleArgument? GetOrDefault(int index) => Arguments.FirstOrDefault(a => a.Index == index && a.Type == ArgumentType.Positional);

    /// <summary>
    /// Gets the value as a string of the positional argument with the specified name.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public string GetString(string name) => Get(name).AsString();

    /// <summary>
    /// Gets the value as a string of the positional argument with the specified name.
    /// </summary>
    public string? GetStringOrDefault(string name) => GetOrDefault(name)?.GetValueOrDefault()?.ToString();

    /// <summary>
    /// Gets the switch argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public ConsoleArgument GetSwitch(string name, bool compareId = false) => Arguments.FirstOrDefault(a => (a.Name == name || (compareId && a.Identifier == name)) && a.Type == ArgumentType.Switch) ?? throw new ArgumentException($"Switch '{name}' not found");

    /// <summary>
    /// Gets the switch argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    public ConsoleArgument? GetSwitchOrDefault(string name, bool compareId = false) => Arguments.FirstOrDefault(a => (a.Name == name || (compareId && a.Identifier == name)) && a.Type == ArgumentType.Switch);

    /// <summary>
    /// Gets the value of the switch argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public object GetSwitchValue(string name, bool compareId = false) => GetSwitch(name, compareId).GetType();

    /// <summary>
    /// Gets the value of the switch argument with the specified name.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    public object? GetSwitchValueOrDefault(string name, bool compareId = false) => GetSwitchOrDefault(name, compareId)?.GetValueOrDefault();

    /// <summary>
    /// Gets the value of the positional argument with the specified name.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public object GetValue(string name) => Get(name).GetValue();

    /// <summary>
    /// Gets the value of the positional argument with the specified index.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the argument is not found.</exception>
    public object GetValue(int index) => Get(index).GetValue();

    /// <summary>
    /// Gets the value of the positional argument with the specified name.
    /// </summary>
    public object? GetValueOrDefault(string name) => GetOrDefault(name)?.GetValueOrDefault();

    /// <summary>
    /// Gets the value of the positional argument with the specified index.
    /// </summary>
    public object? GetValueOrDefault(int index) => GetOrDefault(index)?.GetValueOrDefault();

    /// <summary>
    /// Returns true if the switch argument with the specified name exists and has a value of true.
    /// </summary>
    /// <param name="compareId">If true, also compares the argument ID.</param>
    public bool HasSwitch(string name, bool compareId = false) => GetSwitchValueOrDefault(name, compareId) is bool b && b;

    #endregion 
}