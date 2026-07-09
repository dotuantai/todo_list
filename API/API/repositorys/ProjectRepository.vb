Imports System.Data.Entity
Imports System.Linq
Imports System.Collections.Generic

Public Class ProjectRepository
    Implements IProjectRepository

    Private ReadOnly _dbContext As AppDbContext

    Public Sub New(dbContext As AppDbContext)
        _dbContext = dbContext
    End Sub

    Public Function GetById(id As Guid) As Project Implements IProjectRepository.GetById
        Return _dbContext.Projects.
            Include(Function(p) p.Owner).
            FirstOrDefault(Function(p) p.Id = id)
    End Function

    Public Function Add(project As Project) As Project Implements IProjectRepository.Add
        Return _dbContext.Projects.Add(project)
    End Function

    Public Sub Delete(project As Project) Implements IProjectRepository.Delete
        _dbContext.Projects.Remove(project)
    End Sub

    Public Function GetProjectsByUserId(userId As Guid) As List(Of Project) Implements IProjectRepository.GetProjectsByUserId
        ' Fetch projects where the user is listed in project_members
        Return _dbContext.ProjectMembers.
            Where(Function(pm) pm.UserId = userId).
            Select(Function(pm) pm.Project).
            Include(Function(p) p.Owner).
            ToList()
    End Function

    Public Function GetProjectMembers(projectId As Guid) As List(Of ProjectMember) Implements IProjectRepository.GetProjectMembers
        Return _dbContext.ProjectMembers.
            Where(Function(pm) pm.ProjectId = projectId).
            Include(Function(pm) pm.User).
            ToList()
    End Function

    Public Function GetMember(projectId As Guid, userId As Guid) As ProjectMember Implements IProjectRepository.GetMember
        Return _dbContext.ProjectMembers.
            FirstOrDefault(Function(pm) pm.ProjectId = projectId AndAlso pm.UserId = userId)
    End Function

    Public Sub AddMember(member As ProjectMember) Implements IProjectRepository.AddMember
        _dbContext.ProjectMembers.Add(member)
    End Sub

    Public Sub RemoveMember(member As ProjectMember) Implements IProjectRepository.RemoveMember
        _dbContext.ProjectMembers.Remove(member)
    End Sub

    Public Sub Save() Implements IProjectRepository.Save
        _dbContext.SaveChanges()
    End Sub
End Class
