namespace Kokoabim.CommandLineInterface;

public enum ArgumentConstraints
{
    None,
    MustNotBeEmpty,
    MustNotBeWhiteSpace,
    MustNotBeEmptyOrWhiteSpace,
    MustBeInteger,
    MustBeUInteger,
    MustBeDouble,
    MustBeBoolean,
    MustConvertToType,
    FileMustExist,
    FileMustNotExist,
    DirectoryMustExist,
    DirectoryMustNotExist,
    MustBeUrl,
}