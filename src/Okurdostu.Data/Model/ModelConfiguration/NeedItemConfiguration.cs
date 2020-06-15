using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class NeedItemConfiguration : IEntityTypeConfiguration<NeedItem>
    {
        public void Configure(EntityTypeBuilder<NeedItem> entity)
        {
            entity.ToTable("NeedItem", "okurdostu");

            entity.HasIndex(e => e.Id)
                .HasName("needitem_id_uindex")
                .IsUnique();

            entity.Property(e => e.Link).IsRequired();

            entity.Property(e => e.NeedId).HasColumnName("NeedID");

            entity.Property(e => e.Picture).IsRequired();

            entity.Property(e => e.Price).HasColumnType("money");

            entity.HasOne(d => d.Need)
                .WithMany(p => p.NeedItem)
                .HasForeignKey(d => d.NeedId)
                .HasConstraintName("needitem_need__fk");
        }
    }
}
