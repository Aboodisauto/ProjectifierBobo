using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WezProjectifierBLL;
using WezProjectifierBLL.DTOs.Projects;
using WezProjectifierBLL.Services;

namespace WezProjectifier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly clsProjectService _projects;
        public ProjectController(clsProjectService projects)
        {
            _projects = projects;
        }

        [HttpGet("all", Name = "FetchAllProjects")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProjectSummaryDTO>> GetAllProjectsAsync()
        {
            var result = await _projects.GetAllProjects();
            return Ok(result);
        }

        [HttpGet("getById/{id::int}",Name = "GetProjectByID")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<GetProjectInfoDTO>> GetProjectByID(int id)
        {
            var result = await _projects.GetProjectInfoAsync(id);

            return result.OperationResult switch
            {
                enOperationResult.NotFound => Problem(
                    statusCode: StatusCodes.Status404NotFound,
                    title: "Not Found !",
                    detail: "Enter a valid project id"),
                enOperationResult.Success => Ok(result.Value),
                _ => Problem(statusCode:500,
                title:"Internal Server Error",
                detail:"Hold on until we fix the server")
            };
        }

        [HttpPost("Add", Name = "AddNewProject")]
        [ProducesResponseType(201)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AddNewProjectResponseDTO>> AddNewProject([FromBody] AddNewProjectRequestDTO dto)
        {
            var result = await _projects.AddNewProjectAsync(dto);

            return result.OperationResult switch
            {
                enOperationResult.Success => Created(string.Empty, result.Value),
                enOperationResult.InvalidData => BadRequest(result),
                enOperationResult.NotFound => Problem(
                    statusCode:404,
                    title:"Invalid CreatedBy ID",
                    detail:"The owner was not found among the users table"
                    ),
                _ => Problem(statusCode: 500, title: "Internal Server Error", detail: "Hold on until we fix the server")
            };
        }

        [HttpPut("update/",Name = "UpdateProject")]
        public async Task<ActionResult<AddNewProjectResponseDTO>> UpdateExistingProject(
            [FromBody] UpdateProjectRequestDTO dto)
        {
            var updated = await _projects.UpdateProjectAsync(dto);

            return updated.OperationResult switch
            {
                enOperationResult.Success => Ok(updated.Value),
                enOperationResult.NotFound => Problem(statusCode:404,
                title:"Not Found !",
                detail:"There's no project with this id"),
                enOperationResult.Failed => Problem(statusCode:500,
                title:"Internal Server Error",
                detail:"Hold on until we fix the issue")
            };
        }

        [HttpDelete("delete/{id::int}", Name = "DeleteProject")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<bool>> DeleteProjectAsync(int id)
        {
            var result = await _projects.DeleteProjecyAsync(id);
            return result.OperationResult switch {
                enOperationResult.Success => Ok($"Succesfuly deleted the proejct with id {id}"),
                enOperationResult.Failed => Problem(statusCode:500,
                title:"Internal Server Error",detail:"Hold on until we fix the issue"),
                enOperationResult.NotFound => Problem(statusCode:404,
                title:"Not Found !",detail:"No Project found with this id")
            };
        }

        [HttpGet("getProjectFor/{userId::int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<HashSet<ProjectSummaryDTO>>> GetAllProjectsForUser(int userId)
        {
            var result = await _projects.GetAllProjectsForUser(userId);

            return result.OperationResult switch
            {
                enOperationResult.Success => Ok(result.Value),
                enOperationResult.InvalidData => BadRequest("Enter a valid userId ."),
                enOperationResult.NotFound => NotFound("No user with that id exists"),
                _ => Problem(statusCode:500,title:"Internal server error",detail:"Hold on until we fix the issue")
            };
        }
    }
}
