Imports System.Collections.Generic

Public Interface IProjectService
    Function CreateProject(req As CreateProjectRequest, currentUserId As Guid) As ProjectResponse
    Function GetProjectsForUser(currentUserId As Guid) As List(Of ProjectResponse)
    Function GetProjectDetail(projectId As Guid, currentUserId As Guid) As ProjectResponse
    Function UpdateProject(projectId As Guid, req As UpdateProjectRequest, currentUserId As Guid) As ProjectResponse
    Sub DeleteProject(projectId As Guid, currentUserId As Guid)
    
    Function GetMembers(projectId As Guid) As List(Of MemberResponse)
    Function AddMember(projectId As Guid, req As AddMemberRequest, currentUserId As Guid) As MemberResponse
    Function UpdateMemberRole(projectId As Guid, userId As Guid, req As UpdateMemberRequest, currentUserId As Guid) As MemberResponse
    Sub RemoveMember(projectId As Guid, userId As Guid, currentUserId As Guid)
End Interface
