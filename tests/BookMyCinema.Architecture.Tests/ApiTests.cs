using ArchUnitNET.xUnitV3;
using BookMyCinema.Api.Api;
using BookMyCinema.Api.Api.Abstractions;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookMyCinema.Architecture.Tests;
public class ApiTests : BaseTest
{
    private static readonly string _abstractionsNamespace =
        $"{ApiAssembly.GetName().Name}.Api.Abstractions";

    private static readonly string _endpointNamespace =
        $@"{ApiAssembly.GetName().Name}\.Api\..*\.Endpoints\..*";

    //Naming Conventions Tests
    [Fact]
    public void Endpoints_ShouldHave_NameEndingWith_Endpoint()
    {
        Classes()
             .That()
             .ImplementInterface(typeof(IEndpoint))
             .Should()
             .HaveNameEndingWith("Endpoint")
             .Check(Architecture);
    }

    [Fact]
    public void Classes_That_Have_NameEndingWith_Endpoint_Should_Implement_IEndpoint()
    {
        Classes()
            .That()
            .HaveNameEndingWith("Endpoint")
            .Should()
            .ImplementInterface(typeof(IEndpoint))
            .Check(Architecture);
    }


    //Colocation Tests

    [Fact]
    public void Api_Interfaces_ShouldBe_In_Abstractions_Namespace()
    {
        Interfaces()
            .That()
            .ResideInAssembly(ApiAssembly)
            .Should()
            .ResideInNamespace(_abstractionsNamespace)
            .Check(Architecture);
    }

    [Fact]
    public void Endpoints_Should_Reside_In_Endpoints_Namespace()
    {
        Classes()
            .That()
            .ResideInAssembly(ApiAssembly)
            .And()
            .ImplementInterface(typeof(IEndpoint))
            .Should()
            .ResideInNamespaceMatching(_endpointNamespace)
            .Check(Architecture);
    }


    //Visibility Tests
    [Fact]
    public void All_Types_Other_Than_ApiRoutes_ShouldBe_Internal()
    {
        Types()
            .That()
            .ResideInAssembly(ApiAssembly)
            .And()
            .AreNot(typeof(ApiRoutes))
            .Should()
            .BeInternal()
            .Check(Architecture);
    }

    //Dependency Tests
    [Fact]
    public void Api_ShouldNot_Depend_On_Forbidden_Namespaces()
    {
        Types()
             .That()
             .ResideInAssembly(ApiAssembly)
             .Should()
             .NotDependOnAny(InNamespace(ForbiddenNamespaces.EntityFrameworkCore))
             .Check(Architecture);
    }
}

