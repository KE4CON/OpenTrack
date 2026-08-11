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
            e.Property(i => i.IntakeName).HasMaxLength(FieldLimits.IntakeName);
            e.Property(i => i.IntakeEmail).HasMaxLength(FieldLimits.IntakeEmail);
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
            e.HasIndex(i => new { i.ProjectId, i.ImportedMantisId }); // dedup lookup on re-import
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

        // ---- IssueMonitor / Notification ----
        b.Entity<IssueMonitor>(e =>
        {
            e.HasKey(m => new { m.IssueId, m.UserId });
            e.HasOne(m => m.Issue).WithMany().HasForeignKey(m => m.IssueId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(m => m.User).WithMany().HasForeignKey(m => m.UserId).OnDelete(DeleteBehavior.Cascade);
        });
        b.Entity<Notification>(e =>
        {
            e.Property(n => n.Text).IsRequired();
            e.HasOne(n => n.User).WithMany().HasForeignKey(n => n.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(n => n.Issue).WithMany().HasForeignKey(n => n.IssueId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(n => new { n.UserId, n.IsRead });
        });

        // ---- CustomFieldDefinition / CustomFieldValue ----
        b.Entity<CustomFieldDefinition>(e =>
        {
            e.Property(d => d.Name).HasMaxLength(FieldLimits.CustomFieldName).IsRequired();
            e.Property(d => d.EnumOptions).HasMaxLength(FieldLimits.CustomFieldEnumOptions);
            e.HasOne(d => d.Project)
                .WithMany(p => p.CustomFields)
                .HasForeignKey(d => d.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(d => new { d.ProjectId, d.Name }).IsUnique();
        });
        // ---- ProjectWebhook ----
        b.Entity<ProjectWebhook>(e =>
        {
            e.Property(w => w.Url).HasMaxLength(FieldLimits.WebhookUrl).IsRequired();
            e.HasOne(w => w.Project).WithMany().HasForeignKey(w => w.ProjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(w => w.ProjectId);
        });

        // ---- UserPreference (one row per user) ----
        b.Entity<UserPreference>(e =>
        {
            e.HasKey(p => p.UserId);
            e.HasOne(p => p.User).WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            // DefaultProjectId is a soft reference (set-null semantics handled in app): if the project is
            // gone we just ignore it, so no FK is declared to avoid a delete-time constraint.
        });

        // ---- WorkflowTransition ----
        b.Entity<WorkflowTransition>(e =>
        {
            e.HasOne(w => w.Project).WithMany().HasForeignKey(w => w.ProjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(w => new { w.ProjectId, w.FromStatus, w.ToStatus }).IsUnique();
        });

        // ---- AutomationRule ----
        b.Entity<AutomationRule>(e =>
        {
            e.HasOne(r => r.Project).WithMany().HasForeignKey(r => r.ProjectId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(r => new { r.ProjectId, r.SortOrder });
            e.Property(r => r.Name).HasMaxLength(FieldLimits.ProjectName);
            e.Property(r => r.WhenTextContains).HasMaxLength(FieldLimits.IssueTitle);
            e.Property(r => r.AddTag).HasMaxLength(FieldLimits.TagName);
        });

        // ---- TimeLog ----
        b.Entity<TimeLog>(e =>
        {
            e.Property(t => t.Note).HasMaxLength(FieldLimits.TimeLogNote);
            e.HasOne(t => t.Issue).WithMany().HasForeignKey(t => t.IssueId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(t => t.User).WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(t => t.IssueId);
        });

        // ---- SavedFilter (per-user) ----
        b.Entity<SavedFilter>(e =>
        {
            e.Property(s => s.Name).HasMaxLength(FieldLimits.SavedFilterName).IsRequired();
            e.Property(s => s.Query).HasMaxLength(FieldLimits.SavedFilterQuery);
            e.HasOne(s => s.User).WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(s => new { s.UserId, s.Name }).IsUnique();
        });

        // ---- ChecklistItem ----
        b.Entity<ChecklistItem>(e =>
        {
            e.Property(c => c.Title).HasMaxLength(FieldLimits.ChecklistTitle).IsRequired();
            e.Property(c => c.Area).HasMaxLength(FieldLimits.ChecklistArea);
            e.Property(c => c.Details).HasMaxLength(FieldLimits.ChecklistText);
            e.Property(c => c.Notes).HasMaxLength(FieldLimits.ChecklistText);
            e.HasOne(c => c.Project)
                .WithMany(p => p.ChecklistItems)
                .HasForeignKey(c => c.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
            // The link to a spawned issue just nulls out if that issue is ever deleted (only the
            // ProjectId FK cascades, so there's no multiple-cascade-path problem on SQL Server).
            e.HasOne(c => c.LinkedIssue)
                .WithMany()
                .HasForeignKey(c => c.LinkedIssueId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(c => c.ProjectId);
        });

        b.Entity<CustomFieldValue>(e =>
        {
            e.HasKey(v => new { v.IssueId, v.CustomFieldDefinitionId });
            e.Property(v => v.Value).HasMaxLength(FieldLimits.CustomFieldValue);
            // A project cascades to BOTH its issues and its custom-field definitions, each of which
            // would then cascade into this table — two cascade paths to one table, which SQL Server
            // rejects at schema creation. So the Definition FK cascades (deleting a field removes its
            // values, which DeleteDefinitionAsync relies on) and the Issue FK restricts. Nothing
            // deletes issues in the app today; WHEN an issue-delete feature is added it must first
            // clear that issue's custom-field values (same rule as IssueRelationship above).
            e.HasOne(v => v.Definition).WithMany(d => d.Values).HasForeignKey(v => v.CustomFieldDefinitionId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.Issue).WithMany(i => i.CustomFieldValues).HasForeignKey(v => v.IssueId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    public DbSet<IssueRelationship> IssueRelationships => Set<IssueRelationship>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<IssueTag> IssueTags => Set<IssueTag>();
    public DbSet<IssueMonitor> IssueMonitors => Set<IssueMonitor>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<CustomFieldValue> CustomFieldValues => Set<CustomFieldValue>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<SavedFilter> SavedFilters => Set<SavedFilter>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<ProjectWebhook> ProjectWebhooks => Set<ProjectWebhook>();
    public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
    public DbSet<WorkflowTransition> WorkflowTransitions => Set<WorkflowTransition>();
    public DbSet<AutomationRule> AutomationRules => Set<AutomationRule>();
}
