﻿// <auto-generated />
using System;
using IpLogsCommon.Repository.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IpLogsCommon.Repository.Context.Migrations.IpLogsDb
{
    [DbContext(typeof(IpLogsDbContext))]
    [Migration("20240927103847_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("IpLogsCommon.Repository.Entities.User", b =>
                {
                    b.Property<long>("Id")
                        .HasColumnType("bigint");

                    b.Property<string>("IPAddress")
                        .IsRequired()
                        .HasColumnType("VARCHAR(45)");

                    b.Property<DateTime>("LastConnectionTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("IpLogsCommon.Repository.Entities.UserIP", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("ConnectionTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("IPAddress")
                        .IsRequired()
                        .HasColumnType("VARCHAR(45)");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserIPs");
                });

            modelBuilder.Entity("IpLogsCommon.Repository.Entities.UserIP", b =>
                {
                    b.HasOne("IpLogsCommon.Repository.Entities.User", "User")
                        .WithMany("UserIPs")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("IpLogsCommon.Repository.Entities.User", b =>
                {
                    b.Navigation("UserIPs");
                });
#pragma warning restore 612, 618
        }
    }
}
