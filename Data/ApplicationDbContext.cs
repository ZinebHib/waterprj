using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using waterprj.Models;

namespace waterprj.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<waterprj.Models.Consumption>? Consumption { get; set; }
        public DbSet<waterprj.Models.Estimation>? Estimation { get; set; }
    }
}
