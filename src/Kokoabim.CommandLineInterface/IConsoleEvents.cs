namespace Kokoabim.CommandLineInterface;

public interface IConsoleEvents
{
    /// <summary>
    /// Indicates whether to handle the cancel event (Ctrl+C). Default is true.
    /// </summary>
    bool HandleCancelEvent { get; set; }

    /// <summary>
    /// Event handler for the cancel event (Ctrl+C). Returning true allows the console app to perform the default action; returning false prevents the default action.
    /// </summary>
    Func<ConsoleCancelEventArgs, bool>? OnCancel { get; set; }

    /// <summary>
    /// Event handler for the terminate event (Ctrl+C pressed twice). Returning true allows the console app to perform the default action; returning false prevents the default action.
    /// </summary>
    Func<bool>? OnTerminate { get; set; }
}
