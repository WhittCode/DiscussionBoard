using System.ComponentModel.DataAnnotations;

namespace DiscussionBoard.ViewModels;

public class UserProfile
{
    [Required]
    [StringLength(60, MinimumLength = 2, ErrorMessage = "Display name must be between 2 and 60 characters.")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    public string AvatarId { get; set; } = "default";
}
