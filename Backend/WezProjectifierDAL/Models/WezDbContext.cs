using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WezProjectifier.Models;

public partial class WezDbContext : DbContext
{
    public WezDbContext()
    {
    }

    public WezDbContext(DbContextOptions<WezDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AssignedTask> AssignedTasks { get; set; }

    public virtual DbSet<Member> Members { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectTask> Tasks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=WezDB;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssignedTask>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.MemberId).HasColumnName("MemberID");
            entity.Property(e => e.TaskId).HasColumnName("TaskID");

            entity.HasOne(d => d.Member).WithMany(p => p.AssignedTasks)
                .HasForeignKey(d => d.MemberId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AssignedTasks_Member");

            entity.HasOne(d => d.Task).WithMany(p => p.AssignedTasks)
                .HasForeignKey(d => d.TaskId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AssignedTasks_Tasks");
        });

        modelBuilder.Entity<Member>(entity =>
        {
            entity.ToTable("Member");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.Role).HasMaxLength(10);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Project).WithMany(p => p.Members)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Member_Projects");

            entity.HasOne(d => d.User).WithMany(p => p.Members)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Member_User");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Projects_User");
        });

        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.Priority).HasMaxLength(25);
            entity.Property(e => e.ProjectId).HasColumnName("ProjectID");
            entity.Property(e => e.Status).HasMaxLength(25);

            entity.HasOne(d => d.Project).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tasks_Projects");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Address).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Firstname).HasMaxLength(50);
            entity.Property(e => e.Lastname).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(256);
            entity.Property(e => e.Phonenumber).HasMaxLength(18);
            entity.Property(e => e.Username).HasMaxLength(50);

            entity.HasIndex(e => e.Email, "UQ_CONSTRAINT_EMAIL").IsUnique();
            entity.HasIndex(e => e.Username, "UQ_CONSTRAINT_USER").IsUnique();
            entity.HasIndex(e => e.Phonenumber, "UQ_CONSTRAINT_Phonenumber").IsUnique();

        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
