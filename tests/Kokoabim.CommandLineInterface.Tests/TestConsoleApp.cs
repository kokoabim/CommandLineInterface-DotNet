namespace Kokoabim.CommandLineInterface.Tests;

public class TestConsoleApp
{
    public async Task<int> RunWithArgumentsAsync(string[] args)
    {
        var consoleApp = new ConsoleApp(
            [new ConsoleArgument("yourName", isRequired: true, constraints: ArgumentConstraints.MustNotBeEmptyOrWhiteSpace, helpText: "The name of the user")],
            titleText: $"{nameof(TestConsoleApp)}.{nameof(RunWithArgumentsAsync)}",
            syncFunction: context =>
            {
                var yourName = context.GetValueOrDefault("yourName")!;
                Console.WriteLine($"Hello, {yourName}!");
                return 0;
            }
        );

        return await consoleApp.RunAsync(args);
    }

    public async Task<int> RunWithDoubleNumberCommandAsync(string[] args)
    {
        var consoleApp = new ConsoleApp([
            new ConsoleCommand("double", titleText: "Double a number", arguments: [new ConsoleArgument("number", isRequired: true, helpText: "The number to double")], syncFunction: context =>
            {
                var number = context.GetValueOrDefault("number") is string numberString ? int.Parse(numberString) : 0;
                Console.WriteLine(number * 2);
                return 0;
            }),
        ],
        titleText: $"{nameof(TestConsoleApp)}.{nameof(RunWithDoubleNumberCommandAsync)}");

        return await consoleApp.RunAsync(args);
    }
}