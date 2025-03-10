﻿// <auto-generated />
using System;
using AtmChallenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AtmChallenge.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20250306171010_FirstMigration")]
    partial class FirstMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AtmChallenge.Domain.Entities.Card.Card", b =>
                {
                    b.Property<string>("NumberHash")
                        .HasColumnType("text");

                    b.Property<double>("Balance")
                        .HasColumnType("double precision");

                    b.Property<int>("FailedLoginAttempts")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("LockoutEnd")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Network")
                        .HasColumnType("integer");

                    b.Property<string>("PinHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("NumberHash");

                    b.HasIndex("UserId");

                    b.ToTable("Cards");
                });

            modelBuilder.Entity("AtmChallenge.Domain.Entities.Card.CardTransaction", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<double>("Amount")
                        .HasColumnType("double precision");

                    b.Property<string>("AtmLocation")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("CardNumberHash")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("Date")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("IdempotencyKey")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("CardNumberHash");

                    b.ToTable("CardTransactions");
                });

            modelBuilder.Entity("AtmChallenge.Domain.Entities.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("DocumentNumber")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("AtmChallenge.Domain.Entities.Card.Card", b =>
                {
                    b.HasOne("AtmChallenge.Domain.Entities.User", "User")
                        .WithMany("Cards")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("AtmChallenge.Domain.Entities.Card.CardTransaction", b =>
                {
                    b.HasOne("AtmChallenge.Domain.Entities.Card.Card", "Card")
                        .WithMany("Transactions")
                        .HasForeignKey("CardNumberHash")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Card");
                });

            modelBuilder.Entity("AtmChallenge.Domain.Entities.Card.Card", b =>
                {
                    b.Navigation("Transactions");
                });

            modelBuilder.Entity("AtmChallenge.Domain.Entities.User", b =>
                {
                    b.Navigation("Cards");
                });
#pragma warning restore 612, 618
        }
    }
}
