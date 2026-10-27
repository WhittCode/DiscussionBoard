using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions;

[Authorize]
public class EditModel : PageModel
{
    private readonly IDiscussionThreadService _threadService;

    public EditModel(IDiscussionThreadService threadService) => _threadService = threadService;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public class InputModel
    {
        [Required, StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(10000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var thread = await _threadService.GetAsync(Id);
        if (thread == null) return NotFound();

        if (thread.ApplicationUserId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();

        Input.Title = thread.Title;
        Input.Content = thread.Content;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var thread = await _threadService.GetAsync(Id);
        if (thread == null) return NotFound();

        if (thread.ApplicationUserId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();

        thread.Title = Input.Title;
        thread.Content = Input.Content;

        await _threadService.UpdateAsync(thread);
        return RedirectToPage("./Details", new { threadId = Id });
    }
}
