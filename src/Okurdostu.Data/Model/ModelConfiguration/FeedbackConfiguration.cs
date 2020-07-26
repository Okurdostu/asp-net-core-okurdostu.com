using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.ModelConfiguration
{
    public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
    {
        public void Configure(EntityTypeBuilder<Feedback> entity)
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(2000);
        }
    }
}