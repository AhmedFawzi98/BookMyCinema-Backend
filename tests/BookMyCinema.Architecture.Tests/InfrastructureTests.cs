using ArchUnitNET.xUnitV3;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookMyCinema.Architecture.Tests;

public class InfrastructureTests : BaseTest
{
    //Naming Conventions Tests


    //Colocation Tests


    //Visibility Tests


    //Dependency Tests
    [Fact]
    public void Infrastructure_ShouldNot_Depend_On_Forbidden_Namespaces()
    {
        Types()
             .That()
             .ResideInAssembly(ApiAssembly)
             .Should()
             .NotDependOnAny(InNamespace(ForbiddenNamespaces.FluentValidation))
             .Check(Architecture);
    }
}

