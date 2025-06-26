using Castle.DynamicProxy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Reflection;

namespace Avenged.Extensions.DependencyInjection.ServiceAuthorization;

internal class AuthorizationInterceptor : IInterceptor
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationInterceptor(
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public void Intercept(IInvocation invocation)
    {
        MethodInfo? method = invocation.MethodInvocationTarget;
        Type? declaringType = invocation.TargetType;

        // Verificar autorización a nivel de clase
        var classAuthAttributes = declaringType.GetCustomAttributes<AuthorizeAttribute>(true);
        foreach (var attr in classAuthAttributes)
        {
            if (!IsAuthorized(attr))
            {
                throw new UnauthorizedAccessException($"Access denied to class {declaringType.Name}");
            }
        }

        // Verificar autorización a nivel de método
        var methodAuthAttributes = method.GetCustomAttributes<AuthorizeAttribute>(true);
        foreach (var attr in methodAuthAttributes)
        {
            if (!IsAuthorized(attr))
            {
                throw new UnauthorizedAccessException($"Access denied to method {method.Name}");
            }
        }

        // Si pasa todas las verificaciones, ejecutar el método
        invocation.Proceed();
    }

    private bool IsAuthorized(AuthorizeAttribute authorizeAttribute)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null)
            return false;

        var user = httpContext.User;

        // Si no está autenticado
        if (!user.Identity?.IsAuthenticated ?? true)
            return false;

        // Verificar roles si están especificados
        if (!string.IsNullOrEmpty(authorizeAttribute.Roles))
        {
            var requiredRoles = authorizeAttribute.Roles.Split(',')
                .Select(r => r.Trim())
                .ToArray();

            if (!requiredRoles.Any(role => user.IsInRole(role)))
                return false;
        }

        // Verificar policy si está especificada
        if (!string.IsNullOrEmpty(authorizeAttribute.Policy))
        {
            var authResult = _authorizationService
                .AuthorizeAsync(user, authorizeAttribute.Policy)
                .GetAwaiter()
                .GetResult();

            if (!authResult.Succeeded)
                return false;
        }

        return true;
    }
}