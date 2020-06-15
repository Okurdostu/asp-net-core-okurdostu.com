using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.ToTable("User", "okurdostu");

            entity.HasIndex(e => e.Email)
                .HasName("user_email_uindex")
                .IsUnique();

            entity.HasIndex(e => e.Id)
                .HasName("user_id_uindex")
                .IsUnique();

            entity.HasIndex(e => e.Telephone)
                .HasName("user_telephone_uindex")
                .IsUnique();

            entity.HasIndex(e => e.Username)
                .HasName("user_username_uindex")
                .IsUnique();

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Biography).HasMaxLength(256);

            entity.Property(e => e.ContactEmail).HasMaxLength(50);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(25);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Github).HasMaxLength(39);

            entity.Property(e => e.Password).IsRequired();

            entity.Property(e => e.Twitter).HasMaxLength(15);

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(25);
        }
    }
}
