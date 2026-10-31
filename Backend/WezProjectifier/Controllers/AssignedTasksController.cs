using Microsoft.AspNetCore.Authorization;
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
        private readonly IAuthorizationService _authorizationService;

        public AssignedTasksController 
            (clsAssignedTasksService tasksService, IAuthorizationService authorizationService)
        {
            _tasksService = tasksService;
            _authorizationService = authorizationService;
        }

        [HttpGet("getById/{projectId::int}/{userId::int}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> GetTasksForPersonInProject(int userId, int projectId)
        {
            var authResultForAdmin = await _authorizationService.AuthorizeAsync(User, projectId, "ProjectAdmin");
            var authResultOwnerShip = await _authorizationService.AuthorizeAsync(User, userId, "UserOwnsResource");
            if (!authResultForAdmin.Succeeded || !authResultOwnerShip.Succeeded)
                return Forbid();
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
        [Authorize]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> AssignTaskToUser(int taskId, int projectId, int userId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User,projectId,"ProjectAdmin");
            if (!authResult.Succeeded)
                return Forbid();
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
        [Authorize]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> UnassignTaskFromUser(int taskId, int projectId, int userId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, projectId, "ProjectAdmin");
            if (!authResult.Succeeded)
                return Forbid();
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
