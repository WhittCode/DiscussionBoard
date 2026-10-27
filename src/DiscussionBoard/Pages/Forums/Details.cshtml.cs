using System.Collections.Generic;
using System.Threading.Tasks;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Pages.Forums;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IDiscussionThreadService _threadService;

    public DetailsModel(ApplicationDbContext context, IDiscussionThreadService threadService)
    {
        _context = context;
        _threadService = threadService;
    }

    public Forum Forum { get; set; } = null!;
    public IEnumerable<DiscussionThread> Threads { get; set; } = new List<DiscussionThread>();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var forum = await _context.Forums.FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
        if (forum == null) return NotFound();

        Forum = forum;
        Threads = await _threadService.GetForForumAsync(id);

        return Page();
    }
}
