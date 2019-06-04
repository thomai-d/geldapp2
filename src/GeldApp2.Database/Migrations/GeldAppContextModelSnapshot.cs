﻿// <auto-generated />
using System;
using GeldApp2.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GeldApp2.Database.Migrations
{
    [DbContext(typeof(GeldAppContext))]
    partial class GeldAppContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.1-servicing-10028")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("GeldApp2.Database.Account", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("GeldApp2.Database.Category", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AccountId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("GeldApp2.Database.Expense", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("AccountId");

                    b.Property<decimal>("Amount");

                    b.Property<string>("Category");

                    b.Property<DateTimeOffset>("Created");

                    b.Property<string>("CreatedBy");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Details");

                    b.Property<DateTimeOffset>("LastModified");

                    b.Property<string>("LastModifiedBy");

                    b.Property<string>("Subcategory");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.ToTable("Expenses");
                });

            modelBuilder.Entity("GeldApp2.Database.Subcategory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CategoryId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Subcategories");
                });

            modelBuilder.Entity("GeldApp2.Database.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("LastLogin");

                    b.Property<DateTimeOffset>("LastPasswordChange");

                    b.Property<string>("Name");

                    b.Property<string>("Password");

                    b.Property<string>("RefreshToken");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("GeldApp2.Database.UserAccount", b =>
                {
                    b.Property<long>("UserId");

                    b.Property<long>("AccountId");

                    b.HasKey("UserId", "AccountId");

                    b.HasIndex("AccountId");

                    b.ToTable("UserAccount");
                });

            modelBuilder.Entity("GeldApp2.Database.Category", b =>
                {
                    b.HasOne("GeldApp2.Database.Account")
                        .WithMany("Categories")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GeldApp2.Database.Expense", b =>
                {
                    b.HasOne("GeldApp2.Database.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GeldApp2.Database.Subcategory", b =>
                {
                    b.HasOne("GeldApp2.Database.Category")
                        .WithMany("Subcategories")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("GeldApp2.Database.UserAccount", b =>
                {
                    b.HasOne("GeldApp2.Database.Account", "Account")
                        .WithMany("UserAccounts")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("GeldApp2.Database.User", "User")
                        .WithMany("UserAccounts")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
