using Microsoft.EntityFrameworkCore;
using Hrms.PayrollReport.Domain.Entities;

namespace Hrms.PayrollReport.Infrastructure.Persistence;

public class PayrollReportDbContext : DbContext
{
    public PayrollReportDbContext(DbContextOptions<PayrollReportDbContext> options) : base(options)
    {
    }

    public DbSet<DepartmentProjection> DepartmentProjections => Set<DepartmentProjection>();
    public DbSet<PositionProjection> PositionProjections => Set<PositionProjection>();
    public DbSet<EmployeeProjection> EmployeeProjections => Set<EmployeeProjection>();
    public DbSet<EmployeeSalaryProjection> EmployeeSalaryProjections => Set<EmployeeSalaryProjection>();
    public DbSet<AttendanceProjection> AttendanceProjections => Set<AttendanceProjection>();
    public DbSet<LeaveProjection> LeaveProjections => Set<LeaveProjection>();
    public DbSet<PayrollRule> PayrollRules => Set<PayrollRule>();
    public DbSet<PayrollPeriod> PayrollPeriods => Set<PayrollPeriod>();
    public DbSet<AllowanceType> AllowanceTypes => Set<AllowanceType>();
    public DbSet<DeductionType> DeductionTypes => Set<DeductionType>();
    public DbSet<EmployeeAllowance> EmployeeAllowances => Set<EmployeeAllowance>();
    public DbSet<EmployeeDeduction> EmployeeDeductions => Set<EmployeeDeduction>();
    public DbSet<Payslip> Payslips => Set<Payslip>();
    public DbSet<PayslipItem> PayslipItems => Set<PayslipItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // DepartmentProjection
        modelBuilder.Entity<DepartmentProjection>(entity =>
        {
            entity.ToTable("DepartmentProjections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("DepartmentId");
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // PositionProjection
        modelBuilder.Entity<PositionProjection>(entity =>
        {
            entity.ToTable("PositionProjections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("PositionId");
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // EmployeeProjection
        modelBuilder.Entity<EmployeeProjection>(entity =>
        {
            entity.ToTable("EmployeeProjections");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.Id).HasColumnName("EmployeeId");
            entity.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();
            entity.HasIndex(e => e.EmployeeCode).IsUnique();

            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Position)
                .WithMany()
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerEmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmployeeSalaryProjection
        modelBuilder.Entity<EmployeeSalaryProjection>(entity =>
        {
            entity.ToTable("EmployeeSalaryProjections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AttendanceProjection
        modelBuilder.Entity<AttendanceProjection>(entity =>
        {
            entity.ToTable("AttendanceProjections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("AttendanceRecordId");
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // LeaveProjection
        modelBuilder.Entity<LeaveProjection>(entity =>
        {
            entity.ToTable("LeaveProjections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("LeaveRequestId");

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PayrollRule
        modelBuilder.Entity<PayrollRule>(entity =>
        {
            entity.ToTable("PayrollRules");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // PayrollPeriod
        modelBuilder.Entity<PayrollPeriod>(entity =>
        {
            entity.ToTable("PayrollPeriods");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();

            entity.HasOne(e => e.PayrollRule)
                .WithMany()
                .HasForeignKey(e => e.PayrollRuleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // AllowanceType
        modelBuilder.Entity<AllowanceType>(entity =>
        {
            entity.ToTable("AllowanceTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // DeductionType
        modelBuilder.Entity<DeductionType>(entity =>
        {
            entity.ToTable("DeductionTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // EmployeeAllowance
        modelBuilder.Entity<EmployeeAllowance>(entity =>
        {
            entity.ToTable("EmployeeAllowances");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PayrollPeriod)
                .WithMany()
                .HasForeignKey(e => e.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AllowanceType)
                .WithMany()
                .HasForeignKey(e => e.AllowanceTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EmployeeDeduction
        modelBuilder.Entity<EmployeeDeduction>(entity =>
        {
            entity.ToTable("EmployeeDeductions");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PayrollPeriod)
                .WithMany()
                .HasForeignKey(e => e.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.DeductionType)
                .WithMany()
                .HasForeignKey(e => e.DeductionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payslip
        modelBuilder.Entity<Payslip>(entity =>
        {
            entity.ToTable("Payslips");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasMaxLength(30).IsRequired();

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PayrollPeriod)
                .WithMany()
                .HasForeignKey(e => e.PayrollPeriodId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PayslipItem
        modelBuilder.Entity<PayslipItem>(entity =>
        {
            entity.ToTable("PayslipItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ItemType).HasMaxLength(30).IsRequired();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();

            entity.HasOne(e => e.Payslip)
                .WithMany(p => p.Items)
                .HasForeignKey(e => e.PayslipId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
