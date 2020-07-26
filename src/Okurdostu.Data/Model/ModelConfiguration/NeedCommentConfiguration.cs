using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.Model.ModelConfiguration
{
    public class NeedCommentConfiguration : IEntityTypeConfiguration<NeedComment>
    {
        public void Configure(EntityTypeBuilder<NeedComment> entity)
        {
            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Comment)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CreatedOn).HasColumnType("datetime");

            entity.HasOne(d => d.Need)
                .WithMany(p => p.NeedComment)
                .HasForeignKey(d => d.NeedId)
                .HasConstraintName("FK_NeedComment_Need");

            entity.HasOne(d => d.RelatedComment)
                .WithMany(p => p.InverseRelatedComment)
                .HasForeignKey(d => d.RelatedCommentId)
                .HasConstraintName("FK_NeedComment_NeedComment");

            entity.HasOne(d => d.User)
                .WithMany(p => p.NeedComment)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_NeedComment_User");
        }
    }
}