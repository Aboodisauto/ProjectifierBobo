using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WezProjectifierBLL;
using WezProjectifierBLL.Services;

namespace WezProjectifier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AssignedTasksController : ControllerBase
    {
        private readonly clsAssignedTasksService _tasksService;

        public AssignedTasksController (clsAssignedTasksService tasksService)
        {
            _tasksService = tasksService;
        }

        [HttpGet("getById/{projectId::int}/{userId::int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> GetTasksForPersonInProject(int userId, int projectId)
        {
            var result = await _tasksService.GetAllAssignedTasksAsync(projectId, userId);
            return result.OperationResult switch
            {
                enOperationResult.Success => Ok(result.Value),
                enOperationResult.InvalidData => BadRequest("Please enter valid userId/projectId"),
                enOperationResult.NotFound => NotFound("User/Project you entered doesn't exist"),
                enOperationResult.NotFound2 => NotFound("That is user is not a part of that project"),
                _ => Problem(statusCode:500,title:"Internal server error",detail:"Hold on until we fix the issue")
            };
        }

        [HttpPost("assign/{taskId::int}/{projectId::int}/{userId::int}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> AssignTaskToUser(int taskId, int projectId, int userId)
        {
            var result = await _tasksService.AssignTaskToUserAsync(taskId, projectId, userId);
            return result.OperationResult switch
            {
                enOperationResult.Success => Ok("The task was assigned to the user"),
                enOperationResult.NotFound => NotFound("Task/Project/User does not exist"),
                enOperationResult.NotFound2 => NotFound("The member was not assigned to the project"),
                enOperationResult.InvalidData => BadRequest("Please enter valid data"),
                _ => Problem(statusCode:500,title:"Internal server error",detail:"Hold on until we fix the issue")
            };
        }

        [HttpDelete("unassign/{taskId::int}/{projectId::int}/{userId::int}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> UnassignTaskFromUser(int taskId, int projectId, int userId)
        {
            var result = await _tasksService.UnassignTaskFromUserAsync(taskId, projectId, userId);

            return result.OperationResult switch 
            {
                enOperationResult.Success => Ok(result.Value),
                enOperationResult.NotFound => NotFound("User/Task/Project doesn't exist"),
                enOperationResult.InvalidData => BadRequest("Enter a valid inputs"),
                _ => Problem(statusCode:500,title:"Internal server error",detail:"Hold on until we fix the issue")
            };
        }
    }
}
