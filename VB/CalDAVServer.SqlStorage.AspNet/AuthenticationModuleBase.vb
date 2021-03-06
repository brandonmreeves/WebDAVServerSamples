Imports System
Imports System.Security
Imports System.Security.Principal
Imports System.Text
Imports System.Web
Imports System.Web.Security

''' <summary>
''' Base class for challenge/response authentication ASP.NET modules, like Digest, Basic.
''' </summary>
Public MustInherit Class AuthenticationModuleBase
    Implements IHttpModule

    Public Sub Init(application As HttpApplication) Implements IHttpModule.Init
        AddHandler application.AuthenticateRequest, AddressOf App_OnAuthenticateRequest
        AddHandler application.EndRequest, AddressOf App_OnEndRequest
    End Sub

    Public Sub Dispose() Implements IHttpModule.Dispose
    End Sub

    Protected MustOverride Function AuthenticateRequest(request As HttpRequest) As IPrincipal

    Protected MustOverride Function GetChallenge() As String

    Protected MustOverride Function IsAuthorizationPresent(request As HttpRequest) As Boolean

    Private Sub App_OnAuthenticateRequest(source As Object, eventArgs As EventArgs)
        If IsAuthorizationPresent(HttpContext.Current.Request) Then
            Dim principal As IPrincipal = AuthenticateRequest(HttpContext.Current.Request)
            If principal IsNot Nothing Then
                ' authenticated succesfully
                HttpContext.Current.User = principal
            Else
                ' invalid credentials
                unauthorized()
            End If
        Else
            ' To support Miniredirector/Web Folders on XP and Server 2003 as well as 
            ' Firefox CORS requests, OPTIONS must be processed without authorization.
            If HttpContext.Current.Request.HttpMethod = "OPTIONS" AndAlso Not(HttpContext.Current.Request.UserAgent IsNot Nothing AndAlso HttpContext.Current.Request.UserAgent.StartsWith("Microsoft Office")) Then Return
            unauthorized()
        End If
    End Sub

    Private Sub App_OnEndRequest(source As Object, eventArgs As EventArgs)
        Dim app As HttpApplication = CType(source, HttpApplication)
        If app.Response.StatusCode = 401 Then
            ' show login dialog
            app.Response.AppendHeader("WWW-Authenticate", GetChallenge())
        End If
    End Sub

    Private Shared Sub unauthorized()
        Dim response As HttpResponse = HttpContext.Current.Response
        response.StatusCode = 401
        response.StatusDescription = "Unauthorized"
        response.Write("401 Unauthorized")
    End Sub
End Class
