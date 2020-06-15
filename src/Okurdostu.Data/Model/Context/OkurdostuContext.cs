using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Okurdostu.Data.Model.ModelConfiguration;

namespace Okurdostu.Data.Model.Context
{
    public partial class OkurdostuContext : DbContext
    {
        public OkurdostuContext()
        {
        }

        public OkurdostuContext(DbContextOptions<OkurdostuContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Need> Need { get; set; }
        public virtual DbSet<NeedItem> NeedItem { get; set; }
        public virtual DbSet<NeedLike> NeedLike { get; set; }
        public virtual DbSet<University> University { get; set; }
        public virtual DbSet<User> User { get; set; }
        public virtual DbSet<UserEducation> UserEducation { get; set; }
        public virtual DbSet<UserEducationDocument> UserEducationDocument { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=Okurdostu;Username=postgres;Password=root");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new NeedConfiguration());

            modelBuilder.ApplyConfiguration(new NeedItemConfiguration());

            modelBuilder.ApplyConfiguration(new NeedLikeConfiguration());

            modelBuilder.ApplyConfiguration(new UniversityConfiguration());

            modelBuilder.ApplyConfiguration(new UserConfiguration());

            modelBuilder.ApplyConfiguration(new UserEducationConfiguration());

            modelBuilder.ApplyConfiguration(new UserEducationDocumentConfiguration());

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
