using MediaFinder.Entities;
using MediaFinder.Enums;
using Microsoft.EntityFrameworkCore;

namespace MediaFinder.Data
{
    public class MediaFinderDbContext : DbContext
    {
        public MediaFinderDbContext(DbContextOptions<MediaFinderDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Rating> Ratings => Set<Rating>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Favorite> Favorites => Set<Favorite>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureUsers(modelBuilder);
            ConfigureRatings(modelBuilder);
            ConfigureComments(modelBuilder);
            ConfigureFavorites(modelBuilder);
        }

        private static void ConfigureUsers(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            { 
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(x => x.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.AvatarPath)
                    .HasMaxLength(500);

                entity.Property(x => x.EmailConfirmationToken)
                    .HasMaxLength(200);

                entity.Property(x => x.WarningCount)
                    .IsRequired()
                    .HasDefaultValue((short)0);

                entity.Property(x => x.AccountStatus)
                    .IsRequired()
                    .HasConversion<int>()
                    .HasDefaultValue(AccountStatus.Active);

                entity.Property(x => x.Role)
                    .IsRequired()
                    .HasConversion<int>()
                    .HasDefaultValue(UserRole.User);

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.Property(x => x.UpdatedAt)
                    .IsRequired();

                entity.Property(x => x.DeletedAt)
                    .IsRequired(false);

                entity.HasIndex(x => x.Username)
                    .IsUnique();

                entity.HasIndex(x => x.Email)
                    .IsUnique();
            });
        }

        private static void ConfigureRatings(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rating>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.MediaType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.Score)
                    .IsRequired();

                entity.HasIndex(x => new { x.UserId, x.MediaId, x.MediaType })
                    .IsUnique();

                entity.HasOne(x => x.User)
                    .WithMany(x => x.Ratings)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureComments(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.MediaType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.Content)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.HasOne(x => x.User)
                    .WithMany(x => x.Comments)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureFavorites(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.MediaType)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.Title)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(x => x.PosterPath)
                    .HasMaxLength(500);

                entity.HasIndex(x => new { x.UserId, x.MediaId, x.MediaType })
                    .IsUnique();

                entity.HasOne(x => x.User)
                    .WithMany(x => x.Favorites)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
