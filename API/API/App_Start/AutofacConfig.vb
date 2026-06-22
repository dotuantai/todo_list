Imports Autofac
Imports Autofac.Integration.WebApi
Imports System.Reflection
Imports System.Web.Http

Public Class AutofacConfig
    Public Shared Sub Register()
        Dim builder As New ContainerBuilder()
        Dim config As HttpConfiguration = GlobalConfiguration.Configuration

        builder.RegisterApiControllers(Assembly.GetExecutingAssembly())
        builder.RegisterType(Of AppDbContext)().InstancePerRequest()

        builder.RegisterType(Of AuthService)().As(Of IAuthService)().InstancePerRequest()
        builder.RegisterType(Of UserRepository)().As(Of IUserRepository)().InstancePerRequest()

        builder.RegisterType(Of TaskService)().As(Of ITaskService)().InstancePerRequest()
        builder.RegisterType(Of TaskRepository)().As(Of ITaskRepository)().InstancePerRequest()


        builder.RegisterType(Of TaskAssignmentRepository)().As(Of ITaskAssignmentRepository)().InstancePerRequest()

        Dim container = builder.Build()
        config.DependencyResolver = New AutofacWebApiDependencyResolver(container)
    End Sub
End Class