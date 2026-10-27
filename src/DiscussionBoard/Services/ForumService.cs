using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services;

public class ForumService
{
    private readonly ApplicationDbContext _context;

    public ForumService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Forum>> GetAllAsync()
    {
        return await _context.Forums
            .Where(f => !f.IsDeleted)
            .ToListAsync();
    }
}
