using System;
using FakeSurveyGenerator.API.Identity;
using FakeSurveyGenerator.Application.Common.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace FakeSurveyGenerator.API.Builders
{
    internal static class UserBuilder
    {
        public static IServiceCollection AddUserConfiguration(this IServiceCollection services)
        {
            static IUser GetUser(IServiceProvider sp)
            {
                var httpContext = sp.GetRequiredService<IHttpContextAccessor>().HttpContext;

                if (!httpContext.User.Identity.IsAuthenticated)
                    return new UnauthenticatedUser();

                var userService = sp.GetRequiredService<IUserService>();

                var accessToken = httpContext.Request.Headers["Authorization"].ToString().Substring(7);

                var userInfo = userService.GetUserInfo(accessToken).GetAwaiter().GetResult();

                return userInfo;
            }

            services.AddScoped(GetUser);

            return services;
        }
    }
}