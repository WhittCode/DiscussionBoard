using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services;

public interface IPostService
{
    Task<IEnumerable<Post>> GetForThreadAsync(int threadId);
    Task AddAsync(Post post);
}

public class PostService : IPostService
{
    private readonly ApplicationDbContext _context;

    public PostService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Post>> GetForThreadAsync(int threadId)
    {

        return await _context.Posts
            .Include(p => p.ApplicationUser)
            .Where(p => p.DiscussionThreadId == threadId && !p.IsDeleted)
            .ToListAsync();
    }

    public async Task AddAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
    }
}

