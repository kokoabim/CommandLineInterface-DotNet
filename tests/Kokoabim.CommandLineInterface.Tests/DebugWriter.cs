using System.Diagnostics;
using System.Text;

namespace Kokoabim.CommandLineInterface.Tests;

public class DebugWriter : TextWriter
{
    #region properties
    public override Encoding Encoding => Encoding.UTF8;
    public string[] Lines => _stringBuilder.ToString().Split('\n');
    public string Output => _stringBuilder.ToString();
    #endregion 

    private readonly StringBuilder _stringBuilder = new();

    public static DebugWriter Create()
    {
        var debugWriter = new DebugWriter();
        Console.SetError(debugWriter);
        Console.SetOut(debugWriter);
        return debugWriter;
    }

    public override void Write(string? value)
    {
        _stringBuilder.Append(value);
        Debug.Write(value);
        base.Write(value);
    }
}