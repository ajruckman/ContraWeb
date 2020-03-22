using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Database.ContraDB
{
    public partial class ContraDBContext : DbContext
    {
        public ContraDBContext()
        {
        }

        public ContraDBContext(DbContextOptions<ContraDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<blacklist> blacklist { get; set; }
        public virtual DbSet<config> config { get; set; }
        public virtual DbSet<lease> lease { get; set; }
        public virtual DbSet<lease_details> lease_details { get; set; }
        public virtual DbSet<log> log { get; set; }
        public virtual DbSet<log_block_details> log_block_details { get; set; }
        public virtual DbSet<log_details_recent> log_details_recent { get; set; }
        public virtual DbSet<oui> oui { get; set; }
        public virtual DbSet<reservation> reservation { get; set; }
        public virtual DbSet<whitelist> whitelist { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseNpgsql("Server=127.0.0.1;Port=5432;User Id=contra_usr;Password=EvPvkro59Jb7RK3o;Database=contradb;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<blacklist>(entity =>
            {
                entity.ToTable("blacklist", "contra");

                entity.Property(e => e.id).HasDefaultValueSql("nextval('blacklist_id_seq'::regclass)");

                entity.Property(e => e._class).HasColumnName("class");

                entity.Property(e => e.pattern).IsRequired();
            });

            modelBuilder.Entity<config>(entity =>
            {
                entity.ToTable("config", "contra");

                entity.Property(e => e.id).HasDefaultValueSql("nextval('config_id_seq'::regclass)");

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
                entity.ToTable("lease", "contra");

                entity.Property(e => e.id).HasDefaultValueSql("nextval('lease_id_seq'::regclass)");

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

                entity.ToTable("lease_details", "contra");

                entity.Property(e => e.op)
                    .HasMaxLength(3)
                    .IsFixedLength();
            });

            modelBuilder.Entity<log>(entity =>
            {
                entity.ToTable("log", "contra");

                entity.Property(e => e.id).HasDefaultValueSql("nextval('log_id_seq'::regclass)");

                entity.Property(e => e.action).IsRequired();

                entity.Property(e => e.client).IsRequired();

                entity.Property(e => e.question).IsRequired();

                entity.Property(e => e.question_type).IsRequired();

                entity.Property(e => e.time).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<log_block_details>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("log_block_details", "contra");
            });

            modelBuilder.Entity<log_details_recent>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("log_details_recent", "contra");
            });

            modelBuilder.Entity<oui>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("oui", "contra");

                entity.Property(e => e.mac)
                    .HasMaxLength(8)
                    .IsFixedLength();
            });

            modelBuilder.Entity<reservation>(entity =>
            {
                entity.ToTable("reservation", "contra");

                entity.Property(e => e.id).HasDefaultValueSql("nextval('reservation_id_seq'::regclass)");

                entity.Property(e => e.active)
                    .IsRequired()
                    .HasDefaultValueSql("true");

                entity.Property(e => e.ip).IsRequired();

                entity.Property(e => e.mac).IsRequired();

                entity.Property(e => e.time).HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<whitelist>(entity =>
            {
                entity.ToTable("whitelist", "contra");

                entity.Property(e => e.id).HasDefaultValueSql("nextval('whitelist_id_seq'::regclass)");

                entity.Property(e => e.pattern).IsRequired();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
