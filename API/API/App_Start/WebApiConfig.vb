Imports System.Web.Http
Imports System.Web.Http.Cors

Public Module WebApiConfig

    Public Sub Register(config As HttpConfiguration)

        Dim cors = New EnableCorsAttribute(
            "http://localhost:5173",
            "*",
            "*"
        )
        cors.SupportsCredentials = True
        config.EnableCors(cors)

        config.MapHttpAttributeRoutes()

        config.Routes.MapHttpRoute(
            name:="DefaultApi",
            routeTemplate:="api/{controller}/{id}",
            defaults:=New With {.id = RouteParameter.Optional}
        )

    End Sub
End Module