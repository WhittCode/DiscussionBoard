using System;
using System.Linq;
using System.Threading.Tasks;
using DiscussionBoard.Data;
using Microsoft.EntityFrameworkCore;

namespace DiscussionBoard.Services;

public class UserProfileService : IUserProfileService
{
    private readonly ApplicationDbContext _context;

    public UserProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsDisplayNameAvailableAsync(string displayName, string? excludeUserId = null)
    {
        if (string.IsNullOrWhiteSpace(displayName)) return false;

        string trimmedName = displayName.Trim();
        if (trimmedName.Length < 2 || trimmedName.Length > 60) return false;

        var query = _context.Users.Where(u => u.DisplayName.ToLower() == trimmedName.ToLower());

        if (!string.IsNullOrEmpty(excludeUserId))
        {
            query = query.Where(u => u.Id != excludeUserId);
        }

        bool isTaken = await query.AnyAsync();
        return !isTaken;
    }
}
