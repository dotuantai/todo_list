Public Interface ITaskRepository
    Function GetById(id As Integer) As TodoTask
    Function GetCreatedTasks(userId As Guid) As List(Of TodoTask)

    Sub Add(task As TodoTask)

    Sub Update(task As TodoTask)

    Sub Delete(task As TodoTask)

    Sub Save()
End Interface
