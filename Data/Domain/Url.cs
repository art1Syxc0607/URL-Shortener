using System.ComponentModel.DataAnnotations;

namespace Data.Domain;

public class Url
{
    public int Id { get; set; }

    [Required]
    public string OriginalFullUrl { get; set; } = string.Empty;

    [Required]
    public string Pseudonym { get; set; } = string.Empty; // Ищем по псевдониму

    public string? Password { get; set; } // Пароль может быть null
}