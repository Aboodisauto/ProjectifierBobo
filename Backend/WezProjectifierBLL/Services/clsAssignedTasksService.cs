using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using WezProjectifier.Models;
using WezProjectifierBLL.DTOs.Tasks;
using WezProjectifierBLL.Models;

namespace WezProjectifierBLL.Services
{
    public class clsAssignedTasksService
    {
        private readonly WezDbContext _context;
        private readonly ILogger<clsAssignedTasksService> _logger;

        public clsAssignedTasksService(WezDbContext context, ILogger<clsAssignedTasksService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<bool>> AssignTaskToUserAsync(int taskId, int projectId, int userId)
        {
            if (taskId <= 0 || projectId <= 0 || userId <= 0)
            {
                return Result<bool>.Failure(enOperationResult.InvalidData);
            }
            try
            {
                var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);
                if (user == null || project == null || task == null)
                    return Result<bool>.Failure(enOperationResult.NotFound);

                var member = await _context.Members.AsNoTracking().FirstOrDefaultAsync(m => m.UserId == userId && m.ProjectId == projectId);
                if (member == null)
                    return Result<bool>.Failure(enOperationResult.NotFound2);

                AssignedTask newAssignedTask = new AssignedTask 
                {
                    MemberId = member.Id,
                    TaskId = task.Id
                };

                await _context.AssignedTasks.AddAsync(newAssignedTask);

                await _context.SaveChangesAsync();

                return Result<bool>.Success(true);
            }catch(Exception ex)
            {
                _logger.LogCritical(DateTime.Now + " - " + ex.Message);
                return Result<bool>.Failure(enOperationResult.Failed);
            }
        }

        public async Task<Result<List<TaskDTO>>> GetAllAssignedTasksAsync(int projectId, int userId)
        {
            if (projectId <= 0 || userId <= 0)
                return Result<List<TaskDTO>>.Failure(enOperationResult.InvalidData);
            try 
            {
                var project = await _context.Projects.AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == projectId);
                var user = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null || project == null)
                    return Result<List<TaskDTO>>.Failure(enOperationResult.NotFound);

                var member = await _context.Members.FirstOrDefaultAsync(m => m.ProjectId == projectId && m.UserId == userId);
                if (member == null)
                    return Result<List<TaskDTO>>.Failure(enOperationResult.NotFound2);

                List<TaskDTO> response = await _context.AssignedTasks.AsNoTracking().Join(_context.Members,
                    at => at.MemberId,
                    m => m.Id,
                    (at, m) => new
                    {
                        userid = m.UserId,
                        projectid = m.ProjectId,
                        taskid = at.TaskId
                    })
                    .Where(t => t.userid == userId && t.projectid == projectId)
                    .Join(_context.Tasks,
                    at => at.taskid,
                    t => t.Id,
                    (at, t) => new TaskDTO
                    {
                        ID = t.Id,
                        TaskName = t.Name,
                        Description = t.Description,
                        Priority = t.Priority,
                        Status = t.Status
                    }).ToListAsync();

                return Result<List<TaskDTO>>.Success(response);

            } catch (Exception ex) 
            {
                _logger.LogCritical(DateTime.Now + " - " + ex.Message);
                return Result<List<TaskDTO>>.Failure(enOperationResult.Failed);
            }
        }

        public async Task<Result<bool>> UnassignTaskFromUserAsync(int taskId, int projectId, int userId)
        {
            if (taskId <= 0 || projectId <= 0 || userId <= 0)
            {
                return Result<bool>.Failure(enOperationResult.InvalidData);
            }
            try
            {
                var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId);
                if (user == null || project == null || task == null)
                    return Result<bool>.Failure(enOperationResult.NotFound);

                var member = await _context.Members.AsNoTracking().FirstOrDefaultAsync(m => m.UserId == userId && m.ProjectId == projectId);
                if (member == null)
                    return Result<bool>.Failure(enOperationResult.NotFound2);

                var assignedTask = await _context.AssignedTasks
                    .FirstOrDefaultAsync(at => at.MemberId == member.Id && at.TaskId == taskId);
                if (assignedTask == null)
                    return Result<bool>.Failure(enOperationResult.NotFound2);

                _context.AssignedTasks.Remove(assignedTask);
                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(DateTime.Now + " - " + ex.Message);
                return Result<bool>.Failure(enOperationResult.Failed);
            }
        }
    }
}
