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
    public Type? ConstraintType { get; set; }
    public Action<ConsoleArgument>? CustomPreProcess { get; set; }
    public object? DefaultValue { get; set; }
    public bool Exists => GetValueOrDefault() is not null;
    public bool HasMultipleValues => _values.Count > 1;
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

    public string LongNameIdentifier => Type switch
    {
        ArgumentType.Positional => Name,
        ArgumentType.Option => $"{Identifier}:{Name}",
        ArgumentType.Switch => Identifier != Name ? $"{Identifier},{Name}" : Identifier,
        _ => throw new ArgumentOutOfRangeException(nameof(Type))
    };

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

    public ArgumentPreProcesses PreProcesses { get; set; }
    public bool TopLevelOnly { get; set; }
    public ArgumentType Type { get; set; }
    public object? Value => _values.FirstOrDefault();
    public IReadOnlyList<object> Values => _values;
    #endregion 

    public static readonly ConsoleArgument GlobalHelpSwitch = new("help", "help", "Show help", ArgumentType.Switch) { HideInArgumentsUseText = true };
    public static readonly ConsoleArgument GlobalVersionSwitch = new("version", "version", "Show version", ArgumentType.Switch) { HideInArgumentsUseText = true, TopLevelOnly = true };
    private readonly List<object> _values = [];

    public ConsoleArgument(
        string name,
        string? identifier = null,
        string? helpText = null,
        ArgumentType type = ArgumentType.Positional,
        bool isRequired = false,
        ArgumentConstraints constraints = ArgumentConstraints.None,
        object? defaultValue = null,
        ArgumentPreProcesses preProcesses = ArgumentPreProcesses.None,
        Action<ConsoleArgument>? customPreProcess = null,
        Type? constraintType = null)
    {
        DefaultValue = defaultValue;
        Constraints = constraints;
        ConstraintType = constraintType;
        CustomPreProcess = customPreProcess;
        HelpText = helpText;
        Identifier = identifier ?? "";
        IsRequired = isRequired;
        Name = name;
        PreProcesses = preProcesses;
        Type = type;

        if (Type != ArgumentType.Positional && string.IsNullOrWhiteSpace(identifier)) throw new ArgumentException($"{nameof(Identifier)} is required for non-positional arguments");
    }

    #region methods
    public void AddValue(object value) => _values.Add(value);

    public bool AsBool() => bool.Parse(AsString());

    public double AsDouble() => double.Parse(AsString());

    public int AsInt() => int.Parse(AsString());

    public string AsString() => GetValueOrDefault()!.ToString()!;

    public bool CheckConstraints() => Constraints switch
    {
        ArgumentConstraints.None => true,

        ArgumentConstraints.MustNotBeEmpty => GetValueOrDefault() is string s && !string.IsNullOrEmpty(s),
        ArgumentConstraints.MustNotBeWhiteSpace => GetValueOrDefault() is string s && !string.IsNullOrWhiteSpace(s),
        ArgumentConstraints.MustNotBeEmptyOrWhiteSpace => GetValueOrDefault() is string s && !string.IsNullOrWhiteSpace(s),

        ArgumentConstraints.MustBeInteger => GetValueOrDefault() is var o && o is int || int.TryParse(GetValueOrDefault() as string, out _),
        ArgumentConstraints.MustBeUInteger => GetValueOrDefault() is var o && o is uint || uint.TryParse(GetValueOrDefault() as string, out _),
        ArgumentConstraints.MustBeDouble => GetValueOrDefault() is var o && o is double || double.TryParse(GetValueOrDefault() as string, out _),
        ArgumentConstraints.MustBeBoolean => GetValueOrDefault() is var o && o is bool || bool.TryParse(GetValueOrDefault() as string, out _),

        ArgumentConstraints.FileMustExist => GetValueOrDefault() is string s && File.Exists(s),
        ArgumentConstraints.FileMustNotExist => GetValueOrDefault() is string s && !File.Exists(s),
        ArgumentConstraints.DirectoryMustExist => GetValueOrDefault() is string s && Directory.Exists(s),
        ArgumentConstraints.DirectoryMustNotExist => GetValueOrDefault() is string s && !Directory.Exists(s),

        ArgumentConstraints.MustBeUrl => GetValueOrDefault() is string s && Uri.TryCreate(s, UriKind.Absolute, out _),

        ArgumentConstraints.MustConvertToType => ConstraintType is not null && GetValueOrDefault() is string s &&
            ((ConstraintType.IsEnum && Enum.TryParse(ConstraintType, s, true, out _))
            || (!ConstraintType.IsEnum && Convert.ChangeType(s, ConstraintType) is not null)),

        _ => throw new ArgumentOutOfRangeException(nameof(Constraints))
    };

    public override int GetHashCode() => HashCode.Combine(Identifier, Name, Type);

    /// <summary>
    /// Gets the value of the argument.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown when the value is null and the default value is null.</exception>
    public object GetValue() => Value ?? DefaultValue ?? throw new ArgumentNullException(nameof(Value));

    public object? GetValueOrDefault() => Value ?? DefaultValue;

    public void PreProcess()
    {
        if (PreProcesses.HasFlag(ArgumentPreProcesses.ExpandEnvironmentVariables))
        {
            for (int i = 0; i < _values.Count; i++) if (_values[i] is string s)
                {
                    _values[i] = Environment.ExpandEnvironmentVariables(s).Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                }

            if (DefaultValue is string dv) DefaultValue = Environment.ExpandEnvironmentVariables(dv).Replace("~", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        if (PreProcesses.HasFlag(ArgumentPreProcesses.GetFullPath))
        {
            for (int i = 0; i < _values.Count; i++) if (_values[i] is string s) _values[i] = Path.GetFullPath(s);

            if (DefaultValue is string dv) DefaultValue = Path.GetFullPath(dv);
        }

        if (CustomPreProcess is not null) CustomPreProcess(this);
    }

    public override string? ToString() => GetValueOrDefault()?.ToString();
    #endregion 
}