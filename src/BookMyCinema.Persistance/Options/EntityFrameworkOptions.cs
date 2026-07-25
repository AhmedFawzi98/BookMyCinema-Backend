using System;
using System.Collections.Generic;
using System.Text;

namespace BookMyCinema.Persistance.Options;
internal class EntityFrameworkOptions
{
    public const string SectionName = "EntityFramework";

    public bool EnableSensitiveDataLogging { get; init; }

    public bool EnableDetailedErrors { get; init; }
}
