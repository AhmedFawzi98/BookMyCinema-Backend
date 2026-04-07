using BookMyCinema.Domain;
using NetArchTest.Rules;

namespace BookMyCinema.Architecture.Tests;

public class ArchitectureTests
{
    private const string _domainNamespace = "BookMyCinema.Domain";
    private const string _applicationNamespace = "BookMyCinema.Application";
    private const string _apiNamespace = "BookMyCinema.Api";
    private const string _infrastructureNamespace = "BookMyCinema.Infrastructure";
    private const string _webAppNamespace = "BookMyCinema.WebApp";

    [Fact]
    public void Domain_Should_Not_ReferenceAnyOtherProjects()
    {
        var domainAssembly = typeof(DomainAssemblyMarker).Assembly;

        var otherAssemblies = new[]
        {
            _applicationNamespace,
            _apiNamespace,
            _infrastructureNamespace,
            _webAppNamespace
        };

        var result = Types
            .InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherAssemblies)
            .GetResult();

        Assert.True(result.IsSuccessful);

    }
}
