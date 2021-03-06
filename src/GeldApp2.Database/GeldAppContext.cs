﻿using GeldApp2.Database.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GeldApp2.Database
{
    public class GeldAppContext : DbContext
    {
        public GeldAppContext(DbContextOptions<GeldAppContext> opt)
            : base(opt)
        {
        }
        
        /* Tables */

        public DbSet<User> Users { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Subcategory> Subcategories { get; set; }

        public DbSet<Expense> Expenses { get; set; }
        
        public DbSet<ImportedExpense> ImportedExpenses { get; set; }

        /* Queries */

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Custom queries.
            modelBuilder.Query<LabelledChartItem>();
            modelBuilder.Query<MonthlyDataItem>();
            modelBuilder.Query<KeyValueItem>();
            modelBuilder.Query<AccountSummary>();

            modelBuilder.Entity<ImportedExpense>()
                        .HasOne(i => i.Expense)
                        .WithMany()
                        .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<UserAccount>()
                .HasKey(u => new { u.UserId, u.AccountId });

            modelBuilder.Entity<UserAccount>()
                .HasOne(u => u.User)
                .WithMany(u => u.UserAccounts)
                .HasForeignKey(u => u.UserId);

            modelBuilder.Entity<UserAccount>()
                .HasOne(u => u.Account)
                .WithMany(u => u.UserAccounts)
                .HasForeignKey(u => u.AccountId);
        }
    }
}
