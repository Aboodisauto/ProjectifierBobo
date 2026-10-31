using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WezProjectifier.Models;
using WezProjectifierBLL;
using WezProjectifierBLL.DTOs.Projects;
using WezProjectifierBLL.DTOs.Tasks;
using WezProjectifierBLL.Services;

namespace WezProjectifier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectTaskController : ControllerBase
    {
        private readonly clsTaskService _tasks;
        private readonly IAuthorizationService _authorizationService;
        public ProjectTaskController (clsTaskService taskService, IAuthorizationService authorizationService)
        {
            _tasks = taskService;
            _authorizationService = authorizationService;
        }

        [HttpGet("all/{projectId::int}",Name = "GetAllTasks")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<TaskDTO>>> GetAllTasksForProjectAsync(int projectId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, projectId, "ProjectAdmin");
            if (!authResult.Succeeded)
                return Forbid();
            var result = await _tasks.GetAllTasksAsync(projectId);

            return result.OperationResult switch
            {
                enOperationResult.Success => Ok(result.Value),
                enOperationResult.NotFound => NotFound("Project with id doesn't exist"),
                _ => Problem(statusCode:500
                ,title:"Internal server error"
                ,detail:"Hold on until we fix the issue")
            };
        }

        [HttpPost("addTask",Name = "AddTaskToProject")]
        [Authorize]
        [ProducesResponseType(201)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<bool>> AddNewTaskToProject(AddTaskRequestDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, dto.ProjectID, "ProjectAdmin");
            if (!authResult.Succeeded)
                return Forbid();
            var result = await _tasks.CreateAddTaskToProjectAsync(dto);

            return result.OperationResult switch 
            {
                enOperationResult.Success => Ok(result.Value),
                enOperationResult.NotFound => NotFound("There's no existing project with this ID"),
                enOperationResult.InvalidData => BadRequest("Please Enter Valid Naming and Data"),
                _ => Problem(statusCode:500,title:"Internal server error",detail:"Hold on until we fix the issue")
            };
        }

        

        [HttpPut("editTask")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<UpdateTaskResponseDTO>> UpdateTask(
            [FromBody] UpdateTaskRequestDTO dto)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, dto.ProjectID, "ProjectAdmin");
            if (!authResult.Succeeded)
                return Forbid();
            var result = await _tasks.UpdateTaskAsync(dto);

            return result.OperationResult switch 
            {
                enOperationResult.Success => Ok(result.Value),
                enOperationResult.NotFound => NotFound("Please check your task/project id, because they don't exist"),
                enOperationResult.InvalidData => BadRequest("Please check your input"),
                _ => Problem(statusCode:500,title:"Internal server error",detail:"Hold on until we fix this issue")
            };
        }

        [HttpDelete("delete/{projectId::int}/{taskId::int}",Name = "DeleteTaskFromProject")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<bool>> DeleteTaskFromProjectAsync(int projectId,int taskId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, projectId, "ProjectAdmin");
            if (!authResult.Succeeded)
                return Forbid();
            var result = await _tasks.DeleteTaskFromProjectAsync(projectId, taskId);

            return result.OperationResult switch
            {
                enOperationResult.Success => Ok("Succesfuly deleted the task"),
                enOperationResult.NotFound => NotFound("Project/Task doesn't exist"),
                enOperationResult.InvalidData => BadRequest("Please enter a valid id"),
                _ => Problem(statusCode:500,title:"Internal Server Error",detail:"Hold on until we fix the issue")
                
            };

        }
    }
}
