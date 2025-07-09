namespace Kokoabim.CommandLineInterface.Tests;

public class TestConsoleAppTests
{
    [Fact]
    public async Task WithArgs()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithArguments(["World"]);

        // assert
        Assert.Equal(0, actual);
        Assert.Equal("Hello, World!\n", debugWriter.Output);
    }

    [Fact]
    public async Task WithArgs_ButNoArgs()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithArguments([]);

        // assert
        Assert.Equal(1, actual);
        Assert.Equal("Missing required arguments (use --help switch to view help):\n yourName - The name of the user\n", debugWriter.Output);
    }

    [Fact]
    public async Task WithArgs_ButNoBadArg()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithArguments([""]);

        // assert
        Assert.Equal(1, actual);
        Assert.Equal("Bad arguments (use --help switch to view help):\n yourName - The name of the user - MustNotBeEmptyOrWhiteSpace: \n", debugWriter.Output);
    }

    [Fact]
    public async Task WithArgs_HelpSwitch()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithArguments(["--help"]);

        // assert
        Assert.Equal(1, actual);
        Assert.Equal("TestConsoleApp.RunWithArguments\nUsage: testhost yourName\n\nSwitches:\n help - Show help\n version - Show version\n\nArguments:\n yourName - The name of the user\n", debugWriter.Output);
    }

    [Fact]
    public async Task WithCommand()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithDoubleNumberCommand(["double", "2"]);

        // assert
        Assert.Equal(0, actual);
        Assert.Equal("4", debugWriter.Output);
    }

    [Fact]
    public async Task WithCommand_ButCliArgs()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithDoubleNumberCommand([]);

        // assert
        Assert.Equal(1, actual);
        Assert.Equal("TestConsoleApp.RunWithDoubleNumberCommand\nUsage: testhost command [arguments]\n\nCommands:\n double - Double a number\n\nSwitches:\n help - Show help\n\nSwitches:\n version - Show version\n", debugWriter.Output);
    }

    [Fact]
    public async Task WithCommand_ButCmdArgs()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithDoubleNumberCommand(["double"]);

        // assert
        Assert.Equal(1, actual);
        Assert.Equal("Missing required arguments (use --help switch to view help):\n number - The number to double\n", debugWriter.Output);
    }

    [Fact]
    public async Task WithCommand_WithCmdButHelpOption()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithDoubleNumberCommand(["double", "--help"]);

        // assert
        Assert.Equal(1, actual);
        Assert.Equal("Double a number\nCommand: double number\n\nSwitches:\n help - Show help\n version - Show version\n\nArguments:\n number - The number to double\n", debugWriter.Output);
    }
}