Imports System.Web.Http

Namespace Controllers

    <RoutePrefix("api/tasks")>
    <Authorize>
    Public Class TaskController
        Inherits BaseApiController

        Private ReadOnly _taskService As ITaskService

        Public Sub New(taskService As ITaskService)
            _taskService = taskService
        End Sub

        <HttpPost>
        <Route("")>
        Public Function Create(<FromBody> req As CreateTaskRequest) As IHttpActionResult
            If req Is Nothing Then
                Return Content(Net.HttpStatusCode.BadRequest,
                    New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
            End If
            Return Execute(Function() _taskService.CreateTask(req, CurrentUserId))
        End Function

        <HttpPut>
        <Route("")>
        Public Function Update(<FromBody> req As UpdateTaskRequest) As IHttpActionResult
            If req Is Nothing Then
                Return Content(Net.HttpStatusCode.BadRequest,
                    New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
            End If
            Return Execute(Function() _taskService.UpdateTask(req, CurrentUserId))
        End Function

        <HttpDelete>
        <Route("{id:int}")>
        Public Function Delete(id As Integer) As IHttpActionResult
            Return Execute(Function() _taskService.DeleteTask(id, CurrentUserId))
        End Function

        <HttpGet>
        <Route("my-created")>
        Public Function GetMyCreatedTasks() As IHttpActionResult
            Return Execute(Function() _taskService.GetMyCreatedTasks(CurrentUserId))
        End Function

        <HttpGet>
        <Route("my-assigned")>
        Public Function GetMyAssignedTasks() As IHttpActionResult
            Return Execute(Function() _taskService.GetMyAssignedTasks(CurrentUserId))
        End Function

        <HttpPost>
        <Route("assign")>
        Public Function Assign(<FromBody> req As AssignTaskRequest) As IHttpActionResult
            If req Is Nothing Then
                Return Content(Net.HttpStatusCode.BadRequest,
                    New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
            End If
            Return Execute(Function() _taskService.AssignTask(req, CurrentUserId))
        End Function

        <HttpPut>
        <Route("assign")>
        Public Function UpdatePermission(<FromBody> req As AssignTaskRequest) As IHttpActionResult
            If req Is Nothing Then
                Return Content(Net.HttpStatusCode.BadRequest,
                    New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
            End If
            Return Execute(Function() _taskService.UpdatePermission(req, CurrentUserId))
        End Function

        <HttpDelete>
        <Route("assign")>
        Public Function RemoveAssignment(<FromBody> req As RemoveAssignmentRequest) As IHttpActionResult
            If req Is Nothing Then
                Return Content(Net.HttpStatusCode.BadRequest,
                    New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
            End If
            Return Execute(Function() _taskService.RemoveAssignment(req, CurrentUserId))
        End Function

        <HttpPut>
        <Route("status")>
        Public Function ChangeStatus(<FromBody> req As ChangeTaskStatusRequest) As IHttpActionResult
            If req Is Nothing Then
                Return Content(Net.HttpStatusCode.BadRequest,
                    New ApiResponse(Of Object)(False, "Dữ liệu không hợp lệ.", Nothing))
            End If
            Return Execute(Sub() _taskService.ChangeStatus(req, CurrentUserId))
        End Function

    End Class

End Namespace
