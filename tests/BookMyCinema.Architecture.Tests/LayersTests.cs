using ArchUnitNET.Domain;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookMyCinema.Architecture.Tests;

public class LayersTests : BaseTest
{
    private static readonly IObjectProvider<IType> _domainLayer =
        Types().That().ResideInAssembly(DomainAssembly);

    private static readonly IObjectProvider<IType> _applicationLayer =
        Types().That().ResideInAssembly(ApplicationAssembly);

    private static readonly IObjectProvider<IType> _infrastructureLayer =
        Types().That().ResideInAssembly(InfrastructureAssembly);

    private static readonly IObjectProvider<IType> _persistanceLayer =
        Types().That().ResideInAssembly(PersistanceAssembly);

    private static readonly IObjectProvider<IType> _apiLayer =
        Types().That().ResideInAssembly(ApiAssembly);

    private static readonly IObjectProvider<IType> _webAppLayer =
        Types().That().ResideInAssembly(WebAppAssembly);

    [Fact]
    public void Domain_Should_Not_Depend_On_Any_Other_Layer()
    {
        Types().That().Are(_domainLayer)
           .Should()
           .NotDependOnAny(_applicationLayer)
           .AndShould()
           .NotDependOnAny(_infrastructureLayer)
           .AndShould()
           .NotDependOnAny(_persistanceLayer)
           .AndShould()
           .NotDependOnAny(_apiLayer)
           .AndShould()
           .NotDependOnAny(_webAppLayer)
           .Check(Architecture);
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Any_Layer_Other_Than_Domain()
    {
        Types().That().Are(_applicationLayer)
           .Should()
            .NotDependOnAny(_infrastructureLayer)
            .AndShould()
            .NotDependOnAny(_persistanceLayer)
            .AndShould()
            .NotDependOnAny(_apiLayer)
            .AndShould()
            .NotDependOnAny(_webAppLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Api_Or_Persistance_Or_WebApp()
    {
        Types().That().Are(_infrastructureLayer)
            .Should()
            .NotDependOnAny(_apiLayer)
            .AndShould()
            .NotDependOnAny(_persistanceLayer)
            .AndShould()
            .NotDependOnAny(_webAppLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Persistance_Should_Not_Depend_On_Api_Or_Infrastructure_Or_WebApp()
    {
        Types().That().Are(_persistanceLayer)
            .Should()
            .NotDependOnAny(_apiLayer)
            .AndShould()
            .NotDependOnAny(_infrastructureLayer)
            .AndShould()
            .NotDependOnAny(_webAppLayer)
            .Check(Architecture);
    }

    [Fact]
    public void Api_Should_Not_Depend_On_Infrastructure_Or_Persistence_Or_WebApp()
    {
        Types().That().Are(_apiLayer).Should()
            .NotDependOnAny(_infrastructureLayer)
            .AndShould()
            .NotDependOnAny(_persistanceLayer)
            .AndShould()
            .NotDependOnAny(_webAppLayer)
            .Check(Architecture);
    }
}
