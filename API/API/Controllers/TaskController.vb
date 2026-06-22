Imports System.Net
Imports System.Security.Claims
Imports System.Web.Http

Namespace Controllers
    <RoutePrefix("api/tasks")>
    <Authorize>
    Public Class TaskController
        Inherits ApiController
        Private ReadOnly _taskService As ITaskService

        Public Sub New(taskService As ITaskService)
            _taskService = taskService
        End Sub

        <HttpPost>
        <Route("")>
        Public Function Create(req As CreateTaskRequest) As IHttpActionResult

            Dim userId = Guid.Parse(CType(User.Identity, ClaimsIdentity).FindFirst(ClaimTypes.NameIdentifier).Value)

            Return Ok(_taskService.CreateTask(req, userId))

        End Function
        <HttpPut>
        <Route("")>
        Public Function Update(req As UpdateTaskRequest) As IHttpActionResult

            Dim userId = Guid.Parse(CType(User.Identity, ClaimsIdentity).FindFirst(ClaimTypes.NameIdentifier).Value)

            Return Ok(_taskService.UpdateTask(req, userId))

        End Function
        <HttpDelete>
        <Route("{id:int}")>
        Public Function Delete(id As Integer) As IHttpActionResult

            Dim userId = Guid.Parse(CType(User.Identity, ClaimsIdentity).FindFirst(ClaimTypes.NameIdentifier).Value)

            Return Ok(_taskService.DeleteTask(id, userId))

        End Function
        <HttpPost>
        <Route("assign")>
        Public Function Assign(req As AssignTaskRequest) As IHttpActionResult

            Dim userId = Guid.Parse(CType(User.Identity, ClaimsIdentity).FindFirst(ClaimTypes.NameIdentifier).Value)

            Return Ok(_taskService.AssignTask(req, userId))

        End Function
        <HttpGet>
        <Route("my-created")>
        <Authorize>
        Public Function GetMyCreatedTasks() As IHttpActionResult

            Dim userId = Guid.Parse(CType(User.Identity, ClaimsIdentity).FindFirst(ClaimTypes.NameIdentifier).Value)

            Return Ok(_taskService.GetMyCreatedTasks(userId))

        End Function
        <HttpGet>
        <Route("my-assigned")>
        <Authorize>
        Public Function GetMyAssignedTasks() As IHttpActionResult

            Dim userId = Guid.Parse(CType(User.Identity, ClaimsIdentity).FindFirst(ClaimTypes.NameIdentifier).Value)

            Return Ok(_taskService.GetMyAssignedTasks(userId))

        End Function
    End Class
End Namespace