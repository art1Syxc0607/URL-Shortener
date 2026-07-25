using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Data.Domain;

namespace Data;

public class AppDbContext : DbContext
{
    public DbSet<Url> Ulrs { get; set; }


    // ✅ ДОБАВЬТЕ ЭТОТ КОНСТРУКТОР
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) // ← Передаем настройки в базу (DbContext)
    {

    }
}
