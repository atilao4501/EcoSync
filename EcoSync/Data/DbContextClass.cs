using Microsoft.EntityFrameworkCore;
using EcoSync.Models;

namespace EcoSync.Data;

public class DbContextClass : DbContext
{
    public DbContextClass(DbContextOptions<DbContextClass> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

    public DbSet<Bairro> Bairros { get; set; }
    public DbSet<Pontuacoes> Pontuacoes { get; set; }
}