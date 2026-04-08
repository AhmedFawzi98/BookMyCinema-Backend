using ArchUnitNET.xUnitV3;
using BookMyCinema.Application.Common.Abstractions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;


namespace BookMyCinema.Architecture.Tests;

public class DomainTests : BaseTest
{

    //Naming Conventions Tests
    [Fact]
    public void Domain_ShouldNot_Have_Types_EndingWith_Dto()
    {
        Types()
            .That()
            .ResideInAssembly(DomainAssembly)
            .Should()
            .NotHaveNameEndingWith("Dto")
            .Check(Architecture);
    }

    //Colocation Tests


    //Visibility Tests


    //Dependency Test
    [Fact]
    public void Domain_ShouldNot_Depend_On_ForbiddenNamespaces()
    {
        Types()
            .That()
            .ResideInAssembly(DomainAssembly)
            .Should()
            .NotDependOnAny(InNamespace(ForbiddenNamespaces.EntityFrameworkCore))
            .AndShould()
            .NotDependOnAny(InNamespace(ForbiddenNamespaces.FluentValidation))
            .AndShould()
            .NotDependOnAny(InNamespace(ForbiddenNamespaces.AspNetCore))
            .Check(Architecture);
    }
    [Fact]
    public void Domain_ShouldNot_Have_Types_Implement_IDto()
    {
        Types()
            .That()
            .ResideInAssembly(DomainAssembly)
            .Should()
            .NotImplementInterface(typeof(IDto))
            .Check(Architecture);
    }
}

