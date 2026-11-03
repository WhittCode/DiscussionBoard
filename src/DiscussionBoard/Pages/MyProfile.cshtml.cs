using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DiscussionBoard.Models;
using DiscussionBoard.ViewModels;
using DiscussionBoard.Services;

namespace DiscussionBoard.Pages;

[Authorize]
public class MyProfileModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserProfileService _profileService;

    public MyProfileModel(
        UserManager<ApplicationUser> userManager,
        IUserProfileService profileService)
    {
        _userManager = userManager;
        _profileService = profileService;
    }

    [BindProperty]
    public UserProfile Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        Input = new UserProfile
        {
            DisplayName = user.DisplayName,
            AvatarId = user.AvatarId
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        if (!ModelState.IsValid) return Page();

        // Access the static class directly
        if (!AvatarCatalog.IsValid(Input.AvatarId))
        {
            ModelState.AddModelError(nameof(Input.AvatarId), "Invalid avatar selected.");
            return Page();
        }

        bool isAvailable = await _profileService.IsDisplayNameAvailableAsync(Input.DisplayName, user.Id);
        if (!isAvailable)
        {
            ModelState.AddModelError("Input.DisplayName", "This display name is already taken.");
            return Page();
        }

        user.DisplayName = Input.DisplayName.Trim();
        user.AvatarId = Input.AvatarId;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        StatusMessage = "Your profile has been updated successfully.";
        return RedirectToPage();
    }
}
