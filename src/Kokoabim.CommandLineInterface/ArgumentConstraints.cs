namespace Kokoabim.CommandLineInterface;

public enum ArgumentConstraints
{
    None,
    NotEmpty,
    NotWhiteSpace,
    NotEmptyOrWhiteSpace,
    IsInteger,
    IsDouble,
    IsBoolean,
    FileExists,
    FileDoesNotExist,
    DirectoryExists,
    DirectoryDoesNotExist
}
