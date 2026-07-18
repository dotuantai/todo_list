Imports System.Net
Imports System.Net.Http
Imports System.Security.Claims
Imports System.Web.Http.Controllers
Imports System.Web.Http.Filters
Imports System.Linq

Public Class ProjectAuthorizeAttribute
    Inherits ActionFilterAttribute

    Private ReadOnly _roles As String()

    Public Sub New(ParamArray roles As String())
        _roles = roles
    End Sub

    Public Overrides Sub OnActionExecuting(actionContext As HttpActionContext)
        ' 1. Get User Identity
        Dim principal = actionContext.RequestContext.Principal
        If principal Is Nothing OrElse Not principal.Identity.IsAuthenticated Then
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Unauthorized,
                New ApiResponse(Of Object)(False, "Authentication required.", Nothing))
            Return
        End If

        Dim identity = TryCast(principal.Identity, ClaimsIdentity)
        Dim claim = identity?.FindFirst(ClaimTypes.NameIdentifier)
        If claim Is Nothing Then
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Unauthorized,
                New ApiResponse(Of Object)(False, "Unable to identify user from token.", Nothing))
            Return
        End If

        Dim currentUserId = Guid.Parse(claim.Value)

        ' 2. Retrieve ProjectId from Route Data or Query Parameters
        Dim projectIdStr As String = Nothing
        Dim routeData = actionContext.RequestContext.RouteData
        
        ' First look in route values (e.g. api/projects/{projectId}/tasks)
        If routeData.Values.ContainsKey("projectId") Then
            projectIdStr = routeData.Values("projectId")?.ToString()
        End If

        ' If not found, look in query parameters
        If String.IsNullOrEmpty(projectIdStr) Then
            Dim queryParams = actionContext.Request.GetQueryNameValuePairs()
            For Each kv In queryParams
                If kv.Key.Equals("projectId", StringComparison.OrdinalIgnoreCase) Then
                    projectIdStr = kv.Value
                    Exit For
                End If
            Next
        End If

        ' If still not found, return Bad Request
        If String.IsNullOrEmpty(projectIdStr) Then
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.BadRequest,
                New ApiResponse(Of Object)(False, "Parameter projectId not found.", Nothing))
            Return
        End If

        Dim projectId As Guid
        If Not Guid.TryParse(projectIdStr, projectId) Then
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.BadRequest,
                New ApiResponse(Of Object)(False, "Invalid projectId parameter.", Nothing))
            Return
        End If

        ' 3. Query DB using Service Resolver to verify membership & roles
        Dim dbContext = CType(actionContext.Request.GetDependencyScope().GetService(GetType(AppDbContext)), AppDbContext)
        If dbContext Is Nothing Then
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.InternalServerError,
                New ApiResponse(Of Object)(False, "Database connection error.", Nothing))
            Return
        End If

        Dim member = dbContext.ProjectMembers.
            FirstOrDefault(Function(m) m.ProjectId = projectId AndAlso m.UserId = currentUserId)

        If member Is Nothing Then
            actionContext.Response = actionContext.Request.CreateResponse(
                HttpStatusCode.Forbidden,
                New ApiResponse(Of Object)(False, "You are not a member of this project.", Nothing))
            Return
        End If

        ' 4. Check if the user has one of the required roles (case-insensitive)
        If _roles IsNot Nothing AndAlso _roles.Length > 0 Then
            Dim hasRole = False
            For Each r In _roles
                If member.Role.Equals(r, StringComparison.OrdinalIgnoreCase) Then
                    hasRole = True
                    Exit For
                End If
            Next

            If Not hasRole Then
                actionContext.Response = actionContext.Request.CreateResponse(
                    HttpStatusCode.Forbidden,
                    New ApiResponse(Of Object)(False, "You do not have permission to perform this action.", Nothing))
                Return
            End If
        End If

        ' Proceed if all checks pass
        MyBase.OnActionExecuting(actionContext)
    End Sub
End Class
