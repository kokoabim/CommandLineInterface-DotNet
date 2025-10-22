namespace Kokoabim.CommandLineInterface.Tests;

public class TestConsoleAppTests
{
    [Fact]
    public async Task WithArgsAsync()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithArgumentsAsync(["World"]);

        // assert
        Assert.Equal(0, actual);
        Assert.Equal("Hello, World!\n", debugWriter.Output);
    }

    [Fact]
    public async Task WithArgs_ButNoArgsAsync()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithArgumentsAsync([]);

        // assert
        Assert.Equal(1, actual);
        Assert.Equal("Missing required argument (use --help switch to view help): yourName - The name of the user\n", debugWriter.Output);
    }

    [Fact]
    public async Task WithArgs_ButNoBadArgAsync()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithArgumentsAsync([""]);

        // assert
        Assert.Equal(1, actual);
        Assert.Equal("Bad argument (use --help switch to view help): yourName - The name of the user - MustNotBeEmptyOrWhiteSpace: \n", debugWriter.Output);
    }

    [Fact]
    public async Task WithArgs_HelpSwitchAsync()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithArgumentsAsync(["--help"]);

        // assert
        Assert.Equal(0, actual);
        Assert.Equal("TestConsoleApp.RunWithArgumentsAsync (v15.0)\nUsage: testhost yourName\n\nSwitches:\n help - Show help\n version - Show version\n\nArguments:\n yourName - The name of the user\n", debugWriter.Output);
    }

    [Fact]
    public async Task WithCommandAsync()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithDoubleNumberCommandAsync(["double", "2"]);

        // assert
        Assert.Equal(0, actual);
        Assert.Equal("4", debugWriter.Output);
    }

    [Fact]
    public async Task WithCommand_ButCliArgsAsync()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithDoubleNumberCommandAsync([]);

        // assert
        Assert.Equal(0, actual);
        Assert.Equal("TestConsoleApp.RunWithDoubleNumberCommandAsync (v15.0)\nUsage: testhost command [arguments]\n\nCommands:\n double - Double a number\n\nSwitches:\n help - Show help\n version - Show version\n", debugWriter.Output);
    }

    [Fact]
    public async Task WithCommand_ButCmdArgsAsync()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithDoubleNumberCommandAsync(["double"]);

        // assert
        Assert.Equal(1, actual);
        Assert.Equal("Missing required argument (use --help switch to view help): number - The number to double\n", debugWriter.Output);
    }

    [Fact]
    public async Task WithCommand_WithCmdButHelpOptionAsync()
    {
        // arrange
        var target = new TestConsoleApp();
        var debugWriter = DebugWriter.Create();

        // act
        var actual = await target.RunWithDoubleNumberCommandAsync(["double", "--help"]);

        // assert
        Assert.Equal(0, actual);
        Assert.Equal("Double a number\nCommand: double number\n\nSwitches:\n help - Show help\n\nArguments:\n number - The number to double\n", debugWriter.Output);
    }
}