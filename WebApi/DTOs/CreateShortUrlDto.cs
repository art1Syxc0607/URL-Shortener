using System.ComponentModel.DataAnnotations;

namespace WebApi.DTOs;

public record CreateShortUrlDto(
    [Required, Url] string OriginalUrl,
    [Required, MinLength(3), MaxLength(15)] string Pseudonym,
    string? Password
);