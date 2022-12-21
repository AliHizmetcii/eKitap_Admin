using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using eKitap.Models;

namespace eKitap.Models
{
    public class eKitapDbContext:DbContext
    {
        public eKitapDbContext(DbContextOptions<eKitapDbContext> options):base(options)
        {
        }

        public DbSet<Book> Kitaplar { get; set; }
        public DbSet<Admin> AdminUsers { get; set; }
        public DbSet<eKitap.Models.ClassRoom> ClassRoom { get; set; }
        public DbSet<eKitap.Models.Student> Student { get; set; }
        public DbSet<eKitap.Models.BookStudentConnection> BookStudentConnections { get; set; }


    }
}
