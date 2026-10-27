using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.Models;

public class Post : DiscussionBase
{
    [Required]
    public int DiscussionThreadId { get; set; }
    public DiscussionThread? DiscussionThread { get; set; }

    public int? ParentPostId { get; set; }
    public Post? ParentPost { get; set; }

    public ICollection<Post> Replies { get; set; } = new List<Post>();
}
