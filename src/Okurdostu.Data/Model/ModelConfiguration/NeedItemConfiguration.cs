using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Okurdostu.Data.ModelConfiguration
{
    public class NeedItemConfiguration : IEntityTypeConfiguration<NeedItem>
    {
        public void Configure(EntityTypeBuilder<NeedItem> entity)
        {

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.Property(e => e.Link).IsRequired();

            entity.Property(e => e.Picture).IsRequired();

            entity.Property(e => e.Price).HasColumnType("money");

            entity.HasOne(d => d.Need)
                .WithMany(p => p.NeedItem)
                .HasForeignKey(d => d.NeedId)
                .HasConstraintName("FK_NeedItem_Need");

        }
    }
}