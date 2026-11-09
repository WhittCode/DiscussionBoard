using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages;

[Authorize]
public class ThreadEditModel : PageModel
{
    private readonly IDiscussionThreadService _threadService;
    private readonly IApplicationAuthorizationService _authorizationService;

    public ThreadEditModel(
        IDiscussionThreadService threadService,
        IApplicationAuthorizationService authorizationService)
    {
        _threadService = threadService;
        _authorizationService = authorizationService;
    }

    [BindProperty]
    public DiscussionEditViewModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int threadId)
    {
        var thread = await _threadService.GetAsync(threadId);

        if (thread == null)
            return NotFound();

        if (!await _authorizationService.CanEditAsync(User, thread))
            return Forbid();

        Input = new DiscussionEditViewModel
        {
            Title = thread.Title,
            Content = thread.Content
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int threadId)
    {
        var thread = await _threadService.GetAsync(threadId);

        if (thread == null)
            return NotFound();

        if (!await _authorizationService.CanEditAsync(User, thread))
            return Forbid();

        if (!ModelState.IsValid)
            return Page();

        thread.Title = Input.Title;
        thread.Content = Input.Content;

        await _threadService.UpdateAsync(thread);

        return RedirectToPage(
            "/Discussions/Details",
            new { threadId });
    }
}
