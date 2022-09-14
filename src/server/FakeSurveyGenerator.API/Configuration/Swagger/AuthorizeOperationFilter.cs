﻿using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FakeSurveyGenerator.API.Configuration.Swagger;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>().ToList();

        if ((authAttributes ?? Enumerable.Empty<AuthorizeAttribute>()).Any())
        {
            operation.Responses.Add(StatusCodes.Status401Unauthorized.ToString(), new OpenApiResponse { Description = nameof(HttpStatusCode.Unauthorized) });
            operation.Responses.Add(StatusCodes.Status403Forbidden.ToString(), new OpenApiResponse { Description = nameof(HttpStatusCode.Forbidden) });
        }

        if (!(authAttributes ?? Enumerable.Empty<AuthorizeAttribute>()).Any()) return;

        var oauth2SecurityScheme = new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "OAuth2" }
        };


        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [oauth2SecurityScheme] = new[] { "OAuth2" }
        });
    }
}