namespace Kokoabim.CommandLineInterface;

public class ConsoleContext
{
    public IEnumerable<ConsoleArgument> Arguments => _consoleAppCommand.Arguments;
    public CancellationToken CancellationToken { get; }
    public string HelpText => _consoleAppCommand.HelpText();

    /// <summary>
    /// Returns true if no arguments exist or arguments exist but none have values (built-in arguments are ignored).
    /// </summary>
    public bool NoArgumentValuesExist => Arguments.All(a => !a.Exists || a.IsBuiltIn);

    private readonly IConsoleAppCommand _consoleAppCommand;

    public ConsoleContext(IConsoleAppCommand consoleAppCommand, CancellationToken cancellationToken)
    {
        _consoleAppCommand = consoleAppCommand;
        CancellationToken = cancellationToken;
    }

    #region methods

    /// <summary>
    /// Gets the positional argument with the specified name. If the argument is not found, throws an exception.
    /// </summary>
    public ConsoleArgument Get(string name) => Arguments.First(a => a.Name == name && a.Type == ArgumentType.Positional);

    /// <summary>
    /// Gets the positional argument with the specified index. If the argument is not found, throws an exception.
    /// </summary>
    public ConsoleArgument Get(int index) => Arguments.First(a => a.Index == index && a.Type == ArgumentType.Positional);

    public ConsoleArgument GetOption(string identifier, bool compareName = false) => Arguments.First(a => (a.Identifier == identifier || (compareName && a.Name == identifier)) && a.Type == ArgumentType.Option);

    public ConsoleArgument? GetOptionOrDefault(string identifier, bool compareName = false) => Arguments.FirstOrDefault(a => (a.Identifier == identifier || (compareName && a.Name == identifier)) && a.Type == ArgumentType.Option);

    public object GetOptionValue(string identifier, bool compareName = false) => GetOption(identifier, compareName).GetValue();

    public object? GetOptionValueOrDefault(string identifier, bool compareName = false) => GetOptionOrDefault(identifier, compareName)?.GetValueOrNull();

    /// <summary>
    /// Gets the positional argument with the specified name. If the argument is not found, returns null.
    /// </summary>
    public ConsoleArgument? GetOrDefault(string name) => Arguments.FirstOrDefault(a => a.Name == name && a.Type == ArgumentType.Positional);

    /// <summary>
    /// Gets the positional argument with the specified index. If the argument is not found, returns null.
    /// </summary>
    public ConsoleArgument? GetOrDefault(int index) => Arguments.FirstOrDefault(a => a.Index == index && a.Type == ArgumentType.Positional);

    public ConsoleArgument GetSwitch(string identifier, bool compareName = false) => Arguments.First(a => (a.Identifier == identifier || (compareName && a.Name == identifier)) && a.Type == ArgumentType.Switch);

    public ConsoleArgument? GetSwitchOrDefault(string identifier, bool compareName = false) => Arguments.FirstOrDefault(a => (a.Identifier == identifier || (compareName && a.Name == identifier)) && a.Type == ArgumentType.Switch);

    public object GetSwitchValue(string identifier, bool compareName = false) => GetSwitch(identifier, compareName).GetType();

    public object? GetSwitchValueOrDefault(string identifier, bool compareName = false) => GetSwitchOrDefault(identifier, compareName)?.GetValueOrNull();

    /// <summary>
    /// Gets the value of the positional argument with the specified name. If the argument or its value is not found, throws an exception.
    /// </summary>
    public object GetValue(string name) => Get(name).GetValue();

    /// <summary>
    /// Gets the value of the positional argument with the specified index. If the argument or its value is not found, throws an exception.
    /// </summary>
    public object GetValue(int index) => Get(index).GetValue();

    /// <summary>
    /// Gets the value of the positional argument with the specified name. If the argument is not found, returns null.
    /// </summary>
    public object? GetValueOrDefault(string name) => GetOrDefault(name)?.GetValueOrNull();

    /// <summary>
    /// Gets the value of the positional argument with the specified index. If the argument is not found, returns null.
    /// </summary>
    public object? GetValueOrDefault(int index) => GetOrDefault(index)?.GetValueOrNull();

    #endregion 
}