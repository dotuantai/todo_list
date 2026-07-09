Imports System.Data.Entity.ModelConfiguration

Public Class ProjectMemberConfiguration
    Inherits EntityTypeConfiguration(Of ProjectMember)

    Public Sub New()
        ToTable("project_members")

        HasKey(Function(x) x.Id)

        [Property](Function(x) x.ProjectId) _
            .HasColumnName("project_id") _
            .IsRequired()

        [Property](Function(x) x.UserId) _
            .HasColumnName("user_id") _
            .IsRequired()

        [Property](Function(x) x.Role) _
            .HasColumnName("role") _
            .IsRequired() _
            .HasMaxLength(50)

        [Property](Function(x) x.JoinedAt) _
            .HasColumnName("joined_at") _
            .IsRequired()

        ' Cascades delete for project (when a project is deleted, its member entries should go)
        HasRequired(Function(x) x.Project) _
            .WithMany(Function(x) x.ProjectMembers) _
            .HasForeignKey(Function(x) x.ProjectId) _
            .WillCascadeOnDelete(True)

        ' Does not cascade delete for user to prevent SQL cycle issues, managed in app layer if needed
        HasRequired(Function(x) x.User) _
            .WithMany(Function(x) x.ProjectMembers) _
            .HasForeignKey(Function(x) x.UserId) _
            .WillCascadeOnDelete(False)
    End Sub
End Class
