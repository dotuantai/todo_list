Public Interface ITaskAssignmentRepository
    Function Exists(taskId As Integer, userId As Guid) As Boolean

    Sub Add(assign As TaskAssignment)

    Function GetPermission(taskId As Integer, userId As Guid) As TaskAssignment
    Function GetAssignedTasks(userId As Guid) As List(Of TaskAssignment)

    Sub Save()
End Interface
