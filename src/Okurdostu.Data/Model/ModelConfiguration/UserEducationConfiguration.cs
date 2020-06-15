using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class UserEducationConfiguration : IEntityTypeConfiguration<UserEducation>
    {
        public void Configure(EntityTypeBuilder<UserEducation> entity)
        {
            entity.ToTable("UserEducation", "okurdostu");

            entity.HasIndex(e => e.Id)
                .HasName("usereducation_id_uindex")
                .IsUnique();

            entity.Property(e => e.ActivitiesSocieties).HasMaxLength(1000);

            entity.Property(e => e.Department)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.EndYear).HasColumnType("numeric(4,0)");

            entity.Property(e => e.StartYear).HasColumnType("numeric(4,0)");

            entity.Property(e => e.UniversityId).HasColumnName("UniversityID");

            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.University)
                .WithMany(p => p.UserEducation)
                .HasForeignKey(d => d.UniversityId)
                .HasConstraintName("usereducation_university__fk");

            entity.HasOne(d => d.User)
                .WithMany(p => p.UserEducation)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("usereducation_user__fk");
        }
    }
}
