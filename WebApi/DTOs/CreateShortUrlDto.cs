using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs;

public record CreateShortUrlDto
{
    [Url]
    [Required]
    public string OriginalUrl { get; init; }

    [MinLength(3)]
    [MaxLength(15)]
    [Required]
    public string Pseudonym { get; init; }

    public string? Password { get; init; }
}
