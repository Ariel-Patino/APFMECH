using APFMech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace APFMech.Infrastructure.Persistence.Configurations;

public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.HasKey(w => w.Id);

        builder.Property(w => w.TrackingNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(w => w.Status)
            .HasConversion<string>()
            .IsRequired();

        // Ignore domain events during EF Core tracking and database mapping
        builder.Ignore(w => w.DomainEvents);
    }
}