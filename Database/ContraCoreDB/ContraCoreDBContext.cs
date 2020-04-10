using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Database.ContraCoreDB
{
    public partial class ContraCoreDBContext : DbContext
    {
        public ContraCoreDBContext()
        {
        }

        public ContraCoreDBContext(DbContextOptions<ContraCoreDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<blacklist> blacklist { get; set; }
        public virtual DbSet<config> config { get; set; }
        public virtual DbSet<lease> lease { get; set; }
        public virtual DbSet<lease_details> lease_details { get; set; }
        public virtual DbSet<oui> oui { get; set; }
        public virtual DbSet<oui_vendors> oui_vendors { get; set; }
        public virtual DbSet<reservation> reservation { get; set; }
        public virtual DbSet<whitelist> whitelist { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Server=10.3.0.16;Port=5432;User Id=contracore_usr;Password=EvPvkro59Jb7RK3o;Database=contradb;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<blacklist>(entity =>
            {
                entity.ToTable("blacklist", "contracore");

                entity.Property(e => e.id).UseIdentityAlwaysColumn();

                entity.Property(e => e._class).HasColumnName("class");

                entity.Property(e => e.pattern).IsRequired();
            });

            modelBuilder.Entity<config>(entity =>
            {
                entity.ToTable("config", "contracore");

                entity.Property(e => e.id).UseIdentityAlwaysColumn();

                entity.Property(e => e.domain_needed)
                    .IsRequired()
                    .HasDefaultValueSql("true");

                entity.Property(e => e.search_domains)
                    .IsRequired()
                    .HasDefaultValueSql("ARRAY[]::text[]");

                entity.Property(e => e.sources)
                    .IsRequired()
                    .HasDefaultValueSql("ARRAY[]::text[]");

                entity.Property(e => e.spoofed_a)
                    .IsRequired()
                    .HasDefaultValueSql("'0.0.0.0'::text");

                entity.Property(e => e.spoofed_aaaa)
                    .IsRequired()
                    .HasDefaultValueSql("'::'::text");

                entity.Property(e => e.spoofed_cname)
                    .IsRequired()
                    .HasDefaultValueSql("''::text");

                entity.Property(e => e.spoofed_default)
                    .IsRequired()
                    .HasDefaultValueSql("'-'::text");
            });

            modelBuilder.Entity<lease>(entity =>
            {
                entity.ToTable("lease", "contracore");

                entity.Property(e => e.id).UseIdentityAlwaysColumn();

                entity.Property(e => e.ip).IsRequired();

                entity.Property(e => e.mac).IsRequired();

                entity.Property(e => e.op)
                    .IsRequired()
                    .HasMaxLength(3)
                    .IsFixedLength();

                entity.Property(e => e.source).IsRequired();

                entity.Property(e => e.time).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<lease_details>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("lease_details", "contracore");

                entity.Property(e => e.op)
                    .HasMaxLength(3)
                    .IsFixedLength();
            });

            modelBuilder.Entity<oui>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("oui", "contracore");

                entity.Property(e => e.mac)
                    .HasMaxLength(8)
                    .IsFixedLength();
            });

            modelBuilder.Entity<oui_vendors>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("oui_vendors", "contracore");
            });

            modelBuilder.Entity<reservation>(entity =>
            {
                entity.ToTable("reservation", "contracore");

                entity.Property(e => e.id).UseIdentityAlwaysColumn();

                entity.Property(e => e.active)
                    .IsRequired()
                    .HasDefaultValueSql("true");

                entity.Property(e => e.ip).IsRequired();

                entity.Property(e => e.mac).IsRequired();

                entity.Property(e => e.time).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<whitelist>(entity =>
            {
                entity.ToTable("whitelist", "contracore");

                entity.Property(e => e.id).UseIdentityAlwaysColumn();

                entity.Property(e => e.pattern).IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
