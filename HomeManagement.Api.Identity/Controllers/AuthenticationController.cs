﻿using HomeManagement.Api.Core;
using HomeManagement.Contracts;
using HomeManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace HomeManagement.Api.Identity.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ICryptography cryptography;
        private readonly IConfiguration configuration;
        private readonly ILogger<AuthenticationController> logger;

        public AuthenticationController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ICryptography cryptography,
            IConfiguration configuration,
            ILogger<AuthenticationController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.cryptography = cryptography;
            this.configuration = configuration;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Test()
        {
            System.AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            var grpcAddress = configuration.GetValue<string>("GprcEndpoint");

            var channel = Grpc.Net.Client.GrpcChannel.ForAddress(grpcAddress);

            var client = new Protos.Register.RegisterClient(channel);
            var result = await client.EchoAsync(new Protos.EchoRequest());

            return Ok();
        }


        [HttpPost("SignIn")]
        public async Task<IActionResult> Post([FromBody] UserModel userModel)
        {
            if (!ModelState.IsValid)
            {
                logger.LogInformation($"Invalid model state: {string.Concat(ModelState.Values.Select(x => x.Errors.Select(r => r.ErrorMessage)))}");
                return BadRequest(ModelState);
            }

            var password = cryptography.Decrypt(userModel.Password);

            var result = await signInManager.PasswordSignInAsync(userModel.Email, password, true, false);

            if (!result.Succeeded)
            {
                logger.LogInformation("Invalid email or password.");
                return BadRequest();
            }

            var user = await userManager.FindByEmailAsync(userModel.Email);
            var roles = await userManager.GetRolesAsync(user);
            
            var token = roles.Any() ?
                TokenFactory.CreateToken(user.Email, roles, configuration["Issuer"], configuration["Audience"], configuration["SigningKey"], DateTime.UtcNow.AddDays(1)) :
                TokenFactory.CreateToken(user.Email, configuration["Issuer"], configuration["Audience"], configuration["SigningKey"]);

            var tokenResult = await userManager.SetAuthenticationTokenAsync(user, nameof(JwtSecurityToken), nameof(JwtSecurityToken), token);

            return Ok(new UserModel
            {
                Email = userModel.Email,
                Token = token
            });
        }

        [HttpPost("SignOut")]
        public async Task<IActionResult> SignOut([FromBody] UserModel userModel)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await userManager.FindByEmailAsync(userModel.Email);

            var result = await userManager.RemoveAuthenticationTokenAsync(user, nameof(JwtSecurityToken), nameof(JwtSecurityToken));

            return Ok();
        }
    }
}