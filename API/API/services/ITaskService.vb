Public Interface ITaskService
    Function CreateTask(req As CreateTaskRequest, creatorId As Guid) As String

    Function UpdateTask(req As UpdateTaskRequest, currentUserId As Guid) As String

    Function DeleteTask(taskId As Integer, currentUserId As Guid) As String

    Function AssignTask(req As AssignTaskRequest, currentUserId As Guid) As String
    Function GetMyCreatedTasks(userId As Guid) As List(Of TaskDetailResponse)

    Function GetMyAssignedTasks(userId As Guid) As List(Of TaskResponse)

    Function UpdatePermission(req As AssignTaskRequest, currentUserId As Guid) As String

    Function RemoveAssignment(req As RemoveAssignmentRequest, currentUserId As Guid) As String
    Sub ChangeStatus(req As ChangeTaskStatusRequest, currentUserId As Guid)
End Interface
