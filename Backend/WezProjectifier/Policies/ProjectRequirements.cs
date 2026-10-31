using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WezProjectifier.Models;

namespace WezProjectifier.Policies;

public sealed class ProjectRequirements : IAuthorizationRequirement
{
}
public sealed class ProjectAdminRequirement : IAuthorizationRequirement
{
}
public sealed class ProjectUserRequirement : IAuthorizationRequirement
{
}
public sealed class HasToBeProjectMember : AuthorizationHandler<ProjectRequirements, int>
{
    private readonly WezDbContext _context;

    public HasToBeProjectMember(WezDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProjectRequirements requirement,
        int projectId)
    {
        var userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (projectId <= 0 || !int.TryParse(userIdValue, out var userId))
        {
            return;
        }

        var isCreator = await _context.Projects
            .AsNoTracking()
            .AnyAsync(p => p.Id == projectId && p.CreatedBy == userId);

        var isProjectAdminOrMember = await _context.Members
            .AsNoTracking()
            .AnyAsync(m =>
                m.ProjectId == projectId &&
                m.UserId == userId &&
                (m.Role == "Owner" || m.Role == "User"));

        if (isCreator || isProjectAdminOrMember)
        {
            context.Succeed(requirement);
        }
    }
}
public sealed class HasToBeAdminProjectHandler : AuthorizationHandler<ProjectAdminRequirement, int>
{
    private readonly WezDbContext wezDbContext;
    public HasToBeAdminProjectHandler(WezDbContext wezDbContext)
    {
        this.wezDbContext = wezDbContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ProjectAdminRequirement requirement, int projectId)
    {
        string userIdValue = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out int UserID))
        {
            return;
        }
        var isAdmin = await wezDbContext.Members.AnyAsync(m => m.UserId == UserID && m.Role == "Admin");
        if (isAdmin)
        {
            context.Succeed(requirement);
        }

    }
}
