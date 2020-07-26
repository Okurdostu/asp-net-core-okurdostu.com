using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.ModelConfiguration
{
    public class UserPasswordResetConfiguration : IEntityTypeConfiguration<UserPasswordReset>
    {
        public void Configure(EntityTypeBuilder<UserPasswordReset> entity)
        {

            entity.HasKey(e => e.GUID);

            entity.Property(e => e.GUID)
                .HasColumnName("GUID")
                .ValueGeneratedNever();

            entity.Property(e => e.CreatedOn).HasColumnType("smalldatetime");

            entity.Property(e => e.UsedOn).HasColumnType("smalldatetime");

            entity.HasOne(d => d.User)
                .WithMany(p => p.UserPasswordReset)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserPasswordReset_User");

        }
    }
}