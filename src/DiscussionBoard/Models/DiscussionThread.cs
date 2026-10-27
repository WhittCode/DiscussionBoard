using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.Models;

public class DiscussionThread : DiscussionBase
{
    [Required]
    public int ForumId { get; set; }
    public Forum? Forum { get; set; }

    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
