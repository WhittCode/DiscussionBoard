

using System.Security.Claims;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace DiscussionBoard.Authorization;

public class ApplicationAuthorizationService : IApplicationAuthorizationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthorizationService _authorizationService;

    public ApplicationAuthorizationService(
        UserManager<ApplicationUser> userManager,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
    }

    public async Task<bool> CanEditAsync(
        ClaimsPrincipal principal,
        DiscussionBase resource)
    {
        var result = await _authorizationService.AuthorizeAsync(
            principal,
            resource,
            ApplicationAuthorizationPolicies.IsOwnerOrAdmin);

        return result.Succeeded;
    }

    public async Task<IdentityResult> AddRoleAsync(
        ClaimsPrincipal actingUser,
        string targetUserId,
        string roleName)
    {
        // Verify that the acting user is authenticated and an administrator.
        if (actingUser.Identity?.IsAuthenticated != true ||
            !actingUser.IsInRole(ApplicationRoles.Admin))
        {
            return Failure("You are not authorized to change user roles.");
        }

        if (roleName != ApplicationRoles.Admin)
        {
            return Failure("Only the Admin role can be assigned.");
        }

        if (string.IsNullOrWhiteSpace(targetUserId))
        {
            return Failure("A valid user must be selected.");
        }

        var actingUserId = actingUser.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // Prevent administrators from granting themselves the Admin role.
        if (actingUserId == targetUserId)
        {
            return Failure("You cannot assign the Admin role to yourself.");
        }

        var targetUser = await _userManager.FindByIdAsync(targetUserId);

        if (targetUser == null)
        {
            return Failure("The selected user could not be found.");
        }

        if (await _userManager.IsInRoleAsync(targetUser, roleName))
        {
            return IdentityResult.Success;
        }

        return await _userManager.AddToRoleAsync(targetUser, roleName);
    }

    public async Task<IdentityResult> RemoveRoleAsync(
        ClaimsPrincipal actingUser,
        string targetUserId,
        string roleName)
    {
        // Verify that the acting user is authenticated and an administrator.
        if (actingUser.Identity?.IsAuthenticated != true ||
            !actingUser.IsInRole(ApplicationRoles.Admin))
        {
            return Failure("You are not authorized to change user roles.");
        }

        if (roleName != ApplicationRoles.Admin)
        {
            return Failure("Only the Admin role can be removed.");
        }

        if (string.IsNullOrWhiteSpace(targetUserId))
        {
            return Failure("A valid user must be selected.");
        }

        var actingUserId = actingUser.FindFirstValue(
            ClaimTypes.NameIdentifier);

        // Prevent administrators from removing their own Admin role.
        if (actingUserId == targetUserId)
        {
            return Failure("You cannot remove your own Admin role.");
        }

        var targetUser = await _userManager.FindByIdAsync(targetUserId);

        if (targetUser == null)
        {
            return Failure("The selected user could not be found.");
        }

        if (!await _userManager.IsInRoleAsync(targetUser, roleName))
        {
            return IdentityResult.Success;
        }

        // Prevent removal of the last administrator.
        var administrators = await _userManager.GetUsersInRoleAsync(
            ApplicationRoles.Admin);

        if (administrators.Count <= 1)
        {
            return Failure("The last administrator cannot be removed.");
        }

        return await _userManager.RemoveFromRoleAsync(targetUser, roleName);
    }

    private static IdentityResult Failure(string description)
    {
        return IdentityResult.Failed(
            new IdentityError
            {
                Description = description
            });
    }
}

