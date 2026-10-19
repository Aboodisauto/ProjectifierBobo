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
    public class clsTaskService
    {
        private static string[] Priority = { "LOW", "MEDIUM", "HIGH" };
        private static string[] Status = { null, "TODO", "INPROGRESS", "DONE" };
        private readonly WezDbContext _context;
        private readonly ILogger<clsTaskService> _logger;
        public clsTaskService(WezDbContext context,ILogger<clsTaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<List<TaskDTO>>> GetAllTasksAsync(int projectId)
        {
            try
            {
                var project = await _context.Projects
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == projectId);

                if (project == null)
                    return Result<List<TaskDTO>>.Failure(enOperationResult.NotFound);
                
                var tasks = await _context.Tasks
                    .Where(t => t.ProjectId == projectId)
                    .AsNoTracking()
                    .Select(t => new TaskDTO
                    {
                        ID = t.Id,
                        TaskName = t.Name,
                        Description = t.Description,
                        Priority = t.Priority,
                        Status = t.Status
                    }).ToListAsync();

                return Result<List<TaskDTO>>.Success(tasks);

            }catch(Exception ex)
            {
                _logger.LogCritical( DateTime.Now + " - " + ex.Message);
                return Result<List<TaskDTO>>.Failure(enOperationResult.Failed);

            }
        }
        public async Task<Result<AddTaskResponseDTO>> CreateAddTaskToProjectAsync(AddTaskRequestDTO dto)
        {
            try
            {
                if (string.IsNullOrEmpty(dto.TaskName) || dto.ProjectID <= 0)
                    return Result<AddTaskResponseDTO>.Failure(enOperationResult.InvalidData);


                var project = await _context.Projects.FindAsync(dto.ProjectID);
                if(project == null)
                {
                    return Result<AddTaskResponseDTO>.Failure(enOperationResult.NotFound);
                }

                var task = new ProjectTask
                {
                    Name = dto.TaskName,
                    Description = dto.Description,
                    Priority = dto.Priority,
                    Status = dto.Status,
                    ProjectId = dto.ProjectID
                };

                await _context.AddAsync(task);

                await _context.SaveChangesAsync();

                var response = new AddTaskResponseDTO
                {
                    ID = task.Id,
                    TaskName = task.Name,
                    Description = task.Description,
                    Priority = task.Priority,
                    Status = task.Status,
                    ProjectID = task.ProjectId
                };

                return Result<AddTaskResponseDTO>.Success(response);

            }catch(Exception ex)
            {
                _logger.LogCritical(DateTime.Now + " - " + ex.Message);
                return Result<AddTaskResponseDTO>.Failure(enOperationResult.Failed);
            }
        }

        public async Task<Result<UpdateTaskResponseDTO>> UpdateTaskAsync(UpdateTaskRequestDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TaskName) || dto.ProjectID <= 0 ||
               !Priority.Contains(dto.Priority,StringComparer.OrdinalIgnoreCase) || !Status.Contains(dto.Status, StringComparer.OrdinalIgnoreCase) )
                return Result<UpdateTaskResponseDTO>.Failure(enOperationResult.InvalidData);

            try
            {
                var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == dto.ProjectID);
                if (project == null)
                    return Result<UpdateTaskResponseDTO>.Failure(enOperationResult.NotFound);
                var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == dto.ID);
                if (task == null)
                    return Result<UpdateTaskResponseDTO>.Failure(enOperationResult.NotFound);

                task.Name = dto.TaskName;
                task.Description = dto.Description;
                task.Priority = dto.Priority;
                task.Status = dto.Status;
                task.ProjectId = dto.ProjectID;

                await _context.SaveChangesAsync();

                UpdateTaskResponseDTO response = new UpdateTaskResponseDTO 
                {
                    ID = task.Id,
                    TaskName = task.Name,
                    Description = task.Description,
                    Priority = task.Priority,
                    Status = task.Status,
                    ProjectID = task.ProjectId
                };

                return Result<UpdateTaskResponseDTO>.Success(response);

            }catch(Exception ex)
            {
                _logger.LogCritical(DateTime.Now + " - " + ex.Message);
                return Result<UpdateTaskResponseDTO>.Failure(enOperationResult.Failed);
            }

        }

        public async Task<Result<bool>> DeleteTaskFromProjectAsync(int projectId, int taskId)
        {
            if (projectId <= 0 || taskId <= 0)
                return Result<bool>.Failure(enOperationResult.InvalidData);
            try
            {
                var project = await _context.Projects.FindAsync(projectId);
                var task = await _context.Tasks.FindAsync(taskId);
                if (task == null || project == null)
                    return Result<bool>.Failure(enOperationResult.NotFound);

                _context.Remove(task);
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
