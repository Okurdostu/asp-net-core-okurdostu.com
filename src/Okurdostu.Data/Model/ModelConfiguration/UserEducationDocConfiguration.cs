using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class UserEducationDocConfiguration : IEntityTypeConfiguration<UserEducationDoc>
    {
        public void Configure(EntityTypeBuilder<UserEducationDoc> entity)
        {

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.CreatedOn).HasColumnType("smalldatetime");

            entity.Property(e => e.FullPath).IsRequired();

            entity.Property(e => e.PathAfterRoot).IsRequired();

            entity.Property(e => e.UserEducationId)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValueSql("((0))");

        }
    }
}