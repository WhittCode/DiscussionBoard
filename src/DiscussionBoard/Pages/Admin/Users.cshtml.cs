
using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages.Admin;

[Authorize(Roles = ApplicationRoles.Admin)]
[ValidateAntiForgeryToken]
public class UsersModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationAuthorizationService _authorizationService;

    public UsersModel(
        UserManager<ApplicationUser> userManager,
        IApplicationAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
    }

    public IList<ApplicationUser> Users { get; set; } =
        new List<ApplicationUser>();

    public ApplicationUser? SelectedUser { get; set; }

    public bool SelectedUserIsAdmin { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    [BindProperty]
    public string? SelectedUserId { get; set; }

    public async Task OnGetAsync(string? selectedUserId = null)
    {
        SelectedUserId = selectedUserId;
        await LoadPageDataAsync();
    }

    public async Task<IActionResult> OnPostSelectUserAsync()
    {
        await LoadPageDataAsync();

        if (SelectedUser == null)
        {
            StatusMessage = "Please select a valid user.";
        }

        return Page();
    }

    public Task<IActionResult> OnPostAddAsync()
    {
        return UpdateRoleAsync(addRole: true);
    }

    public Task<IActionResult> OnPostRemoveAsync()
    {
        return UpdateRoleAsync(addRole: false);
    }

    private async Task<IActionResult> UpdateRoleAsync(bool addRole)
    {
        if (string.IsNullOrWhiteSpace(SelectedUserId))
        {
            StatusMessage = "Please select a valid user.";
            return RedirectToPage();
        }

        IdentityResult result;

        if (addRole)
        {
            result = await _authorizationService.AddRoleAsync(
                User,
                SelectedUserId,
                ApplicationRoles.Admin);
        }
        else
        {
            result = await _authorizationService.RemoveRoleAsync(
                User,
                SelectedUserId,
                ApplicationRoles.Admin);
        }

        if (result.Succeeded)
        {
            StatusMessage = addRole
                ? "The Admin role was successfully added."
                : "The Admin role was successfully removed.";
        }
        else
        {
            StatusMessage = string.Join(
                " ",
                result.Errors.Select(error => error.Description));
        }

        return RedirectToPage(new { selectedUserId = SelectedUserId });
    }

    private async Task LoadPageDataAsync()
    {
        Users = await _userManager.Users
            .OrderBy(user => user.DisplayName)
            .ThenBy(user => user.Email)
            .ToListAsync();

        SelectedUser = null;
        SelectedUserIsAdmin = false;

        if (string.IsNullOrWhiteSpace(SelectedUserId))
        {
            return;
        }

        SelectedUser = await _userManager.FindByIdAsync(SelectedUserId);

        if (SelectedUser != null)
        {
            SelectedUserIsAdmin = await _userManager.IsInRoleAsync(
                SelectedUser,
                ApplicationRoles.Admin);
        }
    }
}
