using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.HasIndex(e => e.Email)
                    .HasName("Unique_Key_Email")
                    .IsUnique();

            entity.HasIndex(e => e.Username)
                .HasName("Unique_Key_Username")
                .IsUnique();

            entity.Property(e => e.Biography).HasMaxLength(256);

            entity.Property(e => e.ContactEmail).HasMaxLength(50);

            entity.Property(e => e.CreatedOn).HasColumnType("date");

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Github).HasMaxLength(39);

            entity.Property(e => e.Password).IsRequired();

            entity.Property(e => e.Telephone)
                .HasMaxLength(15)
                .IsFixedLength();

            entity.Property(e => e.Twitter).HasMaxLength(15);

            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(15);
        }
    }
}