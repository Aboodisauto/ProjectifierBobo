using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using WezProjectifier.Models;
using WezProjectifierBLL.DTOs.Projects;
using WezProjectifierBLL.Models;

namespace WezProjectifierBLL.Services
{
    public class clsProjectService
    {
        private readonly ILogger<clsProjectService> _logger;
        private readonly WezDbContext _context;
        public clsProjectService(WezDbContext context,ILogger<clsProjectService> logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<Result<AddNewProjectResponseDTO>> AddNewProjectAsync(AddNewProjectRequestDTO dto)
        {
            if (dto.Image == "string" || string.IsNullOrEmpty(dto.Image))
                dto.Image = null;
            try
            {
                if (string.IsNullOrEmpty(dto.ProjectName))
                    return Result<AddNewProjectResponseDTO>.Failure(enOperationResult.InvalidData);
                var owner = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == dto.CreatedBy);
                if(owner == null)
                {
                    return Result<AddNewProjectResponseDTO>.Failure(enOperationResult.NotFound);
                }
                
                Project pr = new Project
                {
                    Name = dto.ProjectName,
                    Description = dto.Description,
                    CreatedBy = dto.CreatedBy,
                    Image = dto.Image
                };

                

                await _context.Projects.AddAsync(pr);

                await _context.SaveChangesAsync();

                var newAddedProject = await _context.Projects
    .AsNoTracking()
    .Where(p => p.Id == pr.Id)
    .Select(p => new AddNewProjectResponseDTO
    {
        ProjectID = p.Id,
        ProjectName = p.Name,
        Description = p.Description,
        Image = p.Image,
        CreatedBy = p.CreatedBy,
        User = new DTOs.User.UserSummaryDTO
        {
            Id = p.CreatedByNavigation.Id,
            UserName = p.CreatedByNavigation.Username,
            Email = p.CreatedByNavigation.Email
        }
    })
    .FirstOrDefaultAsync();

                if (newAddedProject == null)
                    throw new Exception("Project was added, but there was a problem in fetching it from the database");

               return Result<AddNewProjectResponseDTO>.Success(newAddedProject);
                    

            }catch(Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return Result<AddNewProjectResponseDTO>.Failure(enOperationResult.Failed);
            }
        }

        public async Task<Result<bool>> DeleteProjecyAsync(int id)
        {
            try
            {
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
                if (project == null)
                    return Result<bool>.Failure(enOperationResult.Failed);
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
                return Result<bool>.Success(true);

            }catch(Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return Result<bool>.Failure(enOperationResult.Failed);
            }
        }

        public async Task<List<ProjectSummaryDTO>> GetAllProjects()
        {
            var projects = await _context.Projects
                .Select(p => new ProjectSummaryDTO
                {
                    ProjectID = p.Id,
                    ProjectName = p.Name,
                    Description = p.Description,
                    Image = p.Image
                }).AsNoTracking().ToListAsync();
            return projects;
        }

        public async Task<Result<GetProjectInfoDTO>> GetProjectInfoAsync(int ProjectID)
        {
            try{
                var project = await _context.Projects
                .AsNoTracking()
                .AsSplitQuery()
                .Select(p => new GetProjectInfoDTO
                {
                    ProjectName = p.Name,
                    Description = p.Description,
                    ID = p.Id,
                    Image = p.Image,
                    CreatedBy = p.CreatedBy,
                    Member = p.Members.Select(m => new MemberDTO
                    {
                        Id = m.Id,
                        ProjectId = m.ProjectId,
                        UserId = m.UserId,
                        Role = m.Role,
                        User = new DTOs.User.UserSummaryDTO
                        {
                            Id = m.User.Id,
                            UserName = m.User.Username,
                            Email = m.User.Email
                        }
                    }).ToList(),
                    Tasks = p.Tasks.Select(t => new ProjectTaskDTO
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Description = t.Description,
                        Priority = t.Priority,
                        Status = t.Status,
                        ProjectId = t.ProjectId,
                        AssignedMemberIds = t.AssignedTasks.Select(at => at.MemberId).ToList()
                    }).ToList(),
                    OwnerUser = new DTOs.User.UserSummaryDTO
                    {
                        Email = p.CreatedByNavigation.Email,
                        UserName = p.CreatedByNavigation.Username,
                        Id = p.CreatedByNavigation.Id
                    }
                }).FirstOrDefaultAsync(p => p.ID == ProjectID);

                if (project == null)
                {
                    return Result<GetProjectInfoDTO>.Failure(enOperationResult.InvalidData);
                }
                return Result<GetProjectInfoDTO>.Success(project);
            }catch(Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return Result<GetProjectInfoDTO>.Failure(enOperationResult.Failed);
            }
        }

        public async Task<Result<AddNewProjectResponseDTO>> UpdateProjectAsync(UpdateProjectRequestDTO dto)
        {
            try
            {
                var project = await _context.Projects.FindAsync(dto.ID);

                if (project == null)
                    return Result<AddNewProjectResponseDTO>.Failure(enOperationResult.NotFound);

                if (!string.IsNullOrEmpty(dto.ProjectName))
                    project.Name = dto.ProjectName;
                if (!string.IsNullOrEmpty(dto.Description))
                    project.Description = dto.Description;
                if (!string.IsNullOrEmpty(dto.Image))
                    project.Image = dto.Image;

                await _context.SaveChangesAsync();

                var updatedProject = await _context.Projects
                    .Select(p => new AddNewProjectResponseDTO
                    {
                        ProjectName = p.Name,
                        Description = p.Description,
                        ProjectID = p.Id,
                        CreatedBy = p.CreatedBy,
                        User = new DTOs.User.UserSummaryDTO
                        {
                            Id = p.CreatedByNavigation.Id,
                            Email = p.CreatedByNavigation.Email,
                            UserName = p.CreatedByNavigation.Username
                        },
                        Image = p.Image
                    }).FirstOrDefaultAsync(p => p.ProjectID == dto.ID);

                if (updatedProject == null)
                    return Result<AddNewProjectResponseDTO>.Failure(enOperationResult.Failed);

                return Result<AddNewProjectResponseDTO>.Success(updatedProject);

            }catch(Exception ex)
            {
                _logger.LogCritical(ex.Message);
                return Result<AddNewProjectResponseDTO>.Failure(enOperationResult.Failed);
            }
        }
    }
}
