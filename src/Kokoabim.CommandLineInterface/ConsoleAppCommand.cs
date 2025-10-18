using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;

namespace Kokoabim.CommandLineInterface;

public interface IConsoleAppCommand
{
    IReadOnlyList<ConsoleArgument> Arguments { get; }
    Func<ConsoleContext, Task<int>>? AsyncFunction { get; set; }
    bool DoNotCheckArgumentsConstraints { get; set; }
    string Name { get; set; }
    Func<ConsoleContext, int>? SyncFunction { get; set; }
    string? TitleText { get; set; }

    void AddArgument(ConsoleArgument argument);
    void AddArguments(IEnumerable<ConsoleArgument> arguments);
    string HelpText();
}

public abstract class ConsoleAppCommand
{
    public IReadOnlyList<ConsoleArgument> Arguments => _arguments;
    public Func<ConsoleContext, Task<int>>? AsyncFunction { get; set; }
    public bool DoNotCheckArgumentsConstraints { get; set; }
    public string Name { get; set; } = default!;
    public Func<ConsoleContext, int>? SyncFunction { get; set; }
    public string? TitleText { get; set; }
    internal string? ArgumentsUseText => GenerateArgumentsUseText();
    internal string? CommandUseText => $"{Name}{(ArgumentsUseText is not null ? $" {ArgumentsUseText}" : null)}";
    protected List<ConsoleCommand> InternalCommands { get; set; } = [];

    private readonly List<ConsoleArgument> _arguments = [ConsoleArgument.GlobalHelpSwitch, ConsoleArgument.GlobalVersionSwitch];
    private int _maxPositionalIndex = -1;
    private static readonly Regex _optionOrSwitchIdentifierRegex = new(@"^-(?<id>[^:=-])([:=](?<value>.*))?$", RegexOptions.Compiled);
    private static readonly Regex _optionOrSwitchNameRegex = new(@"^--(?<name>[^:=]+)([:=](?<value>.*))?$", RegexOptions.Compiled);

    public void AddArgument(ConsoleArgument argument)
    {
        if (argument.Type == ArgumentType.Positional) argument.Index = ++_maxPositionalIndex;
        _arguments.Add(argument);
    }

    public void AddArguments(IEnumerable<ConsoleArgument> arguments)
    {
        foreach (var arg in arguments) AddArgument(arg);
    }

    public abstract string HelpText();

    /// <summary>
    /// Checks if an option exists in the defined arguments (in code) and the arguments passed to the console application.
    /// </summary>
    internal bool DoesOptionExist(string identifier, string[] args, [NotNullWhen(true)] out ConsoleArgument? option)
    {
        option = args.Any(a => a == "-" + identifier || a == "--" + identifier) ? Arguments.FirstOrDefault(a => a.Type == ArgumentType.Option && a.Identifier == identifier) : null;
        return option is not null;
    }

    /// <summary>
    /// Checks if a switch exists in the defined arguments (in code) and the arguments passed to the console application.
    /// </summary>
    internal bool DoesSwitchExist(string identifier, string[] args) =>
        args.Any(a => a == "-" + identifier || a == "--" + identifier) && Arguments.FirstOrDefault(a => a.Type == ArgumentType.Switch && a.Identifier == identifier) is ConsoleArgument arg;

    protected void AddArgumentsToHelpText(StringBuilder sb, bool includeTopLevelOnly)
    {
        var switches = Arguments.Where(a => a.Type == ArgumentType.Switch && (!a.TopLevelOnly || includeTopLevelOnly));
        if (switches.Any())
        {
            _ = sb.AppendLine("\nSwitches:");
            foreach (var arg in switches) _ = sb.AppendLine($" {arg.LongNameIdentifier} - {arg.HelpText}");
        }

        var options = Arguments.Where(static a => a.Type == ArgumentType.Option);
        if (options.Any())
        {
            _ = sb.AppendLine("\nOptions:");
            foreach (var arg in options) _ = sb.AppendLine($" {arg.NameIdentifier} - {arg.HelpText}");
        }

        var positionals = Arguments.Where(static a => a.Type == ArgumentType.Positional);
        if (positionals.Any())
        {
            _ = sb.AppendLine("\nArguments:");
            foreach (var arg in positionals.OrderBy(static a => a.Index)) _ = sb.AppendLine($" {arg.NameIdentifier} - {arg.HelpText}");
        }
    }

    protected string? GenerateArgumentsUseText()
    {
        if (InternalCommands.Count > 0) return "command [arguments]";
        else if (!Arguments.Any()) return null;

        var sb = new StringBuilder();
        foreach (var arg in Arguments.Where(static a => a.Type == ArgumentType.Switch && !a.HideInArgumentsUseText)) _ = sb.Append(arg.ArgumentUseText).Append(' ');
        foreach (var arg in Arguments.Where(static a => a.Type == ArgumentType.Option && !a.HideInArgumentsUseText)) _ = sb.Append(arg.ArgumentUseText).Append(' ');
        foreach (var arg in Arguments.Where(static a => a.Type == ArgumentType.Positional).OrderBy(static a => a.Index)) _ = sb.Append($"{(arg.IsRequired ? "" : "[")}{arg.ArgumentUseText}{(arg.IsRequired ? "" : "]")}").Append(' ');
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
                Console.Error.WriteLine("Bad arguments (use --help switch to view help):");
                foreach (var arg in badArguments) Console.Error.WriteLine($" {arg.NameIdentifier} - {arg.HelpText} - {arg.Constraints}: {arg.GetValueOrDefault() ?? "(null)"}");
            }

            if (missingArguments.Any())
            {
                showHelpText = false;
                Console.Error.WriteLine("Missing required arguments (use --help switch to view help):");
                foreach (var arg in missingArguments) Console.Error.WriteLine($" {arg.NameIdentifier} - {arg.HelpText}");
            }

            if (unknownArguments.Any())
            {
                showHelpText = false;
                Console.Error.WriteLine($"Unknown arguments (use --help switch to view help): {string.Join(", ", unknownArguments)}");
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
                bool? matchedByName = null;
                var match = _optionOrSwitchNameRegex.Match(arg);
                if (!match.Success)
                {
                    match = _optionOrSwitchIdentifierRegex.Match(arg);
                    if (match.Success) matchedByName = false;
                }
                else matchedByName = true;

                if (match.Success && matchedByName.HasValue)
                {
                    var argNameOrId = match.Groups[matchedByName == true ? "name" : "id"].Value;
                    var hasValue = match.Groups["value"].Success;

                    argument = Arguments.FirstOrDefault(a =>
                        ((hasValue && a.Type == ArgumentType.Option) || (!hasValue && a.Type == ArgumentType.Switch))
                        && ((matchedByName.Value && a.Name == argNameOrId) || (!matchedByName.Value && a.Identifier == argNameOrId)));

                    if (argument is not null)
                    {
                        argument.AddValue(hasValue ? match.Groups["value"].Value : true);

                        argument.PreProcess();

                        if (!DoNotCheckArgumentsConstraints && argument.Constraints != ArgumentConstraints.None && !argument.CheckConstraints())
                        {
                            badArgs.Add(argument);
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

            argument.AddValue(arg);

            argument.PreProcess();

            if (!DoNotCheckArgumentsConstraints && argument.Constraints != ArgumentConstraints.None && !argument.CheckConstraints())
            {
                badArgs.Add(argument);
                continue;
            }
        }

        foreach (var nonRequiredMissingArgThatIsDefaultValue in Arguments.Where(a => !a.IsBuiltIn && !a.IsRequired && a.IsDefaultValue))
        {
            // these are not processed above
            nonRequiredMissingArgThatIsDefaultValue.PreProcess();

            if (!DoNotCheckArgumentsConstraints && nonRequiredMissingArgThatIsDefaultValue.Constraints != ArgumentConstraints.None && !nonRequiredMissingArgThatIsDefaultValue.CheckConstraints())
            {
                badArgs.Add(nonRequiredMissingArgThatIsDefaultValue);
            }
        }

        badArguments = badArgs;
        missingArguments = Arguments.Where(a => a.IsRequired && !a.Exists && badArgs.All(b => b.Id != a.Id));
        unknownArguments = unknownArgs;

        return !(badArguments.Any() || missingArguments.Any() || unknownArguments.Any());
    }
}