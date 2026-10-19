using BCrypt.Net;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using WezProjectifier.Models;
using WezProjectifierBLL.Models;
using WezProjectifierBLL.DTOs;

namespace WezProjectifierBLL.Services
{
    public class clsUserService
    {
        
        private string HashPassword(string plaintextPassword)
        {
            // A workFactor of 12 is recommended by OWASP to balance speed and security
            return BCrypt.Net.BCrypt.HashPassword(plaintextPassword, workFactor: 12);
        }
        private readonly ILogger<clsUserService> _logger;
        private readonly WezDbContext _context;
        public clsUserService(WezDbContext context, ILogger<clsUserService> logger)
        {
            this._context = context;
            this._logger = logger;
        }
        public async Task<OperationMessage> CreateUserAsync(string firstName, string lastName, string email, string phoneNumber,
            string username, string password, string address, string? image = null)
        {
            if (String.IsNullOrEmpty(image))
                image = null;
            User newUser = new User
            {
                Firstname = firstName,
                Lastname = lastName,
                Email = email,
                Phonenumber = phoneNumber,
                Username = username,
                Password = HashPassword(password),
                Address = address,
                Image = image
            };
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if(user != null)
            {
                return new OperationMessage
                {
                    enResult = enOperationResult.AlreadyExist,
                    Message = "User with this email already exists !"
                
                };
            }
            user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if(user != null)
            {
                return new OperationMessage
                {
                    enResult = enOperationResult.AlreadyExist,
                    Message = "User with this username already exists !"
                };
            }
            user = await _context.Users.FirstOrDefaultAsync(u => u.Phonenumber == phoneNumber);
            if(user != null)
            {
                return new OperationMessage { enResult = enOperationResult.AlreadyExist,
                Message = "User with this Phonenumber already exists !"};
            }
            try
            {
                await _context.AddAsync(newUser);

                int affectedRows = await _context.SaveChangesAsync();
                _logger.LogInformation($"New user {newUser.Id} has been created at {DateTime.Now}");

            }
            catch (DbUpdateException ex) when (
            ex.InnerException is SqlException sqlEx &&
            (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {

                _logger.LogWarning(ex.InnerException.Message);
                return new OperationMessage { enResult = enOperationResult.AlreadyExist };
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return new OperationMessage { enResult = enOperationResult.Failed };
            }
            return new OperationMessage { enResult = enOperationResult.Success };
        } 
        public async Task<List<User>> GetAllUsersAsync()
        {
            var list = await _context.Users.AsNoTracking().ToListAsync();
            return list;
        }
        public async Task<Result<User>> FindUserAsync(int ID)
        {
            User? user;
            if (ID <= 0)
            {
                return Result<User>.Failure(enOperationResult.InvalidData);
            }
            try
            {
                user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == ID);
                if (user == null)
                    return Result<User>.Failure(enOperationResult.NotFound);
                return Result<User>.Success(user);

            }catch(Exception ex)
            {
                return Result<User>.Failure(enOperationResult.InvalidData);
            }
        }
        public async Task<enOperationResult> DeleteUserAsync(int ID)
        {
            if(ID <= 0)
            {
                return enOperationResult.InvalidData;
            }
            try
            {
                var user = await _context.Users.FindAsync(ID);

                if(user == null)
                {
                    return enOperationResult.NotFound;
                }

                _context.Users.Remove(user);

                await _context.SaveChangesAsync();

                
                return enOperationResult.Success;

            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return enOperationResult.Failed;
            }
        }
        public async Task<Result<User>> UpdateUserAsync(int ID, UpdateUserDTO dto)
        {
            if (ID <= 0)
            {
                return Result<User>.Failure(enOperationResult.InvalidData);
            }

            try
            {
                var user = await _context.Users.FindAsync(ID);
                if (user == null)
                {
                    return Result<User>.Failure(enOperationResult.NotFound);
                }

                var conflict = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.Id != ID);
                if (conflict != null)
                {
                    return Result<User>.Failure(enOperationResult.AlreadyExist);
                }

                
                if (!string.IsNullOrEmpty(dto.Firstname))
                    user.Firstname = dto.Firstname;
                if (!string.IsNullOrEmpty(dto.Lastname))
                    user.Lastname = dto.Lastname;
                if (!string.IsNullOrEmpty(dto.Email))
                    user.Email = dto.Email;
                if (!string.IsNullOrEmpty(dto.Phonenumber))
                    user.Phonenumber = dto.Phonenumber;
                if (!string.IsNullOrEmpty(dto.Address))
                    user.Address = dto.Address;
                if (!string.IsNullOrEmpty(dto.Image))
                    user.Image = dto.Image;

                _context.Users.Update(user);

                await _context.SaveChangesAsync();
                _logger.LogInformation($"User {user.Id} has been updated at {DateTime.Now}");

                return Result<User>.Success(user);
            }
            catch (Exception ex)
            {
                _logger?.LogCritical(ex.Message);
                return Result<User>.Failure(enOperationResult.Failed);
            }
        }
    }
}
