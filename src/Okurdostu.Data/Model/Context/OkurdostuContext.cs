using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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

    }
}
