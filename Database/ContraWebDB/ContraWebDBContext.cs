using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Database.ContraWebDB
{
    public partial class ContraWebDBContext : DbContext
    {
        public ContraWebDBContext()
        {
        }

        public ContraWebDBContext(DbContextOptions<ContraWebDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<user> user { get; set; }
        public virtual DbSet<user_session> user_session { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Server=127.0.0.1;Port=5432;User Id=contraweb_usr;Password=U475jBKZfK3xhbVZ;Database=contradb;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum("contraweb", "user_role", new[] { "restricted", "privileged", "administrator" });

            modelBuilder.Entity<user>(entity =>
            {
                entity.HasKey(e => e.username)
                    .HasName("user_pk");

                entity.ToTable("user", "contraweb");

                entity.Property(e => e.username).HasMaxLength(16);

                entity.Property(e => e.password)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(e => e.role)
                    .IsRequired()
                    .HasMaxLength(13);

                entity.Property(e => e.salt)
                    .IsRequired()
                    .HasMaxLength(32);
            });

            modelBuilder.Entity<user_session>(entity =>
            {
                entity.HasKey(e => e.username)
                    .HasName("user_session_pk");

                entity.ToTable("user_session", "contraweb");

                entity.Property(e => e.username).HasMaxLength(16);

                entity.Property(e => e.token)
                    .IsRequired()
                    .HasMaxLength(32);

                entity.HasOne(d => d.usernameNavigation)
                    .WithOne(p => p.user_session)
                    .HasForeignKey<user_session>(d => d.username)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_session_user_id_fk");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
