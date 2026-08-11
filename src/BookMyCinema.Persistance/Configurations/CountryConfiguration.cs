using BookMyCinema.Persistance.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookMyCinema.Persistance.Configurations;

internal sealed class CountryConfiguration
    : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.ToTable("Countries");

        builder.HasKey(x => x.Code);

        builder.Property(x => x.Code)
            .HasColumnType("char(2)")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();
    }
}
