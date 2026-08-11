using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookMyCinema.Persistance.Configurations;

internal sealed class CurrencyConfiguration
    : IEntityTypeConfiguration<Entities.Currency>
{
    public void Configure(EntityTypeBuilder<Entities.Currency> builder)
    {
        builder.ToTable("Currencies");

        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code)
            .HasColumnType("char(3)")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(50)
            .IsRequired();
    }
}
