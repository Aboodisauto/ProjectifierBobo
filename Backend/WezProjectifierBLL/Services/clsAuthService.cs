using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using WezProjectifier.Models;
using WezProjectifierBLL.Models;

namespace WezProjectifierBLL.Services
{
    public class clsAuthService
    {
        private readonly WezDbContext _context;
        private readonly ILogger<clsAuthService> logger;
        private readonly clsLoginService loginService;

        public clsAuthService(WezDbContext context
            , ILogger<clsAuthService> logger
            , clsLoginService loginService)
        {
            _context = context;
            this.logger = logger;
            this.loginService = loginService;
        }
    
        public async Task<clsLoginResult> LoginAsync(string username, string password, string IP )
        {
            clsLoginResult result = new clsLoginResult();
            if(string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                logger.LogWarning($"Invalid Login Attempt To Log at {DateTime.Now} By IP : {IP}");
                result.Result = enOperationResult.InvalidData;
                return result;
            }
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if(user == null)
                {
                    result.Result = enOperationResult.InvalidData;
                    logger.LogWarning($"Invalid Login Attempt To Log at {DateTime.Now} By IP : {IP}");
                    return result;
                }

                if(!BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    result.Result = enOperationResult.InvalidCredentials;
                    logger.LogWarning($"Invalid Login Attempt To Log at {DateTime.Now} By IP : {IP}");
                    return result;
                }

                string accessToken = await loginService.GetAccessToken(user);
                string refreshToken = await loginService.GenerateRefreshToken();

                var refreshTokenRow = await _context.RefreshTokens.FirstOrDefaultAsync(
                    rtr => rtr.UserId == user.Id
                    );

                if(refreshTokenRow != null)
                {
                    _context.Remove(refreshTokenRow);
                    await _context.SaveChangesAsync();
                }

                RefreshToken newRefreshToken = new RefreshToken
                {
                    RefreshToken1 = refreshToken,
                    UserId = user.Id,
                    CreatedAt = DateTime.Now,
                    ExpirationDate = DateTime.Now.AddDays(7)
                }
                ;
                await _context.RefreshTokens.AddAsync(newRefreshToken);
                await _context.SaveChangesAsync();

                result.AccessToken = accessToken;
                result.RefreshToken = refreshToken;
                result.Result = enOperationResult.Success;

                return result;

            }catch (Exception ex)
            {
                logger.LogCritical(DateTime.Now + " - " + ex.Message);
                result.Result = enOperationResult.Failed;
                return result;
            }
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {
                var refreshTokenRow = await _context.RefreshTokens.FirstOrDefaultAsync(rf => rf.UserId == userId);
                if(refreshTokenRow == null)
                {
                    return false;
                }

                _context.Remove(refreshTokenRow);

                await _context.SaveChangesAsync();

                return true;
            }catch(Exception ex)
            {
                logger.LogCritical($"{DateTime.Now} - {ex.Message}");
                return false;
            }
        }

        public async Task<clsLoginResult> RefreshTokenAsync(string RefreshToken, string ip)
        {
            clsLoginResult result = new clsLoginResult();

            try
            {
                var oldRefreshTokenRow = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.RefreshToken1 == RefreshToken);
                if (oldRefreshTokenRow == null)
                {
                    result.Result = enOperationResult.NotFound;
                    logger.LogWarning($"A failed refresh attempt by ip : {ip}");
                    return result;
                }

                //Check for expired refresh tokens

                if(oldRefreshTokenRow.ExpirationDate <= DateTime.Now)
                {
                    result.Result = enOperationResult.Expired;
                    logger.LogWarning($"Refresh Token Expired Attempt By Ip : {ip}");
                    return result;
                }

                _context.Remove(oldRefreshTokenRow);
                await _context.SaveChangesAsync();

                User user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == oldRefreshTokenRow.UserId);

                

                string accessToken = await loginService.GetAccessToken(user);
                string refreshToken = await loginService.GenerateRefreshToken();

                RefreshToken newRefreshTokenRow = new RefreshToken
                {
                    RefreshToken1 = refreshToken,
                    UserId = user.Id,
                    ExpirationDate = DateTime.Now.AddDays(7),
                    CreatedAt = DateTime.Now
                };

                await _context.RefreshTokens.AddAsync(newRefreshTokenRow);
                await _context.SaveChangesAsync();

                result.AccessToken = accessToken;
                result.RefreshToken = refreshToken;
                result.Result = enOperationResult.Success;

                return result;

            }catch(Exception ex)
            {
                result.Result = enOperationResult.Failed;
                logger.LogCritical($"{DateTime.Now} - {ex.Message}");
                return result;
            }
        }
    }
}
