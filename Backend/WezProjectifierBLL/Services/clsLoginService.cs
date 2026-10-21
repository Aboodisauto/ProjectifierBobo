using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WezProjectifier.Models;

namespace WezProjectifierBLL.Services
{
    public class clsLoginService
    {
        private readonly IConfiguration configuration;
        public clsLoginService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<string> GetAccessToken(User? user)
        {
            if (user == null) throw new ArgumentNullException("user is null");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            string secretKey = configuration["Jwt:SecretKey"] ?? throw new ArgumentNullException("Secret key is null or not found");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCreds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(Convert.ToDouble(15)),
            signingCredentials: signingCreds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        public async Task<string> GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
