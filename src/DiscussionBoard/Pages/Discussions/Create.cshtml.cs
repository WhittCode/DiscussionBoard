using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions;

[Authorize]
public class CreateModel : PageModel
{
    private readonly IDiscussionThreadService _threadService;

    public CreateModel(IDiscussionThreadService threadService) => _threadService = threadService;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public int ForumId { get; set; }

    public class InputModel
    {
        [Required, StringLength(200, MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(10000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var thread = new DiscussionThread
        {
            Title = Input.Title,
            Content = Input.Content,
            ForumId = ForumId,
            ApplicationUserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _threadService.AddAsync(thread);
        return RedirectToPage("/Forums/Details", new { id = ForumId });
    }
}

