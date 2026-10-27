using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DiscussionBoard.Services;

public interface IDiscussionThreadService
{
    Task<DiscussionThread?> GetAsync(int id);
    Task<IEnumerable<DiscussionThread>> GetForForumAsync(int forumId);
    Task AddAsync(DiscussionThread thread);
    Task UpdateAsync(DiscussionThread thread);
}

public class DiscussionThreadService : IDiscussionThreadService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DiscussionThreadService>? _logger;


    public DiscussionThreadService(ApplicationDbContext context, ILogger<DiscussionThreadService> logger)
    {
        _context = context;
        _logger = logger;
    }


    public DiscussionThreadService(ApplicationDbContext context)
    {
        _context = context;
        _logger = null;
    }

    public async Task<DiscussionThread?> GetAsync(int id)
    {
        return await _context.DiscussionThreads
            .Include(t => t.ApplicationUser)
            .Include(t => t.Forum)
            .FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted);
    }

    public async Task<IEnumerable<DiscussionThread>> GetForForumAsync(int forumId)
    {

        var threads = await _context.DiscussionThreads
            .Include(t => t.ApplicationUser)
            .Where(t => t.ForumId == forumId && !t.IsDeleted)
            .ToListAsync();


        return threads
            .OrderByDescending(t => t.CreatedAt)
            .ToList();
    }

    public async Task AddAsync(DiscussionThread thread)
    {
        try
        {
            _context.DiscussionThreads.Add(thread);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create thread titled {Title}", thread.Title);
            throw;
        }
    }

    public async Task UpdateAsync(DiscussionThread thread)
    {
        try
        {
            var existing = await _context.DiscussionThreads.FirstOrDefaultAsync(t => t.Id == thread.Id && !t.IsDeleted);
            if (existing == null) throw new KeyNotFoundException("Thread targets missing entity context.");


            existing.Title = thread.Title;
            existing.Content = thread.Content;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to update target details configuration for ID {Id}", thread.Id);
            throw;
        }
    }
}

