using ContactManager.Models;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ContactManager.Models
{
    public class NewContactManagerContext : DbContext
    {
        public DbSet<Contact> Contacts { get; set; }

    }
}