using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class UserEducationDocumentConfiguration : IEntityTypeConfiguration<UserEducationDocument>
    {
        public void Configure(EntityTypeBuilder<UserEducationDocument> entity)
        {
            entity.HasKey(e => e.EducationDocumentId)
                .HasName("usereducationdocument_pk");

            entity.ToTable("UserEducationDocument", "okurdostu");

            entity.HasIndex(e => e.EducationDocumentId)
                .HasName("usereducationdocument_educationdocumentid_uindex")
                .IsUnique();

            entity.Property(e => e.EducationDocumentId)
                .HasColumnName("EducationDocumentID")
                .ValueGeneratedNever();

            entity.Property(e => e.DocumentPath).IsRequired();

            entity.Property(e => e.DocumentUrl).IsRequired();

            entity.Property(e => e.UserEducationId).HasColumnName("UserEducationID");

            entity.HasOne(d => d.UserEducation)
                .WithMany(p => p.UserEducationDocument)
                .HasForeignKey(d => d.UserEducationId)
                .HasConstraintName("usereducationdocument_usereducation__fk");
        }
    }
}
