using System.Reflection;
using ArchUnitNET.xUnitV3;
using BookMyCinema.Application;
using BookMyCinema.Application.Common.Abstractions;
using FluentValidation;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ServiceCollectionExtensions = BookMyCinema.Application.ServiceCollectionExtensions;

namespace BookMyCinema.Architecture.Tests;

public class ApplicationTests : BaseTest
{
    private static readonly string _validatorsNameSpacePattern = $@"{ApplicationAssembly.GetName().Name}\.Features\..*\.Validations";

    //Naming Conventions Tests
    [Fact]
    public void Validators_ShouldHave_NameEndingWith_Validator()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(AbstractValidator<>))
            .Should()
            .HaveNameEndingWith("Validator")
            .Check(Architecture);
    }

    [Fact]
    public void Dtos_ShouldHave_NameEndingWith_Dto()
    {
        Classes()
            .That()
            .ResideInAssembly(ApplicationAssembly)
            .And()
            .ImplementInterface(typeof(IDto))
            .Should()
            .HaveNameEndingWith("Dto")
            .Check(Architecture);
    }

    //Colocation Tests
    [Fact]
    public void Validators_Should_Be_In_Feature_Validations_Folder()
    {
        Classes()
            .That()
            .AreAssignableTo(typeof(AbstractValidator<>))
            .Should()
            .ResideInAssembly(ApplicationAssembly)
            .AndShould()
            .ResideInNamespaceMatching(_validatorsNameSpacePattern)
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
}

