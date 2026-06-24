Public Interface ITaskAssignmentRepository
    Function Exists(taskId As Integer, userId As Guid) As Boolean

    Sub Add(assign As TaskAssignment)

    Function GetAssignment(taskId As Integer, userId As Guid) As TaskAssignment
    Function GetAssignedTasks(userId As Guid) As List(Of TaskAssignment)

    Sub Save()
    Sub Remove(assign As TaskAssignment)
End Interface
