using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Domain;

public class Url
{
    public int Id { get; set; }
    public string OriginalFullUrl { get; set; }
    public string NewFullUrl { get; set; }

    [MinLength(3)]
    [MaxLength(15)]
    [Required]
    public string Pseudonym { get; init; }

    public string? Password { get; set; }
}
