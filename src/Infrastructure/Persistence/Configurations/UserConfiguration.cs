using System;
using EazyMenu.Domain.Aggregates.Users;
using EazyMenu.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EazyMenu.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        // Use field-based property access for all properties to bypass constructor binding
        builder.UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => UserId.From(value))
            .HasColumnName("Id")
            .IsRequired();

        builder.Property(u => u.TenantId)
            .HasConversion(
                id => id.Value,
                value => TenantId.FromGuid(value))
            .HasColumnName("TenantId")
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasMaxLength(200)
            .IsRequired();

        builder.OwnsOne(u => u.PhoneNumber, phone =>
        {
            phone.Property(p => p.Value)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(15)
                .IsRequired(false);

            // Use field-based access for constructor binding
            phone.UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.OwnsOne(u => u.PasswordHash, pwd =>
        {
            pwd.Property(p => p.HashedValue)
                .HasColumnName("PasswordHash")
                .HasMaxLength(500)
                .IsRequired();

            pwd.Property(p => p.Algorithm)
                .HasColumnName("PasswordAlgorithm")
                .HasMaxLength(50)
                .IsRequired();

            // Use field-based access for constructor binding
            pwd.UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.LastLoginAtUtc)
            .IsRequired(false);

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired();

        // Indexes
        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_Users_Email");

        builder.HasIndex(u => u.TenantId)
            .HasDatabaseName("IX_Users_TenantId");

        builder.HasIndex(u => new { u.TenantId, u.Status })
            .HasDatabaseName("IX_Users_TenantId_Status");

        // Ignore domain events collection
        builder.Ignore(u => u.DomainEvents);
    }
}
