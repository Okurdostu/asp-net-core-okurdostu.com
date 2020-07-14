using Microsoft.EntityFrameworkCore;
using Okurdostu.Data.Model;
using Okurdostu.Data.Model.ModelConfiguration;

namespace Okurdostu.Data
{
    public partial class OkurdostuContext : DbContext
    {
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
        public virtual DbSet<UserEducationDoc> UserEducationDoc { get; set; }
        public virtual DbSet<UserEmailConfirmation> UserEmailConfirmation { get; set; }
        public virtual DbSet<UserPasswordReset> UserPasswordReset { get; set; }
        public virtual DbSet<NeedComment> NeedComment { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new NeedConfiguration());
            modelBuilder.ApplyConfiguration(new NeedItemConfiguration());
            modelBuilder.ApplyConfiguration(new NeedLikeConfiguration());
            modelBuilder.ApplyConfiguration(new UniversityConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new UserEducationConfiguration());
            modelBuilder.ApplyConfiguration(new UserEducationDocConfiguration());
            modelBuilder.ApplyConfiguration(new FeedbackConfiguration());

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
