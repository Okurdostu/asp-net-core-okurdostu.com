using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class NeedConfiguration : IEntityTypeConfiguration<Need>
    {
        public void Configure(EntityTypeBuilder<Need> entity)
        {
            entity.HasIndex(e => e.Title)
                    .HasName("Unique_Key_Title")
                    .IsUnique();

            entity.Property(e => e.Description).IsRequired();

            entity.Property(e => e.FinishedOn).HasColumnType("smalldatetime");

            entity.Property(e => e.FriendlyTitle)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.StartedOn).HasColumnType("smalldatetime");

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(75);

            entity.Property(e => e.TotalCharge).HasColumnType("money");

            entity.HasOne(d => d.User)
                .WithMany(p => p.Need)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Need_User");
        }
    }
}