using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class NeedLikeConfiguration : IEntityTypeConfiguration<NeedLike>
    {
        public void Configure(EntityTypeBuilder<NeedLike> entity)
        {

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.IsCurrentLiked).HasColumnName("isCurrentLiked");

            entity.HasOne(d => d.Need)
                .WithMany(p => p.NeedLike)
                .HasForeignKey(d => d.NeedId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NeedLike_Need");

            entity.HasOne(d => d.User)
                .WithMany(p => p.NeedLike)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_NeedLike_User");

        }
    }
}