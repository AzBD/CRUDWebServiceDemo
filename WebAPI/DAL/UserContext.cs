using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiTestDb.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebAPI.DAL
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public UserContext() : base() {  }

        public UserContext(DbContextOptions<UserContext> options) : base(options) { }
    }
}
