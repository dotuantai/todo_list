Imports System.Web.Http
Imports System.Net
Imports System.Collections.Generic

Namespace Controllers

    <RoutePrefix("api/projects")>
    <Authorize>
    Public Class ProjectController
        Inherits BaseApiController

        Private ReadOnly _projectService As IProjectService
        Private ReadOnly _taskService As ITaskService

        Public Sub New(projectService As IProjectService, taskService As ITaskService)
            _projectService = projectService
            _taskService = taskService
        End Sub

        <HttpGet>
        <Route("")>
        Public Function GetMyProjects() As IHttpActionResult
            Return Execute(Function() _projectService.GetProjectsForUser(CurrentUserId))
        End Function

        <HttpPost>
        <Route("")>
        Public Function Create(<FromBody> req As CreateProjectRequest) As IHttpActionResult
            If req Is Nothing Then
                Return Content(HttpStatusCode.BadRequest, New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
            End If
            Return Execute(Function() _projectService.CreateProject(req, CurrentUserId))
        End Function

        <HttpGet>
        <Route("{projectId:guid}")>
        <ProjectAuthorize>
        Public Function GetProjectDetail(projectId As Guid) As IHttpActionResult
            Return Execute(Function() _projectService.GetProjectDetail(projectId, CurrentUserId))
        End Function

        <HttpPut>
        <Route("{projectId:guid}")>
        <ProjectAuthorize("Owner")>
        Public Function UpdateProject(projectId As Guid, <FromBody> req As UpdateProjectRequest) As IHttpActionResult
            If req Is Nothing Then
                Return Content(HttpStatusCode.BadRequest, New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
            End If
            Return Execute(Function() _projectService.UpdateProject(projectId, req, CurrentUserId))
        End Function

        <HttpDelete>
        <Route("{projectId:guid}")>
        <ProjectAuthorize("Owner")>
        Public Function DeleteProject(projectId As Guid) As IHttpActionResult
            Return Execute(Sub() _projectService.DeleteProject(projectId, CurrentUserId))
        End Function

        ' ── MEMBERS MANAGEMENT ──────────────────────────

        <HttpGet>
        <Route("{projectId:guid}/members")>
        <ProjectAuthorize>
        Public Function GetMembers(projectId As Guid) As IHttpActionResult
            Return Execute(Function() _projectService.GetMembers(projectId))
        End Function

        <HttpPost>
        <Route("{projectId:guid}/members")>
        <ProjectAuthorize("Owner")>
        Public Function AddMember(projectId As Guid, <FromBody> req As AddMemberRequest) As IHttpActionResult
            If req Is Nothing Then
                Return Content(HttpStatusCode.BadRequest, New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
            End If
            Return Execute(Function() _projectService.AddMember(projectId, req, CurrentUserId))
        End Function

        <HttpPut>
        <Route("{projectId:guid}/members/{userId:guid}")>
        <ProjectAuthorize("Owner")>
        Public Function UpdateMemberRole(projectId As Guid, userId As Guid, <FromBody> req As UpdateMemberRequest) As IHttpActionResult
            If req Is Nothing Then
                Return Content(HttpStatusCode.BadRequest, New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
            End If
            Return Execute(Function() _projectService.UpdateMemberRole(projectId, userId, req, CurrentUserId))
        End Function

        <HttpDelete>
        <Route("{projectId:guid}/members/{userId:guid}")>
        <ProjectAuthorize("Owner")>
        Public Function RemoveMember(projectId As Guid, userId As Guid) As IHttpActionResult
            Return Execute(Sub() _projectService.RemoveMember(projectId, userId, CurrentUserId))
        End Function

        ' ── TASKS IN PROJECT ────────────────────────────

        <HttpGet>
        <Route("{projectId:guid}/tasks")>
        <ProjectAuthorize>
        Public Function GetProjectTasks(projectId As Guid) As IHttpActionResult
            Return Execute(Function() _taskService.GetProjectTasks(projectId, CurrentUserId))
        End Function

        <HttpPost>
        <Route("{projectId:guid}/tasks")>
        <ProjectAuthorize("Owner", "Editor")>
        Public Function CreateTask(projectId As Guid, <FromBody> req As CreateTaskRequest) As IHttpActionResult
            If req Is Nothing Then
                Return Content(HttpStatusCode.BadRequest, New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
            End If
            Return Execute(Function() _taskService.CreateTask(req, CurrentUserId, projectId))
        End Function

    End Class

End Namespace
