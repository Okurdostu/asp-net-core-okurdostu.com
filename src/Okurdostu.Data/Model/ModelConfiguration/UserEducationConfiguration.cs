using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.ModelConfiguration
{
    public class UserEducationConfiguration : IEntityTypeConfiguration<UserEducation>
    {
        public void Configure(EntityTypeBuilder<UserEducation> entity)
        {

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.ActivitiesSocieties).HasMaxLength(200);

            entity.Property(e => e.CreatedOn).HasColumnType("smalldatetime");

            entity.Property(e => e.Department)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.EndYear).HasMaxLength(4);

            entity.Property(e => e.StartYear).HasMaxLength(4);

            entity.HasOne(d => d.University)
                .WithMany(p => p.UserEducation)
                .HasForeignKey(d => d.UniversityId)
                .HasConstraintName("FK_UserEducation_University");

            entity.HasOne(d => d.User)
                .WithMany(p => p.UserEducation)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserEducation_User");

        }
    }
}