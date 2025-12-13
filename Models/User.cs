using System.ComponentModel.DataAnnotations;

namespace MusicLearningApp.Models;

public class User
{
    public int Id { get; set; }

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Gender { get; set; } = "Other"; // Male, Female, Other

    [Required]
    public DateTime BirthDate { get; set; }

    [Required]
    public string Role { get; set; } = "User"; // Admin, User
}