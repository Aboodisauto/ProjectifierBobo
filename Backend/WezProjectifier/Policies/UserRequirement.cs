using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace WezProjectifier.Policies
{
    public class UserRequirement : IAuthorizationRequirement
    {
    }

    public class UserHandler : AuthorizationHandler<UserRequirement,int>
    {
        protected override Task HandleRequirementAsync
            (AuthorizationHandlerContext context, UserRequirement requirement,int UserID)
        {
            var UserNameIdentifier = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(int.TryParse(UserNameIdentifier, out int authUserId) && authUserId == UserID)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
