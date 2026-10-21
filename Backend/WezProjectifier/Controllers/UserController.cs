using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WezProjectifier.Models;
using WezProjectifierBLL;
using WezProjectifierBLL.DTOs;
using WezProjectifierBLL.Services;

namespace WezProjectifier.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly clsUserService _user;
        public UserController(clsUserService user)
        {
            _user = user;
        }


        [HttpPost(Name = "create")]
        [Authorize]
        [ProducesResponseType((int)StatusCodes.Status201Created)]
        [ProducesResponseType((int)StatusCodes.Status400BadRequest)]
        [ProducesResponseType((int)StatusCodes.Status409Conflict)]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserDTO userDTO)
        {
            if (userDTO.Image == "string")
            {
                userDTO.Image = null;
            }

            var result = await _user.CreateUserAsync(userDTO.FirstName, userDTO.LastName,
                userDTO.Email, userDTO.PhoneNumber, userDTO.Username,
                userDTO.Password, userDTO.Address, userDTO.Image ?? null);

            return result.enResult switch
            {
                enOperationResult.Success => Ok("User Was Create Successfully"),
                enOperationResult.AlreadyExist => Problem(statusCode: StatusCodes.Status409Conflict,
                title: "Duplicate Data", detail: result.Message),
                enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Data !"),
                _ => Problem(statusCode: StatusCodes.Status500InternalServerError, title: "Internal Server Error !")
            };
        }

        [HttpGet("all")]
        [Authorize]
        [ProducesResponseType((int)StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> fetchAllUsers()
        {
            var result = await _user.GetAllUsersAsync();
            if (result.Count == 0)
            {
                return Ok("No Users Found");
            }
            return Ok(result);
        }

        [HttpGet("getById/{id::int}")]
        [ProducesResponseType((int)StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetUserByIdAsync(int id)
        {
            var result = await _user.FindUserAsync(id);

            return result.OperationResult switch
            {
                enOperationResult.Success => Ok(result.Value),
                enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Data", detail: "Please enter valid ID"),
                enOperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not Found !", detail: $"User with The ID {id} was not found"),
                _ => Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error", detail: "Hold on until we fix the issue")
            };
        }
        [HttpDelete("delete/{id::int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteUserAsync(int id)
        {
            var result = await _user.DeleteUserAsync(id);
            return result switch
            {
                enOperationResult.Success => Ok("Succesfuly Deleted The User !"),
                enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid ID", detail: "Please enter a valid id"),
                enOperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not Found", detail: "User with this id doesn't exist"),
                _ => Problem(statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error", detail: "Hold on until we fix the issue")
            };
        }

        [HttpPatch("update/{id::int}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<User>> UpdateUserAsync(int id, [FromBody] UpdateUserDTO dto)
        {
            var result = await _user.UpdateUserAsync(id, dto);
            return result.OperationResult switch
            {
                enOperationResult.Success => Ok(result.Value),
                enOperationResult.NotFound => Problem(statusCode: StatusCodes.Status404NotFound,
                title: "Not Found", detail: $"User with the id {id} was not found"),
                enOperationResult.InvalidData => Problem(statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Data", detail: $"Invalid Data Provided"),
                _ => Problem(statusCode: 500, title: "Internal Server Error", detail: "Hold on until we fix the issue")
            };
        }
    }
}
