using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WezProjectifierBLL;
using WezProjectifierBLL.DTOs.Members;
using WezProjectifierBLL.DTOs.User;
using WezProjectifierBLL.Services;

namespace WezProjectifier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly clsMemberService _member;
        private readonly IAuthorizationService _authorizationService;

        public MemberController(clsMemberService memberService, IAuthorizationService authorizationService)
        {
            _member = memberService;
            _authorizationService = authorizationService;
        }

        [HttpPost("addMemberToProject/{projectID::int}/{userID::int}/{role}")]
        [Authorize]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<ActionResult<AddMemberResponseDTO>> AddMemberToProject(int projectID, int userID,string role)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, projectID, "ProjectAdmin");
            if (!authResult.Succeeded)
                return Forbid();
            var result = await _member.AddUserToProject(projectID, userID, role);

            return result.OperationResult switch
            {
                enOperationResult.Success => Ok(result.Value),
                enOperationResult.NotFound => NotFound("Project/User with this id doesn't exist"),
                enOperationResult.InvalidData => BadRequest("Please validate your inputs"),
                enOperationResult.AlreadyExist => Conflict("This user is already added to the project"),
                _ => Problem(statusCode:500,title:"Internal server error",detail:"Hold on until we fix the issue")
            };
        }

        [HttpGet("all/{projectId::int}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<List<UserSummaryDTO>>> GetAllUsersInProject(int projectId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User,projectId, "ProjectMemberOrAdmin");
            var result = await _member.GetAllMembers(projectId);

            return result.OperationResult switch 
            {
                enOperationResult.Success => Ok(result.Value),
                enOperationResult.NotFound => NotFound("Project with this id doesn't exist"),
                enOperationResult.InvalidData => BadRequest("Please validate your input"),
                _ => Problem(statusCode: 500, title: "Internal server error", detail: "Hold on until we fix the issue")
            };
        }

        [HttpDelete("remove/{projectId::int}/{userId::int}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> RemoveUserFromProjectAsync(int projectId, int userId)
        {
            var authResult = await _authorizationService.AuthorizeAsync(User, projectId, "ProjectAdmin");
            if (!authResult.Succeeded)
                return Forbid();
            var result = await _member.RemoveUserFromProjectAsync(projectId, userId);

            return result.OperationResult switch
            {
                enOperationResult.Success => Ok("Succesfuly removed the member from the project"),
                enOperationResult.NotFound => NotFound("That member is not registered in the project"),
                enOperationResult.InvalidData => BadRequest("Please enter valid userId and projectId"),
                _ => Problem(statusCode:500,title:"Internal server error",detail:"Hold on until we fix the issue")
                
            };
        }
    }
}
