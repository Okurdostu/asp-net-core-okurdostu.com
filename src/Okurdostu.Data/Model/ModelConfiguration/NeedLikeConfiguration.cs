using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class NeedLikeConfiguration : IEntityTypeConfiguration<NeedLike>
    {
        public void Configure(EntityTypeBuilder<NeedLike> entity)
        {
            entity.ToTable("NeedLike", "okurdostu");

            entity.HasIndex(e => e.Id)
                .HasName("needlike_id_uindex")
                .IsUnique();

            entity.Property(e => e.NeedId).HasColumnName("NeedID");

            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Need)
                .WithMany(p => p.NeedLike)
                .HasForeignKey(d => d.NeedId)
                .HasConstraintName("needlike_need__fk");

            entity.HasOne(d => d.User)
                .WithMany(p => p.NeedLike)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("needlike_user__fk");
        }
    }
}
