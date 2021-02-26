using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Playground
{
    public class AppDbContext : DbContext
    {

        public AppDbContext()
        {
            ChangeTracker.LazyLoadingEnabled = false;
        }
        public DbSet<Account> Accounts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql("Host=localhost;Database=bank;Username=postgres;Password=admin")               
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }
    }

    [Table("accounts")]
    public class Account
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("balance")]
        public int Balance { get; set; }   

    }

}
