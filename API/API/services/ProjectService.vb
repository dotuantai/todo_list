Imports System.Linq
Imports System.Collections.Generic

Public Class ProjectService
    Implements IProjectService

    Private ReadOnly _projectRepo As IProjectRepository
    Private ReadOnly _userRepo As IUserRepository

    Public Sub New(projectRepo As IProjectRepository, userRepo As IUserRepository)
        _projectRepo = projectRepo
        _userRepo = userRepo
    End Sub

    Public Function CreateProject(req As CreateProjectRequest, currentUserId As Guid) As ProjectResponse _
        Implements IProjectService.CreateProject

        If String.IsNullOrWhiteSpace(req.Name) Then
            Throw ApiException.BadRequest("Project name cannot be empty.")
        End If

        Dim user = _userRepo.GetById(currentUserId)
        If user Is Nothing Then
            Throw ApiException.Unauthorized("Account information not found.")
        End If

        Dim project = New Project With {
            .Id = Guid.NewGuid(),
            .Name = req.Name.Trim(),
            .Description = req.Description?.Trim(),
            .OwnerId = currentUserId,
            .CreatedAt = DateTime.UtcNow,
            .UpdatedAt = DateTime.UtcNow
        }

        _projectRepo.Add(project)

        ' Set the creator of the project as the first member with Owner role
        Dim member = New ProjectMember With {
            .Id = Guid.NewGuid(),
            .ProjectId = project.Id,
            .UserId = currentUserId,
            .Role = "Owner",
            .JoinedAt = DateTime.UtcNow
        }
        _projectRepo.AddMember(member)
        _projectRepo.Save()

        ' Reload project details
        Dim dbProject = _projectRepo.GetById(project.Id)

        Return MapToProjectResponse(dbProject, "Owner")
    End Function

    Public Function GetProjectsForUser(currentUserId As Guid) As List(Of ProjectResponse) _
        Implements IProjectService.GetProjectsForUser

        Dim projects = _projectRepo.GetProjectsByUserId(currentUserId)
        Dim responses = New List(Of ProjectResponse)()

        For Each p In projects
            Dim member = _projectRepo.GetMember(p.Id, currentUserId)
            responses.Add(MapToProjectResponse(p, member?.Role))
        Next

        Return responses
    End Function

    Public Function GetProjectDetail(projectId As Guid, currentUserId As Guid) As ProjectResponse _
        Implements IProjectService.GetProjectDetail

        Dim project = _projectRepo.GetById(projectId)
        If project Is Nothing Then
            Throw ApiException.NotFound("Project does not exist.")
        End If

        Dim member = _projectRepo.GetMember(projectId, currentUserId)
        If member Is Nothing Then
            Throw ApiException.Forbidden("You do not have access to this project.")
        End If

        Return MapToProjectResponse(project, member.Role)
    End Function

    Public Function UpdateProject(projectId As Guid, req As UpdateProjectRequest, currentUserId As Guid) As ProjectResponse _
        Implements IProjectService.UpdateProject

        Dim project = _projectRepo.GetById(projectId)
        If project Is Nothing Then
            Throw ApiException.NotFound("Project does not exist.")
        End If

        ' Authorization check (must be Owner)
        Dim member = _projectRepo.GetMember(projectId, currentUserId)
        If member Is Nothing OrElse Not member.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) Then
            Throw ApiException.Forbidden("Only the Owner is allowed to edit project information.")
        End If

        If String.IsNullOrWhiteSpace(req.Name) Then
            Throw ApiException.BadRequest("Project name cannot be empty.")
        End If

        project.Name = req.Name.Trim()
        project.Description = req.Description?.Trim()
        project.UpdatedAt = DateTime.UtcNow

        _projectRepo.Save()

        Return MapToProjectResponse(project, member.Role)
    End Function

    Public Sub DeleteProject(projectId As Guid, currentUserId As Guid) _
        Implements IProjectService.DeleteProject

        Dim project = _projectRepo.GetById(projectId)
        If project Is Nothing Then
            Throw ApiException.NotFound("Project does not exist.")
        End If

        ' Only the creator (Owner of project table) can delete the project
        If project.OwnerId <> currentUserId Then
            Throw ApiException.Forbidden("Only the project owner is allowed to delete the project.")
        End If

        _projectRepo.Delete(project)
        _projectRepo.Save()
    End Sub

    Public Function GetMembers(projectId As Guid) As List(Of MemberResponse) _
        Implements IProjectService.GetMembers

        Dim members = _projectRepo.GetProjectMembers(projectId)
        Return members.Select(Function(m) New MemberResponse With {
            .UserId = m.UserId,
            .Email = m.User?.Email,
            .Role = m.Role,
            .JoinedAt = m.JoinedAt
        }).ToList()
    End Function

    Public Function AddMember(projectId As Guid, req As AddMemberRequest, currentUserId As Guid) As MemberResponse _
        Implements IProjectService.AddMember

        If String.IsNullOrWhiteSpace(req.Role) OrElse
           (Not req.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) AndAlso
            Not req.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase) AndAlso
            Not req.Role.Equals("Viewer", StringComparison.OrdinalIgnoreCase)) Then
            Throw ApiException.BadRequest("Invalid role. Valid roles: Owner, Editor, Viewer.")
        End If

        Dim targetUser = _userRepo.GetByEmail(req.Email?.Trim())
        If targetUser Is Nothing Then
            Throw ApiException.NotFound($"No account found with email '{req.Email}'.")
        End If

        Dim existingMember = _projectRepo.GetMember(projectId, targetUser.Id)
        If existingMember IsNot Nothing Then
            Throw ApiException.Conflict("This user is already a member of the project.")
        End If

        Dim member = New ProjectMember With {
            .Id = Guid.NewGuid(),
            .ProjectId = projectId,
            .UserId = targetUser.Id,
            .Role = req.Role,
            .JoinedAt = DateTime.UtcNow
        }

        _projectRepo.AddMember(member)
        _projectRepo.Save()

        ' Refresh member to load target user relation
        Dim dbMember = _projectRepo.GetMember(projectId, targetUser.Id)

        Return New MemberResponse With {
            .UserId = dbMember.UserId,
            .Email = targetUser.Email,
            .Role = dbMember.Role,
            .JoinedAt = dbMember.JoinedAt
        }
    End Function

    Public Function UpdateMemberRole(projectId As Guid, userId As Guid, req As UpdateMemberRequest, currentUserId As Guid) As MemberResponse _
        Implements IProjectService.UpdateMemberRole

        If String.IsNullOrWhiteSpace(req.Role) OrElse
           (Not req.Role.Equals("Owner", StringComparison.OrdinalIgnoreCase) AndAlso
            Not req.Role.Equals("Editor", StringComparison.OrdinalIgnoreCase) AndAlso
            Not req.Role.Equals("Viewer", StringComparison.OrdinalIgnoreCase)) Then
            Throw ApiException.BadRequest("Invalid role.")
        End If

        Dim project = _projectRepo.GetById(projectId)
        If project Is Nothing Then
            Throw ApiException.NotFound("Project does not exist.")
        End If

        ' Prevent changing the primary Owner's role
        If project.OwnerId = userId Then
            Throw ApiException.BadRequest("Cannot change the role of the project owner.")
        End If

        Dim targetMember = _projectRepo.GetMember(projectId, userId)
        If targetMember Is Nothing Then
            Throw ApiException.NotFound("Member does not belong to this project.")
        End If

        targetMember.Role = req.Role
        _projectRepo.Save()

        Return New MemberResponse With {
            .UserId = targetMember.UserId,
            .Email = targetMember.User?.Email,
            .Role = targetMember.Role,
            .JoinedAt = targetMember.JoinedAt
        }
    End Function

    Public Sub RemoveMember(projectId As Guid, userId As Guid, currentUserId As Guid) _
        Implements IProjectService.RemoveMember

        Dim project = _projectRepo.GetById(projectId)
        If project Is Nothing Then
            Throw ApiException.NotFound("Project does not exist.")
        End If

        ' Prevent removing the primary Owner
        If project.OwnerId = userId Then
            Throw ApiException.BadRequest("Cannot remove the project owner from the project.")
        End If

        Dim targetMember = _projectRepo.GetMember(projectId, userId)
        If targetMember Is Nothing Then
            Throw ApiException.NotFound("Member does not belong to this project.")
        End If

        _projectRepo.RemoveMember(targetMember)
        _projectRepo.Save()
    End Sub

    Private Function MapToProjectResponse(p As Project, userRole As String) As ProjectResponse
        Return New ProjectResponse With {
            .Id = p.Id,
            .Name = p.Name,
            .Description = p.Description,
            .OwnerId = p.OwnerId,
            .OwnerEmail = p.Owner?.Email,
            .CreatedAt = p.CreatedAt,
            .UpdatedAt = p.UpdatedAt,
            .UserRole = userRole
        }
    End Function
End Class
