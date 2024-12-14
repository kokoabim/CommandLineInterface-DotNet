using System.Text;

namespace Kokoabim.CommandLineInterface;

public class ConsoleCommand : ConsoleAppCommand, IConsoleAppCommand
{
    public ConsoleCommand(
        string name,
        IEnumerable<ConsoleArgument>? arguments = null,
        Func<ConsoleContext, Task<int>>? asyncFunction = null,
        Func<ConsoleContext, int>? syncFunction = null,
        string? titleText = null)
    {
        if (arguments is not null) AddArguments(arguments);

        AsyncFunction = asyncFunction;
        SyncFunction = syncFunction;
        Name = name;
        TitleText = titleText;
    }

    #region methods

    internal override string HelpText()
    {
        var sb = new StringBuilder();

        if (TitleText is not null) sb.AppendLine(TitleText);
        sb.AppendLine($"Command: {CommandUseText}");

        AddArgumentsToHelpText(sb);

        return sb.ToString()[..^1];
    }

    internal bool ProcessArguments(string[] args) => ProcessCliArguments(args);

    internal async Task<int> RunFunctionAsync(CancellationToken cancellationToken = default)
    {
        if (AsyncFunction is null && SyncFunction is null) throw new InvalidOperationException("Command function not set");

        try
        {
            var context = new ConsoleContext(Arguments, cancellationToken);
            return AsyncFunction is not null ? await AsyncFunction(context) : SyncFunction!(context);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Unhandled command error: {ex.Message}");
            return 1;
        }
    }

    #endregion 
}