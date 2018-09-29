﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HomeManagement.API.Extensions;
using System.IdentityModel.Tokens.Jwt;

namespace HomeManagement.API.Filters
{
    public class AuthorizationAttribute : Attribute, IAsyncActionFilter
    {
        private JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var header = context.HttpContext.GetAuthorizationHeader();

            if (string.IsNullOrEmpty(header))
            {
                context.Result = new ContentResult { StatusCode = (int)HttpStatusCode.Forbidden, Content = "Header not present" };

                return;
            }

            if (!jwtSecurityTokenHandler.IsValid(header))
            {
                context.Result = new ContentResult { StatusCode = (int)HttpStatusCode.Unauthorized, Content = "Token has expired" };

                return;
            }

            await next();
        }
    }
}
