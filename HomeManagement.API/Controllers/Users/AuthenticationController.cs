﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using HomeManagement.API.Data;
using HomeManagement.API.Data.Entities;
using HomeManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HomeManagement.API.Controllers.Users
{
    [Produces("application/json")]
    [Route("api/Authentication")]
    public class AuthenticationController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IConfiguration configuration;
        private readonly JwtSecurityTokenHandler jwtSecurityToken = new JwtSecurityTokenHandler();

        public AuthenticationController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] UserModel user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var appUser = await userManager.FindByEmailAsync(user.Email);

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                var header = HttpContext.Request.Headers["Authorization"].FirstOrDefault();

                return Ok(header);
            }

            var result = await signInManager.PasswordSignInAsync(user.Email, user.Password, true, false);
            
            if (!result.Succeeded) return Forbid();

            var cl = await signInManager.CreateUserPrincipalAsync(appUser);
                      
            
            var claims = new[]
            {
                 new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken
            (
                issuer: configuration["Issuer"],
                   audience: configuration["Audience"],
                   claims: claims,
                   expires: DateTime.UtcNow.AddDays(1),
                   notBefore: DateTime.UtcNow,
                   signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["SigningKey"])),
                        SecurityAlgorithms.HmacSha256)
            );

            var tokenValue = jwtSecurityToken.WriteToken(token);

            if (result.Succeeded) return Ok(tokenValue);
            else return BadRequest();
        }

        [HttpPost("signout")]
        public async Task<IActionResult> SignOut([FromBody] UserModel user)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                await signInManager.SignOutAsync();
            }

            return Ok();
        }
    }
}