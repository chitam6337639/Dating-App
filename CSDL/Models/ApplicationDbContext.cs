using Microsoft.EntityFrameworkCore;

namespace CSDL.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<History> Histories { get; set; } = null!;
        public DbSet<Image> Images { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            //    // Cấu hình mối quan hệ giữa các lớp ở đây

            modelBuilder.Entity<Match>()
               .HasOne(m => m.User)
               .WithMany(u => u.Matches)
               .HasForeignKey(m => m.UserId)
              .OnDelete(DeleteBehavior.Restrict); // Tùy chọn xóa sẽ phụ thuộc vào yêu cầu của bạn

            modelBuilder.Entity<Match>()
                .HasOne(m => m.TargetUser)
                .WithMany()
                .HasForeignKey(m => m.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
           .HasOne(m => m.UserTo) // Thay vì UserIdTo, sử dụng UserTo làm navigation property
           .WithMany(u => u.Messages) // Chỉ định navigation property trong User
           .HasForeignKey(m => m.UserIdTo)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.UserFrom)
                .WithMany()
                .HasForeignKey(m => m.UserIdFrom) 
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<History>()
            .HasOne(h => h.User)
            .WithMany(u => u.Histories)
            .HasForeignKey(h => h.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.User)
                .WithMany(u => u.Images)
                .HasForeignKey(i => i.userId);
        }
    }
}
