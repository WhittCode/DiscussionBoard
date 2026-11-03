using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace DiscussionBoard.Models;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public string AvatarId { get; set; } = "default";


    public virtual ICollection<DiscussionThread> DiscussionThreads { get; set; } = new List<DiscussionThread>();
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
