using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Kokoabim.CommandLineInterface;

public interface IConsoleAppCommand
{
    IEnumerable<ConsoleArgument> Arguments { get; }
    Func<ConsoleContext, Task<int>>? AsyncFunction { get; set; }
    bool DoNotCheckArgumentsConstraints { get; set; }
    string Name { get; set; }
    Func<ConsoleContext, int>? SyncFunction { get; set; }
    string? TitleText { get; set; }

    void AddArgument(ConsoleArgument argument);
    void AddArguments(IEnumerable<ConsoleArgument> arguments);
}

public abstract class ConsoleAppCommand
{
    #region properties
    public IEnumerable<ConsoleArgument> Arguments => _arguments;
    public Func<ConsoleContext, Task<int>>? AsyncFunction { get; set; }
    public bool DoNotCheckArgumentsConstraints { get; set; }
    public string Name { get; set; } = default!;
    public Func<ConsoleContext, int>? SyncFunction { get; set; }
    public string? TitleText { get; set; }
    internal string? ArgumentsUseText => GenerateArgumentsUseText();
    internal string? CommandUseText => $"{Name}{(ArgumentsUseText is not null ? $" {ArgumentsUseText}" : null)}";
    #endregion 

    #region fields
    protected readonly List<ConsoleCommand> _commands = [];
    private readonly List<ConsoleArgument> _arguments = [ConsoleArgument.GlobalHelpSwitch, ConsoleArgument.GlobalVersionSwitch];
    private int _maxPositionalIndex = -1;
    private static readonly Regex _optionOrSwitchRegex = new(@"^--?(?<name>[^:=]+)([:=](?<value>.*))?$", RegexOptions.Compiled);
    #endregion 

    #region methods

    public void AddArgument(ConsoleArgument argument)
    {
        if (argument.Type == ArgumentType.Positional) argument.Index = ++_maxPositionalIndex;
        _arguments.Add(argument);
    }

    public void AddArguments(IEnumerable<ConsoleArgument> arguments)
    {
        foreach (var arg in arguments) AddArgument(arg);
    }

    internal abstract string HelpText();

    protected void AddArgumentsToHelpText(StringBuilder sb)
    {
        var switches = Arguments.Where(a => a.Type == ArgumentType.Switch);
        if (switches.Any())
        {
            sb.AppendLine("\nSwitches:");
            foreach (var arg in switches) sb.AppendLine($" {arg.NameIdentifier} - {arg.HelpText}");
        }

        var options = Arguments.Where(a => a.Type == ArgumentType.Option);
        if (options.Any())
        {
            sb.AppendLine("\nOptions:");
            foreach (var arg in options) sb.AppendLine($" {arg.NameIdentifier} - {arg.HelpText}");
        }

        var positionals = Arguments.Where(a => a.Type == ArgumentType.Positional);
        if (positionals.Any())
        {
            sb.AppendLine("\nArguments:");
            foreach (var arg in positionals.OrderBy(a => a.Index)) sb.AppendLine($" {arg.NameIdentifier} - {arg.HelpText}");
        }
    }

    /// <summary>
    /// Checks if an option exists in the defined arguments (in code) and the arguments passed to the console application.
    /// </summary>
    protected bool DoesOptionExist(string identifier, string[] args, [NotNullWhen(true)] out ConsoleArgument? option)
    {
        option = args.Any(a => a == "-" + identifier || a == "--" + identifier) ? Arguments.FirstOrDefault(a => a.Type == ArgumentType.Option && a.Identifier == identifier) : null;
        return option is not null;
    }

    /// <summary>
    /// Checks if a switch exists in the defined arguments (in code) and the arguments passed to the console application.
    /// </summary>
    protected bool DoesSwitchExist(string identifier, string[] args) =>
        args.Any(a => a == "-" + identifier || a == "--" + identifier) && Arguments.FirstOrDefault(a => a.Type == ArgumentType.Switch && a.Identifier == identifier) is ConsoleArgument arg;

    protected string? GenerateArgumentsUseText()
    {
        if (_commands.Count > 0) return "command [arguments]";
        else if (!Arguments.Any()) return null;

        var sb = new StringBuilder();
        foreach (var arg in Arguments.Where(a => a.Type == ArgumentType.Switch && !a.HideInArgumentsUseText)) sb.Append(arg.ArgumentUseText).Append(' ');
        foreach (var arg in Arguments.Where(a => a.Type == ArgumentType.Option && !a.HideInArgumentsUseText)) sb.Append(arg.ArgumentUseText).Append(' ');
        foreach (var arg in Arguments.Where(a => a.Type == ArgumentType.Positional).OrderBy(a => a.Index)) sb.Append(arg.ArgumentUseText).Append(' ');
        return sb.Length > 0 ? sb.ToString()[..^1] : null;
    }

    protected bool ProcessCliArguments(string[] args)
    {
        if (DoesSwitchExist("help", args))
        {
            Console.WriteLine(HelpText());
            return false;
        }

        if (!TryProcessCliArguments(args, out IEnumerable<ConsoleArgument> badArguments, out IEnumerable<ConsoleArgument> missingArguments, out IEnumerable<string> unknownArguments))
        {
            var showHelpText = true;
            if (badArguments.Any())
            {
                showHelpText = false;
                Console.Error.WriteLine("Bad arguments:");
                foreach (var arg in badArguments) Console.Error.WriteLine($" {arg.NameIdentifier} - {arg.HelpText} - {arg.Constraints}");
            }

            if (missingArguments.Any())
            {
                showHelpText = false;
                Console.Error.WriteLine("Missing required arguments:");
                foreach (var arg in missingArguments) Console.Error.WriteLine($" {arg.NameIdentifier} - {arg.HelpText}");
            }

            if (unknownArguments.Any())
            {
                showHelpText = false;
                Console.Error.WriteLine($"Unknown arguments: {string.Join(", ", unknownArguments)}");
            }

            if (showHelpText) Console.WriteLine(HelpText());

            return false;
        }

        return true;
    }

    protected bool TryProcessCliArguments(string[] args, out IEnumerable<ConsoleArgument> badArguments, out IEnumerable<ConsoleArgument> missingArguments, out IEnumerable<string> unknownArguments)
    {
        var endOfOptionsAndSwitches = false;
        int positionIndex = -1;

        List<ConsoleArgument> badArgs = [];
        List<ConsoleArgument> missingArgs = [];
        List<string> unknownArgs = [];

        foreach (var arg in args)
        {
            if (arg == "--")
            {
                endOfOptionsAndSwitches = true;
                continue;
            }

            ConsoleArgument? argument;

            if (!endOfOptionsAndSwitches && arg.Length > 0 && arg[0] == '-')
            {
                var match = _optionOrSwitchRegex.Match(arg);
                if (match.Success)
                {
                    var identifier = match.Groups["name"].Value;
                    object? value;

                    if (match.Groups["value"].Success) // option
                    {
                        argument = Arguments.FirstOrDefault(a => a.Type == ArgumentType.Option && a.Identifier == identifier);
                        value = match.Groups["value"].Value;
                    }
                    else // switch
                    {
                        argument = Arguments.FirstOrDefault(a => a.Type == ArgumentType.Switch && a.Identifier == identifier);
                        value = true;
                    }

                    if (argument is not null)
                    {
                        argument.Value = value;

                        if (!DoNotCheckArgumentsConstraints && argument.Constraints != ArgumentConstraints.None)
                        {
                            if (argument.PreConstraintProcessing is not null) argument.Value = argument.PreConstraintProcessing(argument);
                            if (!argument.CheckConstraints()) badArgs.Add(argument);
                        }

                        continue; // do not fall through if in this block
                    }
                }

                unknownArgs.Add(arg);
            }

            // positional

            positionIndex++;

            if (positionIndex > _maxPositionalIndex)
            {
                unknownArgs.Add(arg);
                continue;
            }

            argument = Arguments.FirstOrDefault(a => a.Type == ArgumentType.Positional && a.Index == positionIndex);
            if (argument is null)
            {
                unknownArgs.Add(arg);
                continue;
            }

            argument.Value = arg;

            if (!DoNotCheckArgumentsConstraints && argument.Constraints != ArgumentConstraints.None)
            {
                if (argument.PreConstraintProcessing is not null) argument.Value = argument.PreConstraintProcessing(argument);
                if (!argument.CheckConstraints()) badArgs.Add(argument);
                continue;
            }
        }

        badArguments = badArgs;
        missingArguments = Arguments.Where(a => a.IsRequired && !a.Exists && badArgs.All(b => b.Id != a.Id));
        unknownArguments = unknownArgs;

        return !(badArguments.Any() || missingArguments.Any() || unknownArguments.Any());
    }

    #endregion 
}