using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.ModelConfiguration
{
    public class UserEducationDocConfiguration : IEntityTypeConfiguration<UserEducationDoc>
    {
        public void Configure(EntityTypeBuilder<UserEducationDoc> entity)
        {

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.CreatedOn).HasColumnType("smalldatetime");

            entity.Property(e => e.FullPath).IsRequired();

            entity.HasOne(d => d.UserEducation)
                .WithMany(p => p.UserEducationDoc)
                .HasForeignKey(d => d.UserEducationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserEducationDoc_UserEducation");

        }
    }
}