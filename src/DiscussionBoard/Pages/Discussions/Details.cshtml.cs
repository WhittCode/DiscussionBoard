using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages.Discussions;

public class DetailsModel : PageModel
{
    private readonly IDiscussionThreadService _threadService;
    private readonly IPostService _postService;

    public DetailsModel(IDiscussionThreadService threadService, IPostService postService)
    {
        _threadService = threadService;
        _postService = postService;
    }

    public DiscussionThread Thread { get; set; } = null!;
    public List<PostTreeBuilder.Node> PostTree { get; set; } = new();

    [BindProperty]
    public ReplyInputModel Input { get; set; } = new();

    public class ReplyInputModel
    {
        public int? ParentPostId { get; set; }

        [Required, StringLength(10000, MinimumLength = 1)]
        public string Content { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int threadId)
    {
        var thread = await _threadService.GetAsync(threadId);
        if (thread == null) return NotFound();

        Thread = thread;
        var posts = await _postService.GetForThreadAsync(threadId);
        PostTree = PostTreeBuilder.Build(posts);

        return Page();
    }

    public async Task<IActionResult> OnPostReplyAsync(int threadId)
    {
        var posts = await _postService.GetForThreadAsync(threadId);

        // Required by B4: Validate that any specified parent post belongs to the current discussion thread
        if (Input.ParentPostId.HasValue)
        {
            if (!posts.Any(p => p.Id == Input.ParentPostId.Value))
            {
                ModelState.AddModelError(string.Empty, "The parent post does not belong to this discussion thread.");
            }
        }

        if (!ModelState.IsValid)
        {
            var thread = await _threadService.GetAsync(threadId);
            if (thread == null) return NotFound();
            Thread = thread;

            PostTree = PostTreeBuilder.Build(posts);
            return Page();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Challenge();

        var post = new Post
        {
            Content = Input.Content,
            DiscussionThreadId = threadId,
            ParentPostId = Input.ParentPostId,
            ApplicationUserId = userId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _postService.AddAsync(post);


        return RedirectToPage("/Discussions/Details", new { threadId = threadId });
    }
}



