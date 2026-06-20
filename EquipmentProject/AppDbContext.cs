using SharedEquipment.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EquipmentProject
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> option) : base(option)
        {}
        public DbSet<RentalContract> RentalContracts { get; set; }
        public DbSet<RentalDetail> RentalDetails { get; set; }

        public DbSet<Equipment> Equipments { get; set; }

        public DbSet<Customer> Customers { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Maintenance> Maintenances { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Payment> payments { get; set; }
    }
}
