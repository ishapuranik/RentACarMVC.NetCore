﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Bookings.Shared.Persistance;
using System.Reflection;

namespace Bookings.Persistence
{
    public class RentACarDBContext : DbContext
    {
        private readonly IConfiguration _configuration;
        public RentACarDBContext(
            DbContextOptions<RentACarDBContext> dbContextOptions,
            IConfiguration configuration)
            : base(dbContextOptions)
        {
            _configuration = configuration;
        }

        public RentACarDBContext(
            DbContextOptions<RentACarDBContext> dbContextOptions)
            : base(dbContextOptions)
        {

        }

        public RentACarDBContext()
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString(DbConfig.CONNECTION_STRING_DB));
        }
    }
}
