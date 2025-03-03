using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Model;

namespace KredditAPI.Data
{
    public class EntryContext : DbContext
    {
        public DbSet<Entry> Entries { get; set; }
        public string DbPath { get; }

        public EntryContext()
        {
            DbPath = "bin/Kreddit.db";
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entry>().ToTable("Entries");
        }
    }
}