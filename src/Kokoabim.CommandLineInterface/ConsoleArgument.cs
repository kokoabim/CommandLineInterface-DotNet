namespace Kokoabim.CommandLineInterface;

public class ConsoleArgument
{
    #region properties

    public string ArgumentUseText => Type switch
    {
        ArgumentType.Positional => Name,
        ArgumentType.Option or ArgumentType.Switch => $"-{NameIdentifier}",
        _ => throw new ArgumentOutOfRangeException(nameof(Type))
    };

    public ArgumentConstraints Constraints { get; set; }
    public object? DefaultValue { get; set; }
    public bool Exists => GetValueOrNull() is not null;
    public string? HelpText { get; set; }
    public bool HideInArgumentsUseText { get; set; }
    public int Id => GetHashCode();

    /// <summary>
    /// If the argument is an option or switch, the identifier (i.e. -i or --identifier).
    /// </summary>
    public string Identifier { get; set; }

    public int Index { get; set; } = -1;
    public bool IsBuiltIn => this == GlobalHelpSwitch || this == GlobalVersionSwitch;
    public bool IsDefaultValue => Value is null && DefaultValue is not null;
    public bool IsRequired { get; set; }

    /// <summary>
    /// If the argument is a positional argument, the name of the argument. If the argument is an option or switch, the name of the argument value.
    /// </summary>
    public string Name { get; set; }

    public string NameIdentifier => Type switch
    {
        ArgumentType.Positional => Name,
        ArgumentType.Option => $"{Identifier}:{Name}",
        ArgumentType.Switch => Identifier,
        _ => throw new ArgumentOutOfRangeException(nameof(Type))
    };

    public Func<ConsoleArgument, object?>? PreConstraintProcessing { get; set; }
    public ArgumentType Type { get; set; }
    public object? Value { get; set; }
    #endregion 

    public static readonly ConsoleArgument GlobalHelpSwitch = new("help", "help", "Show help", ArgumentType.Switch) { HideInArgumentsUseText = true };
    public static readonly ConsoleArgument GlobalVersionSwitch = new("version", "version", "Show version", ArgumentType.Switch) { HideInArgumentsUseText = true };

    public ConsoleArgument(
        string name,
        string? identifier = null,
        string? helpText = null,
        ArgumentType type = ArgumentType.Positional,
        bool isRequired = false,
        ArgumentConstraints constraints = ArgumentConstraints.None,
        object? defaultValue = null,
        Func<ConsoleArgument, object?>? preConstraintProcessing = null)
    {
        DefaultValue = defaultValue;
        Constraints = constraints;
        HelpText = helpText;
        Identifier = identifier ?? "";
        IsRequired = isRequired;
        Name = name;
        PreConstraintProcessing = preConstraintProcessing;
        Type = type;

        if (Type != ArgumentType.Positional && string.IsNullOrWhiteSpace(identifier)) throw new ArgumentException($"{nameof(Identifier)} is required for non-positional arguments");
    }

    public bool AsBool() => bool.Parse(AsString());

    public double AsDouble() => double.Parse(AsString());

    public int AsInt() => int.Parse(AsString());

    public string AsString() => GetValueOrNull()!.ToString()!;

    public bool CheckConstraints() => Constraints switch
    {
        ArgumentConstraints.None => true,

        ArgumentConstraints.MustNotBeEmpty => GetValueOrNull() is string s && !string.IsNullOrEmpty(s),
        ArgumentConstraints.MustNotBeWhiteSpace => GetValueOrNull() is string s && !string.IsNullOrWhiteSpace(s),
        ArgumentConstraints.MustNotBeEmptyOrWhiteSpace => GetValueOrNull() is string s && !string.IsNullOrWhiteSpace(s),

        ArgumentConstraints.MustBeInteger => int.TryParse(GetValueOrNull() as string, out _),
        ArgumentConstraints.MustBeDouble => double.TryParse(GetValueOrNull() as string, out _),
        ArgumentConstraints.MustBeBoolean => bool.TryParse(GetValueOrNull() as string, out _),

        ArgumentConstraints.FileMustExist => GetValueOrNull() is string s && File.Exists(s),
        ArgumentConstraints.FileMustNotExist => GetValueOrNull() is string s && !File.Exists(s),
        ArgumentConstraints.DirectoryMustExist => GetValueOrNull() is string s && Directory.Exists(s),
        ArgumentConstraints.DirectoryMustNotExist => GetValueOrNull() is string s && !Directory.Exists(s),

        _ => throw new ArgumentOutOfRangeException(nameof(Constraints))
    };

    public override int GetHashCode() => HashCode.Combine(Identifier, Name, Type);

    /// <summary>
    /// Gets the value of the argument.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the value is null and the default value is null.</exception>
    public object GetValue() => Value ?? DefaultValue ?? throw new ArgumentNullException(nameof(Value));

    public object? GetValueOrNull() => Value ?? DefaultValue;

    public override string? ToString() => GetValueOrNull()?.ToString();
}