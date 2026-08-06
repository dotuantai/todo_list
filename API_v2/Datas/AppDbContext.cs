using Microsoft.EntityFrameworkCore;
using API_v2.Models;

namespace API_v2.Datas
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TodoTask> Tasks { get; set; }
        public DbSet<TaskAssignment> TaskAssignments { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectMember> ProjectMembers { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ProjectColumn> ProjectColumns { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<TaskActivity> TaskActivities { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Project>()
                .HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectMember>()
                .HasIndex(pm => new { pm.ProjectId, pm.UserId })
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            modelBuilder.Entity<TaskAssignment>()
                .HasKey(ta => new { ta.TaskId, ta.UserId });

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.Task)
                .WithMany(t => t.Assignments)
                .HasForeignKey(ta => ta.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskAssignment>()
                .HasOne(ta => ta.User)
                .WithMany(u => u.TaskAssignments)
                .HasForeignKey(ta => ta.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TodoTask>()
                .HasOne(t => t.Creator)
                .WithMany(u => u.CreatedTasks)
                .HasForeignKey(t => t.CreatorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TodoTask>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TodoTask>()
                .HasOne(t => t.Column)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.ColumnId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProjectColumn>()
                .HasOne(c => c.Project)
                .WithMany(p => p.Columns)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TodoTask>()
                .Property(t => t.Priority)
                .HasDefaultValue(API_v2.Models.Enums.TaskPriority.Medium)
                .HasConversion<string>();

            // Add missing indexes for performance optimization
            modelBuilder.Entity<TodoTask>()
                .HasIndex(t => t.ProjectId)
                .HasDatabaseName("IX_Tasks_ProjectId");

            modelBuilder.Entity<TodoTask>()
                .HasIndex(t => t.CreatorId)
                .HasDatabaseName("IX_Tasks_CreatorId");

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.UserId)
                .HasDatabaseName("IX_Notifications_UserId");

            modelBuilder.Entity<Notification>()
                .HasIndex(n => new { n.UserId, n.IsRead })
                .HasDatabaseName("IX_Notifications_UserId_IsRead");

            modelBuilder.Entity<ProjectMember>()
                .HasIndex(pm => pm.ProjectId)
                .HasDatabaseName("IX_ProjectMembers_ProjectId");

            modelBuilder.Entity<ProjectMember>()
                .HasIndex(pm => pm.UserId)
                .HasDatabaseName("IX_ProjectMembers_UserId");

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId)
                .HasDatabaseName("IX_RefreshTokens_UserId");

            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.Task)
                .WithMany(t => t.Comments)
                .HasForeignKey(tc => tc.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskComment>()
                .HasOne(tc => tc.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(tc => tc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskActivity>()
                .HasOne(ta => ta.Task)
                .WithMany()
                .HasForeignKey(ta => ta.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskActivity>()
                .HasOne(ta => ta.User)
                .WithMany()
                .HasForeignKey(ta => ta.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TaskActivity>()
                .HasIndex(ta => ta.TaskId)
                .HasDatabaseName("IX_TaskActivities_TaskId");

            // Role Configuration & Seeding
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            var adminRoleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var managerRoleId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var memberRoleId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = adminRoleId, Name = "Admin" },
                new Role { Id = managerRoleId, Name = "Manager" },
                new Role { Id = memberRoleId, Name = "Member" }
            );

            var adminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = adminUserId,
                    Email = "admin@gmail.com",
                    PasswordHash = "$2a$11$X3uPA9Yy730DZgeFEHKiiuSfcUtkQRjEDfRuPzDVbFwUGYio17M92",
                    IsActive = true,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    RoleId = adminRoleId
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
