Imports System.Collections.Generic

Public Interface IProjectRepository
    Function GetById(id As Guid) As Project
    Function Add(project As Project) As Project
    Sub Delete(project As Project)
    Function GetProjectsByUserId(userId As Guid) As List(Of Project)
    Function GetProjectMembers(projectId As Guid) As List(Of ProjectMember)
    Function GetMember(projectId As Guid, userId As Guid) As ProjectMember
    Sub AddMember(member As ProjectMember)
    Sub RemoveMember(member As ProjectMember)
    Sub Save()
End Interface
