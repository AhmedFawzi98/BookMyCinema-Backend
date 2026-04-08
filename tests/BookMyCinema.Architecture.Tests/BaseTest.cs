using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using BookMyCinema.Api;
using BookMyCinema.Application;
using BookMyCinema.Domain;
using BookMyCinema.Infrastructure;
using BookMyCinema.Persistance;
using BookMyCinema.WebApp;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace BookMyCinema.Architecture.Tests;
public abstract class BaseTest
{
    protected static readonly System.Reflection.Assembly DomainAssembly = typeof(DomainAssemblyMarker).Assembly;
    protected static readonly System.Reflection.Assembly ApplicationAssembly = typeof(ApplicationAssemblyMarker).Assembly;
    protected static readonly System.Reflection.Assembly ApiAssembly = typeof(ApiAssemblyMarker).Assembly;
    protected static readonly System.Reflection.Assembly InfrastructureAssembly = typeof(InfrastructureAssemblyMarker).Assembly;
    protected static readonly System.Reflection.Assembly PersistanceAssembly = typeof(PersistanceAssemblyMarker).Assembly;
    protected static readonly System.Reflection.Assembly WebAppAssembly = typeof(WebAppAssemblyMarker).Assembly;

    protected static readonly ArchUnitNET.Domain.Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            DomainAssembly,
            ApplicationAssembly,
            ApiAssembly,
            InfrastructureAssembly,
            PersistanceAssembly,
            WebAppAssembly
        ).Build();

    protected static class ForbiddenNamespaces
    {
        public const string EntityFrameworkCore = "Microsoft.EntityFrameworkCore";
        public const string FluentValidation = "FluentValidation";
        public const string AspNetCore = "Microsoft.AspNetCore";
    }

    //regex matching so it catches all sub-namespaces
    protected static IObjectProvider<IType> InNamespace(string ns) =>
       Types().That().ResideInNamespaceMatching($"{ns}.*");
}
