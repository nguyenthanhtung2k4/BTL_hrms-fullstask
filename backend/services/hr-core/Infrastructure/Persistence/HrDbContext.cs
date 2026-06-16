using Hrms.HrCore.Domain.Entities;
using Hrms.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Hrms.HrCore.Infrastructure.Persistence;

public class HrDbContext : DbContext
{
    private readonly UpdateAuditableEntitiesInterceptor _auditInterceptor;

    public HrDbContext(
        DbContextOptions<HrDbContext> options,
        UpdateAuditableEntitiesInterceptor auditInterceptor) : base(options)
    {
        _auditInterceptor = auditInterceptor;
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<EmployeeStatusHistory> EmployeeStatusHistories => Set<EmployeeStatusHistory>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Departments
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            
            entity.HasOne(e => e.ParentDepartment)
                .WithMany(p => p.ChildDepartments)
                .HasForeignKey(e => e.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ManagerEmployee)
                .WithMany()
                .HasForeignKey(e => e.ManagerEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Positions
        modelBuilder.Entity<Position>(entity =>
        {
            entity.ToTable("Positions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // Employees
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Manager)
                .WithMany(m => m.Subordinates)
                .HasForeignKey(e => e.ManagerEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(e => e.Employee)
                .WithOne(emp => emp.User)
                .HasForeignKey<User>(u => u.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Roles
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        // UserRoles
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Contracts
        modelBuilder.Entity<Contract>(entity =>
        {
            entity.ToTable("Contracts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ContractNo).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(e => e.Employee)
                .WithMany(emp => emp.Contracts)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmployeeStatusHistories
        modelBuilder.Entity<EmployeeStatusHistory>(entity =>
        {
            entity.ToTable("EmployeeStatusHistories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ChangedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(e => e.Employee)
                .WithMany(emp => emp.StatusHistories)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ChangedByUser)
                .WithMany()
                .HasForeignKey(e => e.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AuditLogs
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

            entity.HasOne(e => e.ActorUser)
                .WithMany(u => u.AuditLogs)
                .HasForeignKey(e => e.ActorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OutboxMessages
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OccurredAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });
    }
}
