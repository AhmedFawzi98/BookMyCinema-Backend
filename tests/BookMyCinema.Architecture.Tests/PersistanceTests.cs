using System.Reflection;
using ArchUnitNET.xUnitV3;
using BookMyCinema.Persistance;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookMyCinema.Architecture.Tests;

public class PersistanceTests : BaseTest
{
    //Naming Conventions Tests


    //Colocation Tests


    //Visibility Tests
    [Fact]
    public void All_Types_Other_Than_Abstractions_Or_Wiring_Utility_ShouldBe_Internal()
    {
        Classes()
           .That()
           .ResideInAssembly(PersistanceAssembly)
           .And()
           .AreNotAssignableTo(typeof(Migration))
           .And()
           .AreNotAssignableTo(typeof(ModelSnapshot))
           .And()
           .DoNotHaveAnyAttributes(typeof(PubliclyVisibleAttribute))
           .And()
           .AreNot(typeof(PersistanceAssemblyMarker))
           .And()
           .AreNot(typeof(ServiceCollectionExtensions))
           .Should()
           .BeInternal()
           .Check(Architecture);
    }

    [Fact]
    public void All_Enums_Other_Than_Abstractions_Or_Wiring_Utility_ShouldBe_Internal()
    {
        var publicEnumsThatShouldntBePubliclyExposed = PersistanceAssembly.GetTypes()
            .Where(t => t.IsEnum && t.IsPublic)
            .Where(t => t.GetCustomAttribute<PubliclyVisibleAttribute>() is null)
            .ToList();

        Assert.True(
            publicEnumsThatShouldntBePubliclyExposed.Count == 0,
            $"Found public enums that shouldn't be exposed: {string.Join(", ", publicEnumsThatShouldntBePubliclyExposed.Select(t => t.Name))}");
    }


    //Dependency Tests
    [Fact]
    public void Persistance_ShouldNot_Depend_On_Forbidden_Namespaces()
    {
        Types()
             .That()
             .ResideInAssembly(ApiAssembly)
             .Should()
             .NotDependOnAny(InNamespace(ForbiddenNamespaces.FluentValidation))
             .Check(Architecture);
    }
}

