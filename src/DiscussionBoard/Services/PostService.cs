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
    Task<Post?> GetAsync(int id);
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
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

    public async Task<Post?> GetAsync(int id)
    {
        return await _context.Posts
            .Include(p => p.ApplicationUser)
            .Include(p => p.DiscussionThread)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task AddAsync(Post post)
    {
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Post post)
    {
        var existingPost = await _context.Posts
            .FirstOrDefaultAsync(p => p.Id == post.Id && !p.IsDeleted);

        if (existingPost == null)
        {
            throw new InvalidOperationException("The post could not be found.");
        }


        existingPost.Title = post.Title;
        existingPost.Content = post.Content;

        await _context.SaveChangesAsync();
    }
}
