using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class NeedConfiguration : IEntityTypeConfiguration<Need>
    {
        public void Configure(EntityTypeBuilder<Need> entity)
        {
            entity.ToTable("Need", "okurdostu");

            entity.HasIndex(e => e.FriendlyTitle)
                .HasName("need_friendlytitle_uindex")
                .IsUnique();

            entity.HasIndex(e => e.Id)
                .HasName("need_id_uindex")
                .IsUnique();

            entity.HasIndex(e => e.Title)
                .HasName("need_title_uindex")
                .IsUnique();

            entity.Property(e => e.Description).IsRequired();

            entity.Property(e => e.FriendlyTitle)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(75);

            entity.Property(e => e.TotalCharge).HasColumnType("money");

            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Need)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("need_user__fk");

        }
    }
}
