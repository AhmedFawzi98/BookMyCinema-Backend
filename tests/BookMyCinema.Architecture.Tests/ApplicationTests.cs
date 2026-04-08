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
    public void All_Types_Other_Than_Dtos_Or_Result_ShouldBe_Internal()
    {
        Types()
           .That()
           .ResideInAssembly(ApplicationAssembly)
           .And()
           .AreNot(typeof(IDto))
           .And()
           .AreNot(typeof(IResult))
           .And()
           .DoNotImplementInterface(typeof(IDto))
           .And()
           .DoNotImplementInterface(typeof(IResult))
           .And()
           .AreNot(typeof(ApplicationAssemblyMarker))
           .And()
           .AreNot(typeof(ServiceCollectionExtensions))
           .Should()
           .BeInternal()
           .Check(Architecture);
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

