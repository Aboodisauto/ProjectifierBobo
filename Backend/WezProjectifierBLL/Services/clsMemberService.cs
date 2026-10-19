using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using WezProjectifier.Models;
using WezProjectifierBLL.DTOs.Members;
using WezProjectifierBLL.DTOs.Projects;
using WezProjectifierBLL.DTOs.User;
using WezProjectifierBLL.Models;

namespace WezProjectifierBLL.Services
{
    public class clsMemberService
    {
        private static string[] _roles = { "Owner", "User" };

        private readonly WezDbContext _context;
        private readonly ILogger<clsMemberService> _logger;
        public clsMemberService(WezDbContext context, ILogger<clsMemberService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<Result<AddMemberResponseDTO>> AddUserToProject(int projectId,int userId,string Role)
        {
            if (projectId <= 0 || userId <= 0 || !_roles.Contains(Role,StringComparer.OrdinalIgnoreCase))
                return Result<AddMemberResponseDTO>.Failure(enOperationResult.InvalidData);
            try
            {
                var project = await _context.Projects.FindAsync(projectId);
                var user = await _context.Users.FindAsync(userId);
                if (project == null || user == null)
                    return Result<AddMemberResponseDTO>.Failure(enOperationResult.NotFound);

                var member = new Member
                {
                    UserId = userId,
                    ProjectId = projectId,
                    Role = Role
                };

                await _context.AddAsync(member);

                await _context.SaveChangesAsync();

                AddMemberResponseDTO response = new AddMemberResponseDTO { 
                    MemberId = member.Id,
                    Project = new ProjectSummaryDTO 
                    {
                        ProjectID = project.Id,
                        ProjectName = project.Name,
                        Description = project.Description,
                        Image = project.Image
                    },
                    User = new DTOs.User.UserSummaryDTO 
                    {
                        Id = user.Id,
                        UserName = user.Username,
                        Email = user.Email
                    }
                };

                return Result<AddMemberResponseDTO>.Success(response);


            }
            catch (DbUpdateException ex)
                when (ex.InnerException is SqlException sqlEx
              && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
            {
                _logger.LogCritical(
                    ex,
                    "A unique constraint violation occurred while saving a Member."
                );
                return Result<AddMemberResponseDTO>.Failure(enOperationResult.AlreadyExist);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(DateTime.Now + " - " + ex.Message);
                return Result<AddMemberResponseDTO>.Failure(enOperationResult.Failed);
            }
        }
        public async Task<Result<List<UserSummaryDTO>>> GetAllMembers(int projectId)
        {
            if (projectId <= 0)
                return Result<List<UserSummaryDTO>>.Failure(enOperationResult.InvalidData);
            try
            {
                var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
                if (project == null)
                    return Result<List<UserSummaryDTO>>.Failure(enOperationResult.NotFound);

                var members = await _context.Members.AsNoTracking().Where(m => m.ProjectId == projectId)
                    .Join(_context.Users,
                    m => m.UserId,
                    u => u.Id,
                    (m, u) => new UserSummaryDTO { 
                        Id = u.Id,
                        UserName = u.Username,
                        Email = u.Email })
                    .ToListAsync();

                return Result<List<UserSummaryDTO>>.Success(members);


            }
            
            catch (Exception ex)
            {
                _logger.LogCritical(DateTime.Now + " - " + ex.Message);
                return Result<List<UserSummaryDTO>>.Failure(enOperationResult.Failed);
            }
        }

        public async Task<Result<bool>> RemoveUserFromProjectAsync(int projectId, int userId)
        {
            if (projectId <= 0 || userId <= 0)
                return Result<bool>.Failure(enOperationResult.InvalidData);
            try
            {
                var project = await _context.Projects.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == projectId);
                var user = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null || project == null)
                    return Result<bool>.Failure(enOperationResult.NotFound);

                var member = await _context.Members.FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
                if (member == null)
                    return Result<bool>.Failure(enOperationResult.NotFound);

                _context.Remove(member);

                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);

            }catch(Exception ex)
            {
                _logger.LogCritical(DateTime.Now + " - " + ex.Message);
                return Result<bool>.Failure(enOperationResult.Failed);
            }
        }
    }
}
