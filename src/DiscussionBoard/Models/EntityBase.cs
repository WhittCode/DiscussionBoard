using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.Models;

public abstract class EntityBase
{
    [Key]
    public int Id { get; set; }
    public bool IsDeleted { get; set; } = false;
}
