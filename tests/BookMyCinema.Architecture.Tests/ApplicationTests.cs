using System.Reflection;
using System.Text.RegularExpressions;
using ArchUnitNET.xUnitV3;
using BookMyCinema.Application;
using BookMyCinema.Application.Common.Abstractions;
using BookMyCinema.Application.Common.Abstractions.Messaging;
using FluentValidation;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ServiceCollectionExtensions = BookMyCinema.Application.ServiceCollectionExtensions;

namespace BookMyCinema.Architecture.Tests;

public class ApplicationTests : BaseTest
{
    private static readonly string _validatorsNamespacePattern =
        $@"^{Regex.Escape(ApplicationAssembly.GetName().Name!)}\.Features\..+$";

    private const string ValidatorSuffix = "Validator";
    private const string CommandSuffix = "Command";
    private const string QuerySuffix = "Query";

    //Naming Conventions Tests
    [Fact]
    public void Validators_ShouldHave_NameEndingWith_Validator()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith(ValidatorSuffix)
            .Check(Architecture);
    }

    [Fact]
    public void Commands_ShouldHave_NameEndingWith_Command()
    {
        var invalidCommands = ApplicationAssembly.GetTypes()
            .Where(IsConcreteCommandType)
            .Where(type => !type.Name.EndsWith(CommandSuffix, StringComparison.Ordinal))
            .ToList();

        Assert.True(
            invalidCommands.Count == 0,
            $"Found command types without '{CommandSuffix}' suffix: {string.Join(", ", invalidCommands.Select(t => t.Name))}");
    }

    [Fact]
    public void Queries_ShouldHave_NameEndingWith_Query()
    {
        var invalidQueries = ApplicationAssembly.GetTypes()
            .Where(IsConcreteQueryType)
            .Where(type => !type.Name.EndsWith(QuerySuffix, StringComparison.Ordinal))
            .ToList();

        Assert.True(
            invalidQueries.Count == 0,
            $"Found query types without '{QuerySuffix}' suffix: {string.Join(", ", invalidQueries.Select(t => t.Name))}");
    }

    //Colocation Tests
    [Fact]
    public void Validators_Should_Be_In_Feature_UseCase_Folder()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(AbstractValidator<>))
            .Should()
            .ResideInAssembly(ApplicationAssembly)
            .AndShould()
            .ResideInNamespaceMatching(_validatorsNamespacePattern)
            .Check(Architecture);
    }


    //Visibility Tests
    [Fact]
    public void All_Classes_Other_Than_Abstractions_Or_Wiring_Utility_ShouldBe_Internal()
    {
        Classes()
            .That()
            .ResideInAssembly(ApplicationAssembly)
            .And()
            .DoNotHaveAnyAttributes(typeof(PubliclyVisibleAttribute))
            .And()
            .DoNotImplementInterface(typeof(IDto))
            .And()
            .DoNotImplementInterface(typeof(ICommand))
            .And()
            .DoNotImplementInterface(typeof(ICommand<>))
            .And()
            .DoNotImplementInterface(typeof(IQuery<>))
            .And()
            .AreNot(typeof(ApplicationAssemblyMarker))
            .And()
            .AreNot(typeof(ServiceCollectionExtensions))
            .Should()
            .BeInternal()
            .Check(Architecture);
    }

    [Fact]
    public void All_Enums_Other_Than_Abstractions_Or_Wiring_Utility_ShouldBe_Internal()
    {
        var publicEnumsThatShouldntBePubliclyExposed = ApplicationAssembly.GetTypes()
            .Where(t => t.IsEnum && t.IsPublic)
            .Where(t => t.GetCustomAttribute<PubliclyVisibleAttribute>() is null)
            .ToList();

        Assert.True(
            publicEnumsThatShouldntBePubliclyExposed.Count == 0,
            $"Found public enums that shouldn't be exposed: {string.Join(", ", publicEnumsThatShouldntBePubliclyExposed.Select(t => t.Name))}");
    }

    [Fact]
    public void All_Structs_Should_Other_Than_Commands_Or_Queries_ShouldBe_Internal()
    {
        var invalidStructs = ApplicationAssembly.GetTypes()
            .Where(t => t.IsValueType && !t.IsEnum && t.IsPublic)
            .Where(t =>
                !IsConcreteCommandType(t) &&
                !IsConcreteQueryType(t))
            .ToList();

        Assert.True(
            invalidStructs.Count == 0,
            $"Found public structs that aren't commands/queries: " +
            $"{string.Join(", ", invalidStructs.Select(t => t.Name))}");
    }

    //Mutability Tests
    [Fact]
    public void Commands_And_Queries_ShouldBe_SealedClasses_Or_SealedRecords_Or_ReadonlyStructs()
    {
        var invalidTypes = ApplicationAssembly.GetTypes()
            .Where(t => IsConcreteCommandType(t) || IsConcreteQueryType(t))
            .Where(t =>
                (t.IsClass && !t.IsSealed) ||
                (t.IsValueType && !IsReadOnlyStruct(t)))
            .ToList();

        Assert.True(
            invalidTypes.Count == 0,
            $"Found commands/queries that aren't sealed classes or readonly structs: " +
            $"{string.Join(", ", invalidTypes.Select(t => t.Name))}");
    }

    //Dependency Tests
    [Fact]
    public void Application_ShouldNot_Depend_On_ForbiddenNamespaces()
    {
        Types()
            .That()
            .ResideInAssembly(ApplicationAssembly)
            .Should()
            .NotDependOnAny(InNamespace(ForbiddenNamespaces.EntityFrameworkCore))
            .Check(Architecture);
    }

    private static bool IsConcreteCommandType(Type type) =>
        !type.IsInterface &&
        !type.IsAbstract &&
        typeof(ICommand).IsAssignableFrom(type) ||
        ImplementsOpenGenericInterface(type, typeof(ICommand<>));

    private static bool IsConcreteQueryType(Type type) =>
        !type.IsInterface &&
        !type.IsAbstract &&
        ImplementsOpenGenericInterface(type, typeof(IQuery<>));

    private static bool ImplementsOpenGenericInterface(Type type, Type openGenericInterfaceType) =>
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterfaceType);

    private static bool IsReadOnlyStruct(Type type) =>
    type.IsValueType &&
    type.GetCustomAttribute<
        System.Runtime.CompilerServices.IsReadOnlyAttribute>() is not null;

}

