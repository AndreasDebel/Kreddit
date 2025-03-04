using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using shared.Model;

namespace Data
{
    public class PostContext : DbContext
    {
        public DbSet<Post> Posts { get; set; }
        public string DbPath { get; }


        public PostContext(DbContextOptions<PostContext> options) : base(options)
        {
        }

        //public PostContext()
        //{
        //    DbPath = "bin/Kreddit.db";
        //}

        //protected override void OnConfiguring(DbContextOptionsBuilder options)
        //    => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>().ToTable("Posts");
        }
    }
}