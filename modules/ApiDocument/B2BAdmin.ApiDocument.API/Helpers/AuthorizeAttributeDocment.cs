using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using Microsoft.AspNetCore.Mvc;
using B2BAdmin.ApiDocument.Domains.Models;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DocumentAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = (UserAdminTourchain) context.HttpContext.Items["UserAdminTourchain"];
        
        if (user == null)
        {
            // not logged in
            context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
        }
    }
}