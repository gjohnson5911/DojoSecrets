using System;
using Microsoft.EntityFrameworkCore;

namespace Secrets.Models
{
    public class DojoContext:DbContext
    {
        public DojoContext(DbContextOptions<DojoContext> options):base(options){}

        public DbSet<User> Users {get; set;}
        public DbSet<Secret> Secrets {get;set;}  
        public DbSet<Like> Likes {get; set;}
    }
}