using Microsoft.EntityFrameworkCore;
using FantasyGolf.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyGolf.Core
{
    public class Database : DbContext
    {

        public DbSet<Player> Players { get; set; }
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<TournamentResult> TournamentResults { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=fantasy.database;Trusted_Connection=True;");

           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TournamentResult>().HasKey(t => new { t.PlayerId, t.Year, t.TournamentId });
        }

    }
}
