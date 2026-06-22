Imports Owin
Imports Microsoft.Owin
Imports Microsoft.Owin.Security.Jwt
Imports Microsoft.Owin.Security
Imports Microsoft.IdentityModel.Tokens
Imports System.Text
Imports System.Configuration
<Assembly: OwinStartup(GetType(API.Startup))>
Public Class Startup
    Public Sub Configuration(app As IAppBuilder)


        app.Use(GetType(LoggingMiddleware))


        Dim key = New SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                ConfigurationManager.AppSettings("Jwt:Key")))

        app.UseJwtBearerAuthentication(
            New JwtBearerAuthenticationOptions With {
                .AuthenticationMode = AuthenticationMode.Active,
                .TokenValidationParameters = New TokenValidationParameters With {
                    .ValidateIssuer = True,
                    .ValidIssuer = ConfigurationManager.AppSettings("Jwt:Issuer"),
                    .ValidateAudience = True,
                    .ValidAudience = ConfigurationManager.AppSettings("Jwt:Audience"),
                    .ValidateLifetime = True,
                    .ValidateIssuerSigningKey = True,
                    .IssuerSigningKey = key,
                    .ClockSkew = TimeSpan.Zero
                }
            })
    End Sub
End Class