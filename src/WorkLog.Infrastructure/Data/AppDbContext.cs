using Microsoft.EntityFrameworkCore;
using WorkLog.Domain.Entities;

namespace WorkLog.Infrastructure.Data;

/// <summary>
/// 應用程式資料庫上下文
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<WorkLogEntry> WorkLogEntries => Set<WorkLogEntry>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<WorkType> WorkTypes => Set<WorkType>();
    public DbSet<ProcessStatus> ProcessStatuses => Set<ProcessStatus>();
    public DbSet<WorkLogDepartment> WorkLogDepartments => Set<WorkLogDepartment>();
    public DbSet<WorkLogWorkType> WorkLogWorkTypes => Set<WorkLogWorkType>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<TodoCategory> TodoCategories => Set<TodoCategory>();
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<TodoSubTask> TodoSubTasks => Set<TodoSubTask>();
    public DbSet<TodoAttachment> TodoAttachments => Set<TodoAttachment>();
    public DbSet<TodoComment> TodoComments => Set<TodoComment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Role).HasConversion<string>().HasMaxLength(20);
        });

        // WorkLogEntry
        modelBuilder.Entity<WorkLogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Content).HasMaxLength(5000).IsRequired();
            entity.Property(e => e.WorkHours).HasPrecision(5, 2);
            entity.HasIndex(e => e.RecordDate);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.WorkLogEntries)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.WorkLogEntries)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ProcessStatus)
                .WithMany(s => s.WorkLogEntries)
                .HasForeignKey(e => e.ProcessStatusId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // WorkLogDepartment (多對多)
        modelBuilder.Entity<WorkLogDepartment>(entity =>
        {
            entity.HasKey(e => new { e.WorkLogEntryId, e.DepartmentId });

            entity.HasOne(e => e.WorkLogEntry)
                .WithMany(w => w.WorkLogDepartments)
                .HasForeignKey(e => e.WorkLogEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.WorkLogDepartments)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // WorkLogWorkType (多對多)
        modelBuilder.Entity<WorkLogWorkType>(entity =>
        {
            entity.HasKey(e => new { e.WorkLogEntryId, e.WorkTypeId });

            entity.HasOne(e => e.WorkLogEntry)
                .WithMany(w => w.WorkLogWorkTypes)
                .HasForeignKey(e => e.WorkLogEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.WorkType)
                .WithMany(t => t.WorkLogWorkTypes)
                .HasForeignKey(e => e.WorkTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Department
        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // WorkType
        modelBuilder.Entity<WorkType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ProcessStatus
        modelBuilder.Entity<ProcessStatus>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // RefreshToken
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TodoCategory
        modelBuilder.Entity<TodoCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ColorCode).HasMaxLength(7).IsRequired();
            entity.Property(e => e.Icon).HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // TodoItem
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.DueDate);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.TodoItems)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // TodoSubTask
        modelBuilder.Entity<TodoSubTask>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.TodoItemId);

            entity.HasOne(e => e.TodoItem)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(e => e.TodoItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TodoAttachment
        modelBuilder.Entity<TodoAttachment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ContentType).HasMaxLength(100).IsRequired();
            entity.HasIndex(e => e.TodoItemId);

            entity.HasOne(e => e.TodoItem)
                .WithMany(t => t.Attachments)
                .HasForeignKey(e => e.TodoItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TodoComment
        modelBuilder.Entity<TodoComment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).HasMaxLength(1000).IsRequired();
            entity.HasIndex(e => e.TodoItemId);
            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.TodoItem)
                .WithMany(t => t.Comments)
                .HasForeignKey(e => e.TodoItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed Data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // 預設工作類型
        var workTypes = new[]
        {
            "修改", "分析", "測試", "Bug", "開發", "優化",
            "文件", "資料確認", "上課", "業務單位會議或溝通", "資料整理"
        };
        for (int i = 0; i < workTypes.Length; i++)
        {
            modelBuilder.Entity<WorkType>().HasData(new WorkType
            {
                Id = i + 1,
                Name = workTypes[i],
                IsActive = true,
                SortOrder = i + 1
            });
        }

        // 預設處理狀態
        var statuses = new[] { "尚未進行", "擱置", "處理中", "已完成" };
        for (int i = 0; i < statuses.Length; i++)
        {
            modelBuilder.Entity<ProcessStatus>().HasData(new ProcessStatus
            {
                Id = i + 1,
                Name = statuses[i],
                IsActive = true,
                SortOrder = i + 1
            });
        }

        // 預設管理員帳號 (密碼: Admin@123)
        // PasswordHash 使用預先計算的靜態值以避免 EF Core model 變動警告
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@worklog.local",
            PasswordHash = "$2a$11$PXJswxf4o10iq2vzhkCJE.E7D3iWySGWips2a7SJAXsv0P0Zt2dvm", // Admin@123
            Role = Domain.Enums.UserRole.Admin,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // 預設待辦事項分類
        var todoCategories = new[]
        {
            new { Name = "工作", ColorCode = "#3B82F6", Icon = "briefcase" },
            new { Name = "個人", ColorCode = "#10B981", Icon = "user" },
            new { Name = "學習", ColorCode = "#8B5CF6", Icon = "book" },
            new { Name = "其他", ColorCode = "#6B7280", Icon = "folder" }
        };
        for (int i = 0; i < todoCategories.Length; i++)
        {
            modelBuilder.Entity<TodoCategory>().HasData(new TodoCategory
            {
                Id = i + 1,
                Name = todoCategories[i].Name,
                ColorCode = todoCategories[i].ColorCode,
                Icon = todoCategories[i].Icon,
                IsActive = true,
                SortOrder = i + 1
            });
        }
    }
}
