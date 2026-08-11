// OpenTrack — open-source issue tracker
// Copyright (C) 2026 KE4CON
//
// This program is free software: you can redistribute it and/or modify it under
// the terms of the GNU Affero General Public License as published by the Free
// Software Foundation, either version 3 of the License, or (at your option) any
// later version. This program is distributed WITHOUT ANY WARRANTY; without even
// the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License <https://www.gnu.org/licenses/> for
// more details.

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OpenTrack.Core.Entities;
using OpenTrack.Core.Validation;

namespace OpenTrack.Infrastructure.Data;

// IdentityUserContext (not IdentityDbContext): OpenTrack authorization is driven entirely by the
// custom UserRole enum (User.Role) and ProjectMembership, never ASP.NET Identity's role store, so we
// omit the AspNetRoles/AspNetUserRoles/AspNetRoleClaims tables. The .NET 10 passkey table is kept
// (IdentityUserContext includes it at Identity SchemaVersion v3, which AddOpenTrackIdentity and the
// design-time factory both configure).
public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityUserContext<User, int>(options)
{
    // Users, the user claim/login/token tables, and the passkey table come from IdentityUserContext.
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMembership> ProjectMemberships => Set<ProjectMembership>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProjectVersion> Versions => Set<ProjectVersion>();
    public DbSet<Issue> Issues => Set<Issue>();
    public DbSet<IssueNote> IssueNotes => Set<IssueNote>();
    public DbSet<IssueHistory> IssueHistories => Set<IssueHistory>();
    public DbSet<IssueAttachment> IssueAttachments => Set<IssueAttachment>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // ---- Project ----
        b.Entity<Project>(e =>
        {
            e.Property(p => p.Name).HasMaxLength(FieldLimits.ProjectName).IsRequired();
            e.Property(p => p.Description).HasMaxLength(FieldLimits.Description);
            e.Property(p => p.RowVersion).IsConcurrencyToken();
            e.HasOne(p => p.Owner)
                .WithMany()
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- ProjectMembership (composite key) ----
        b.Entity<ProjectMembership>(e =>
        {
            e.HasKey(m => new { m.UserId, m.ProjectId });
            e.HasOne(m => m.User)
                .WithMany(u => u.Memberships)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ---- Category ----
        b.Entity<Category>(e =>
        {
            e.Property(c => c.Name).HasMaxLength(FieldLimits.CategoryName).IsRequired();
            e.HasOne(c => c.Project)
                .WithMany(p => p.Categories)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(c => new { c.ProjectId, c.Name }).IsUnique();
        });

        // ---- ProjectVersion ----
        b.Entity<ProjectVersion>(e =>
        {
            e.Property(v => v.Name).HasMaxLength(FieldLimits.VersionName).IsRequired();
            e.Property(v => v.Description).HasMaxLength(FieldLimits.Description);
            e.HasOne(v => v.Project)
                .WithMany(p => p.Versions)
                .HasForeignKey(v => v.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(v => new { v.ProjectId, v.Name }).IsUnique();
        });

        // ---- Issue ----
        b.Entity<Issue>(e =>
        {
            e.Property(i => i.Title).HasMaxLength(FieldLimits.IssueTitle).IsRequired();
            e.Property(i => i.Description).IsRequired();
            e.Property(i => i.RowVersion).IsConcurrencyToken();

            e.HasOne(i => i.Project)
                .WithMany(p => p.Issues)
                .HasForeignKey(i => i.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.Category)
                .WithMany(c => c.Issues)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(i => i.Reporter)
                .WithMany(u => u.ReportedIssues)
                .HasForeignKey(i => i.ReporterId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.Assignee)
                .WithMany(u => u.AssignedIssues)
                .HasForeignKey(i => i.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(i => i.AffectsVersion)
                .WithMany()
                .HasForeignKey(i => i.AffectsVersionId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(i => i.FixVersion)
                .WithMany()
                .HasForeignKey(i => i.FixVersionId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasIndex(i => i.Status);
            e.HasIndex(i => i.ProjectId);
        });

        // ---- IssueNote ----
        b.Entity<IssueNote>(e =>
        {
            e.Property(n => n.Text).IsRequired();
            e.HasOne(n => n.Issue)
                .WithMany(i => i.Notes)
                .HasForeignKey(n => n.IssueId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.Author)
                .WithMany()
                .HasForeignKey(n => n.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- IssueHistory ----
        b.Entity<IssueHistory>(e =>
        {
            e.Property(h => h.FieldChanged).HasMaxLength(100).IsRequired();
            e.HasOne(h => h.Issue)
                .WithMany(i => i.History)
                .HasForeignKey(h => h.IssueId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- IssueAttachment ----
        b.Entity<IssueAttachment>(e =>
        {
            e.Property(a => a.FileName).HasMaxLength(260).IsRequired();
            e.Property(a => a.FilePath).HasMaxLength(1024).IsRequired();
            e.Property(a => a.ContentType).HasMaxLength(128).IsRequired();
            e.HasOne(a => a.Issue)
                .WithMany(i => i.Attachments)
                .HasForeignKey(a => a.IssueId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(a => a.UploadedBy)
                .WithMany()
                .HasForeignKey(a => a.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ---- IssueRelationship ----
        b.Entity<IssueRelationship>(e =>
        {
            // Source cascades, Target restricts: two cascade FKs to the same Issue table would be
            // "multiple cascade paths" on SQL Server, so only one cascades. Nothing deletes issues or
            // projects through the UI today. WHEN a delete feature is added, it must first clear
            // relationships where the doomed issue is the TARGET — this includes deleting a PROJECT,
            // whose cascade to its issues would otherwise trip this Restrict FK (even for a
            // same-project A→B link). Prefer an app-level pre-delete sweep of relationships then.
            e.HasOne(r => r.SourceIssue)
                .WithMany()
                .HasForeignKey(r => r.SourceIssueId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.TargetIssue)
                .WithMany()
                .HasForeignKey(r => r.TargetIssueId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(r => r.CreatedBy)
                .WithMany()
                .HasForeignKey(r => r.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
            // No duplicate identical relationships.
            e.HasIndex(r => new { r.SourceIssueId, r.TargetIssueId, r.Type }).IsUnique();
        });

        // ---- Tag / IssueTag ----
        b.Entity<Tag>(e =>
        {
            e.Property(t => t.Name).HasMaxLength(FieldLimits.TagName).IsRequired();
            e.HasIndex(t => t.Name).IsUnique();
        });
        b.Entity<IssueTag>(e =>
        {
            e.HasKey(it => new { it.IssueId, it.TagId });
            e.HasOne(it => it.Issue).WithMany(i => i.IssueTags).HasForeignKey(it => it.IssueId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(it => it.Tag).WithMany(t => t.IssueTags).HasForeignKey(it => it.TagId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    public DbSet<IssueRelationship> IssueRelationships => Set<IssueRelationship>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<IssueTag> IssueTags => Set<IssueTag>();
}
