using System.Text;

namespace Kokoabim.CommandLineInterface;

public interface IConsoleApp : IConsoleEvents, IConsoleAppCommand
{
    IReadOnlyList<ConsoleCommand> Commands { get; }
    string? DefaultCommandName { get; set; }
    string Version { get; set; }

    void AddCommand(ConsoleCommand command);
    void AddCommands(IEnumerable<ConsoleCommand> commands);
    /// <summary>
    /// Run the console application.
    /// </summary>
    Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default);
}

public class ConsoleApp : ConsoleAppCommand, IConsoleApp
{
    public IReadOnlyList<ConsoleCommand> Commands => InternalCommands;
    public string? DefaultCommandName { get; set; }
    public bool HandleCancelEvent { get; set; } = true;
    public Func<ConsoleCancelEventArgs, bool>? OnCancel { get; set; }
    public Func<bool>? OnTerminate { get; set; }
    public string Version { get; set; }

    private int _cancelEventCount = 0;
    private CancellationTokenSource? _cancellationTokenSource;
    private ConsoleCommand? _command;
    private bool _hasProcessedCliArguments;
    private bool _isCommandBased;

    /// <summary>
    /// Create a console application.
    /// </summary>
    public ConsoleApp(string? titleText = null, string? version = null)
    {
        Name = EntryAssembly.Name;
        TitleText = titleText;
        Version = version ?? EntryAssembly.Version;

        Console.CancelKeyPress += ConsoleCancelEventHandler;
    }

    /// <summary>
    /// Create a console application.
    /// </summary>
    public ConsoleApp(
        IEnumerable<ConsoleArgument> arguments,
        Func<ConsoleContext, Task<int>>? asyncFunction = null,
        Func<ConsoleContext, int>? syncFunction = null,
        string? titleText = null, string? version = null)
        : this(titleText, version)
    {
        AddArguments(arguments);
        AsyncFunction = asyncFunction;
        SyncFunction = syncFunction;
    }

    /// <summary>
    /// Create a command-based console application.
    /// </summary>
    public ConsoleApp(IEnumerable<ConsoleCommand> commands, string? titleText = null, string? version = null) : this(titleText, version)
    {
        AddCommands(commands);
    }

    #region methods

    public void AddCommand(ConsoleCommand command)
    {
        _isCommandBased = true;
        InternalCommands.Add(command);
    }

    public void AddCommands(IEnumerable<ConsoleCommand> commands)
    {
        _isCommandBased = true;
        InternalCommands.AddRange(commands);
    }

    public static bool GetBooleanInput(string message, bool defaultValue = false)
    {
        var yesNo = defaultValue ? "Y/n" : "y/N";
        Console.Write($"{message}? [{yesNo}] ");

        var input = Console.ReadLine()?.Trim().ToLower();
        return input == "y" || input == "yes" || string.IsNullOrWhiteSpace(input) && defaultValue;
    }

    public static double GetDoubleInput(string message, double min = double.MinValue, double max = double.MaxValue, double? defaultValue = null)
    {
        double input;

        Console.Write($"{message}: {(defaultValue is not null ? $"[{defaultValue}] " : null)}");
        while (!double.TryParse(Console.ReadLine(), out input) || input < min || input > max)
        {
            if (defaultValue.HasValue) return defaultValue.Value;
            Console.Write($"Invalid number. Enter double between {min} and {max}: ");
        }

        return input;
    }

    public static int GetIntegerInput(string message, int min = int.MinValue, int max = int.MaxValue, int? defaultValue = null)
    {
        int input;

        Console.Write($"{message}: {(defaultValue is not null ? $"[{defaultValue}] " : null)}");
        while (!int.TryParse(Console.ReadLine(), out input) || input < min || input > max)
        {
            if (defaultValue.HasValue) return defaultValue.Value;
            Console.Write($"Invalid number. Enter integer between {min} and {max}: ");
        }

        return input;
    }

    public static string GetStringInput(string message)
    {
        Console.Write($"{message}: ");
        return Console.ReadLine()!;
    }

    public override string HelpText()
    {
        var sb = new StringBuilder();

        if (TitleText is not null) sb.AppendLine($"{TitleText} (v{Version})");
        sb.AppendLine($"Usage: {CommandUseText}");

        if (_isCommandBased && Commands.Any())
        {
            sb.AppendLine("\nCommands:");
            foreach (var cmd in Commands) sb.AppendLine($" {cmd.Name}{(cmd.TitleText is not null ? $" - {cmd.TitleText}" : null)}");

            foreach (var arg in Arguments.Where(a => a.Type == ArgumentType.Switch))
            {
                sb.AppendLine("\nSwitches:");
                sb.AppendLine($" {arg.LongNameIdentifier} - {arg.HelpText}");
            }

            foreach (var arg in Arguments.Where(a => a.Type == ArgumentType.Option))
            {
                sb.AppendLine("\nOptions:");
                sb.AppendLine($" {arg.NameIdentifier} - {arg.HelpText}");
            }
        }
        else if (!_isCommandBased && Arguments.Any())
        {
            AddArgumentsToHelpText(sb);
        }

        return sb.ToString()[..^1];
    }

    /// <summary>
    /// Run the console application
    /// </summary>
    public async Task<int> RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        if (_isCommandBased)
        {
            if (args.Length == 0)
            {
                if (DefaultCommandName is null)
                {
                    Console.WriteLine(HelpText());
                    return 0;
                }

                args = [DefaultCommandName];
            }

            _command = Commands.FirstOrDefault(c => c.Name == args[0]);
        }

        if (!_isCommandBased || _command is null)
        {
            if (DoesSwitchExist("help", args))
            {
                Console.WriteLine(HelpText());
                return 0;
            }
            else if (DoesSwitchExist("version", args))
            {
                Console.WriteLine($"{Name}{(TitleText is not null ? $" — {TitleText}" : null)} (v{Version})");
                return 0;
            }
        }

        if (_isCommandBased)
        {
            if (_command is null)
            {
                Console.Error.WriteLine($"Unknown command: {args[0]}");
                return 1;
            }
            else if (_command.DoesSwitchExist("help", [.. args.Skip(1)]))
            {
                Console.WriteLine(_command.HelpText());
                return 0;
            }
        }

        try
        {
            if (ProcessArguments(args)) return await RunFunctionAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Unhandled task cancellation");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled error {ex.GetType().Name}: {ex.Message}");
        }

        return 1;
    }

    internal bool ProcessArguments(string[] args)
    {
        _hasProcessedCliArguments = true;
        return _isCommandBased ? ProcessCommandsArguments(args) : ProcessConsoleAppArguments(args);
    }

    internal async Task<int> RunFunctionAsync(CancellationToken cancellationToken = default)
    {
        if (!_hasProcessedCliArguments) throw new InvalidOperationException("Arguments not processed");

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        return _isCommandBased ? await RunCommandAsync(_cancellationTokenSource.Token) : await RunWithArgumentsAsync(_cancellationTokenSource.Token);
    }

    private void ConsoleCancelEventHandler(object? sender, ConsoleCancelEventArgs e)
    {
        if (!HandleCancelEvent) return;

        if (++_cancelEventCount > 1)
        {
            if (OnTerminate?.Invoke() == false) return;

            Console.Error.WriteLine("Terminating...");
            Environment.Exit(1);
        }

        e.Cancel = true;

        if (OnCancel?.Invoke(e) == false) return;

        Console.Error.WriteLine("Canceling...");
        _cancellationTokenSource?.Cancel();
    }

    private bool ProcessCommandsArguments(string[] args) => _command!.ProcessArguments([.. args.Skip(1)]);

    private bool ProcessConsoleAppArguments(string[] args) => ProcessCliArguments(args);

    private async Task<int> RunCommandAsync(CancellationToken cancellationToken = default)
    {
        if (_command is null) throw new InvalidOperationException("Command not set");

        return await _command!.RunFunctionAsync(this, cancellationToken);
    }

    private async Task<int> RunWithArgumentsAsync(CancellationToken cancellationToken = default)
    {
        if (AsyncFunction is null && SyncFunction is null) throw new InvalidOperationException("Console function not set");

        try
        {
            var context = new ConsoleContext(this, this, cancellationToken);
            return AsyncFunction is not null ? await AsyncFunction(context) : SyncFunction!(context);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled console error {ex.GetType().Name}: {ex.Message}");
            return 1;
        }
    }

    #endregion 
}