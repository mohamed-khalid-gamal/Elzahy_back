using Microsoft.EntityFrameworkCore;
using Elzahy.Models;

namespace Elzahy.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<RecoveryCode> RecoveryCodes { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectImage> ProjectImages { get; set; }
        public DbSet<ProjectVideo> ProjectVideos { get; set; }
        public DbSet<ProjectTranslation> ProjectTranslations { get; set; }
        public DbSet<Award> Awards { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<TwoFactorCode> TwoFactorCodes { get; set; }
        public DbSet<AdminRequest> AdminRequests { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Language).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
            });
            
            // RefreshToken configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.Property(e => e.Token).IsRequired().HasMaxLength(255);
                
                entity.HasOne(e => e.User)
                      .WithMany(e => e.RefreshTokens)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // RecoveryCode configuration
            modelBuilder.Entity<RecoveryCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CodeHash).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => new { e.UserId, e.IsUsed });
                
                entity.HasOne(e => e.User)
                      .WithMany(e => e.RecoveryCodes)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AdminRequest configuration
            modelBuilder.Entity<AdminRequest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
                entity.Property(e => e.AdditionalInfo).HasMaxLength(1000);
                entity.Property(e => e.AdminNotes).HasMaxLength(500);
                entity.HasIndex(e => new { e.UserId, e.IsProcessed });
                
                entity.HasOne(e => e.User)
                      .WithMany(e => e.AdminRequests)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.NoAction);
                      
                entity.HasOne(e => e.ProcessedByAdmin)
                      .WithMany(e => e.ProcessedAdminRequests)
                      .HasForeignKey(e => e.ProcessedByAdminId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Project configuration with enhanced real estate fields
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Status).HasConversion<string>();
                
                // Legacy fields
                entity.Property(e => e.Budget).HasPrecision(18, 2);
                entity.Property(e => e.TechnologiesUsed).HasMaxLength(500);
                entity.Property(e => e.ProjectUrl).HasMaxLength(500);
                entity.Property(e => e.GitHubUrl).HasMaxLength(500);
                entity.Property(e => e.Client).HasMaxLength(100);
                
                // Real estate specific fields
                entity.Property(e => e.CompanyUrl).HasMaxLength(500);
                entity.Property(e => e.GoogleMapsUrl).HasMaxLength(500);
                entity.Property(e => e.Location).HasMaxLength(200);
                entity.Property(e => e.PropertyType).HasMaxLength(100);
                entity.Property(e => e.ProjectArea).HasPrecision(18, 2);
                entity.Property(e => e.PriceStart).HasPrecision(20, 2);
                entity.Property(e => e.PriceEnd).HasPrecision(20, 2);
                entity.Property(e => e.PriceCurrency).HasMaxLength(10);
                
                entity.HasOne(e => e.CreatedBy)
                      .WithMany(e => e.Projects)
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
                      
                // Indexes for performance
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.IsPublished);
                entity.HasIndex(e => e.IsFeatured);
                entity.HasIndex(e => e.PropertyType);
                entity.HasIndex(e => e.Location);
                entity.HasIndex(e => e.SortOrder);
            });

            // ProjectImage configuration - Updated for file system storage
            modelBuilder.Entity<ProjectImage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);
                
                entity.HasOne(e => e.Project)
                      .WithMany(e => e.Images)
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                      
                entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
                      
                entity.HasIndex(e => new { e.ProjectId, e.IsMainImage });
                entity.HasIndex(e => e.SortOrder);
                entity.HasIndex(e => e.FilePath);
            });

            // ProjectVideo configuration - Updated for file system storage
            modelBuilder.Entity<ProjectVideo>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FilePath).IsRequired().HasMaxLength(500);
                entity.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FileName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.FileSize).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.Project)
                      .WithMany(e => e.Videos)
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.CreatedBy)
                      .WithMany()
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.ProjectId, e.IsMainVideo });
                entity.HasIndex(e => e.SortOrder);
                entity.HasIndex(e => e.FilePath);
            });

            // ProjectTranslation configuration with direction support
            modelBuilder.Entity<ProjectTranslation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Language).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Direction).HasConversion<string>();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).IsRequired();

                entity.HasOne(e => e.Project)
                      .WithMany(e => e.Translations)
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ProjectId, e.Language }).IsUnique();
                entity.HasIndex(e => e.Language);
            });

            // Award configuration
            modelBuilder.Entity<Award>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.GivenBy).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CertificateUrl).HasMaxLength(500);
                entity.Property(e => e.ImageData).HasColumnType("varbinary(max)");
                entity.Property(e => e.ImageContentType).HasMaxLength(100);
                entity.Property(e => e.ImageFileName).HasMaxLength(255);
                
                entity.HasOne(e => e.CreatedBy)
                      .WithMany(e => e.Awards)
                      .HasForeignKey(e => e.CreatedByUserId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // ContactMessage configuration
            modelBuilder.Entity<ContactMessage>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.EmailAddress).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.PhoneNumber).HasMaxLength(500);
                entity.Property(e => e.Company).HasMaxLength(100);
                entity.HasIndex(e => e.CreatedAt);
            });

            // TwoFactorCode configuration
            modelBuilder.Entity<TwoFactorCode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(6);
                entity.Property(e => e.Purpose).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => new { e.UserId, e.Code, e.Purpose });
                
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
