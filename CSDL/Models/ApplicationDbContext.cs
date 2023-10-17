
using Microsoft.EntityFrameworkCore;

namespace CSDL.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        public async Task<User> GetUserByAccountIdAsync(int accountId)
        {
            return await Users.FirstOrDefaultAsync(u => u.accountId == accountId);
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<History> Histories { get; set; } = null!;
        public DbSet<Image> Images { get; set; } = null!;
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<Message> Messages { get; set; } = null!;
        public DbSet<Account> Accounts { get; set; } = null!;
        //public DbSet<OtherUser> OtherUsers { get; set; } = null!;
        public DbSet<Relation> Relations { get; set; } = null!;

        //public DbSet<DislikedUser> DislikedUsers { get; set; } = null!;
        //public DbSet<UserDislikedRelation> UserDislikedRelations { get; set; } = null!;
        //public DbSet<LikeUser> LikeUsers { get; set; }
        //public DbSet<UserLikeRelation> UserLikeRelations { get; set; }

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

            modelBuilder.Entity<Relation>()
                .HasOne(r => r.User)
                .WithMany(u => u.UserRelations)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Relation>()
                .HasOne(r => r.OtherUser)
                .WithMany(u => u.OtherUserRelations)
                .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Relation>()
            //    .HasOne(ud => ud.User)
            //    .WithMany(u => u.Relations)
            //    .HasForeignKey(ud => ud.UserID);

            //modelBuilder.Entity<Relation>()
            //    .HasOne(ud => ud.OtherUser)
            //    .WithMany(u => u.Relations)
            //    .HasForeignKey(ud => ud.OtherUserId);

            //modelBuilder.Entity<Relation>()
            //   .HasOne(m => m.CurrentUser)
            //   .WithMany(u => u.Relations)
            //   .HasForeignKey(m => m.CurrentUserId)
            //  .OnDelete(DeleteBehavior.Restrict); // Tùy chọn xóa sẽ phụ thuộc vào yêu cầu của bạn

            //modelBuilder.Entity<Relation>()
            //   .HasOne(m => m.OtherUser)
            //   .WithMany()
            //   .HasForeignKey(m => m.OtherUserId)
            //  .OnDelete(DeleteBehavior.Restrict);


            //modelBuilder.Entity<UserDislikedRelation>()
            //.HasKey(ud => new { ud.UserID, ud.DislikedUserID });

            //modelBuilder.Entity<UserDislikedRelation>()
            //    .HasOne(ud => ud.User)
            //    .WithMany(u => u.UserDislikedRelations)
            //    .HasForeignKey(ud => ud.UserID);

            //modelBuilder.Entity<UserDislikedRelation>()
            //    .HasOne(ud => ud.DislikedUser)
            //    .WithMany(u => u.UserDislikedRelations)
            //    .HasForeignKey(ud => ud.DislikedUserID);

            //modelBuilder.Entity<UserLikeRelation>()
            //    .HasOne(ul => ul.User)
            //    .WithMany(u => u.UserLikeRelations)
            //    .HasForeignKey(ul => ul.UserID);

            //modelBuilder.Entity<UserLikeRelation>()
            //    .HasOne(ul => ul.LikeUser)
            //    .WithMany(u => u.UserLikeRelations)
            //    .HasForeignKey(ul => ul.LikeUserID);

        }
    }
}
