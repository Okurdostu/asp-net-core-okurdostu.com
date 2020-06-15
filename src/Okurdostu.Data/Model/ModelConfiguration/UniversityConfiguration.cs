using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class UniversityConfiguration : IEntityTypeConfiguration<University>
    {
        public void Configure(EntityTypeBuilder<University> entity)
        {
            entity.ToTable("University", "okurdostu");

            entity.HasIndex(e => e.Id)
                .HasName("university_id_uindex")
                .IsUnique();

            entity.Property(e => e.Name).IsRequired();
        }
    }
}
