using Microsoft.EntityFrameworkCore;
using System.IO;

namespace PSXLink.MVVM.Models
{
    public class PSXLinkDataContext : DbContext
    {
        public DbSet<Game>? PSXGames { get; set; }
        public DbSet<Firmware>? PSXFirmwares { get; set; }

        public PSXLinkDataContext()
        {
            Database.Migrate();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            if (!Directory.Exists("Database"))
            {
                Directory.CreateDirectory("Database");
            }
            optionsBuilder.UseSqlite(@"Data Source=Database\PSXLink.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Firmware>().HasData(
                new Firmware
                {
                    ID = 1,
                    Title = "PS4 Firmware File",
                    Version = "0",
                    XmlLink = @"http://fus01.ps4.update.playstation.net/update/ps4/list/us/ps4-updatelist.xml"
                }
                );
        }
    }
}
