namespace Kokoabim.CommandLineInterface;

public enum ArgumentConstraints
{
    None,
    MustNotBeEmpty,
    MustNotBeWhiteSpace,
    MustNotBeEmptyOrWhiteSpace,
    MustBeInteger,
    MustBeDouble,
    MustBeBoolean,
    FileMustExist,
    FileMustNotExist,
    DirectoryMustExist,
    DirectoryMustNotExist
}