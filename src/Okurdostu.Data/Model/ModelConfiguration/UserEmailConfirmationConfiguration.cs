using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.ModelConfiguration
{
    public class UserEmailConfirmationConfiguration : IEntityTypeConfiguration<UserEmailConfirmation>
    {
        public void Configure(EntityTypeBuilder<UserEmailConfirmation> entity)
        {

            entity.HasKey(e => e.GUID);

            entity.Property(e => e.GUID)
                .HasColumnName("GUID")
                .ValueGeneratedNever();

            entity.Property(e => e.CreatedOn).HasColumnType("smalldatetime");

            entity.Property(e => e.NewEmail).HasMaxLength(100);

            entity.Property(e => e.UsedOn).HasColumnType("smalldatetime");

            entity.HasOne(d => d.User)
                .WithMany(p => p.UserEmailConfirmation)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserEmailConfirmation_User");

        }
    }
}