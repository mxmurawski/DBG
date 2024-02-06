﻿// <auto-generated />
using System;
using DBG.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DBG.Infrastructure.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("DBG.Infrastructure.Models.Db.DbDynamicState", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("ConnectionsCount")
                        .HasColumnType("int");

                    b.Property<string>("DbAndTableSizesAsJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("DbSystemEntryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("DbSystemEntryId");

                    b.ToTable("db_dynamic_states", "db_states");
                });

            modelBuilder.Entity("DBG.Infrastructure.Models.Db.DbStaticState", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("DbSystemEntryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("MaxConnectionsCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("Version")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("DbSystemEntryId");

                    b.ToTable("db_static_states", "db_states");
                });

            modelBuilder.Entity("DBG.Infrastructure.Models.Db.DbSystemEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DbType")
                        .HasColumnType("int");

                    b.Property<int>("Port")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("db_system_entries", "core");
                });

            modelBuilder.Entity("DBG.Infrastructure.Models.Db.OsDynamicState", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("CpuUsage")
                        .HasColumnType("float");

                    b.Property<string>("DiskUsageAsJson")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OsSystemEntryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<double>("RamUsage")
                        .HasColumnType("float");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("OsSystemEntryId");

                    b.ToTable("os_dynamic_states", "os_states");
                });

            modelBuilder.Entity("DBG.Infrastructure.Models.Db.OsStaticState", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("CpuCount")
                        .HasColumnType("int");

                    b.Property<Guid>("OsSystemEntryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("RamCount")
                        .HasColumnType("int");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("datetime2");

                    b.Property<string>("Uptime")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Version")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("OsSystemEntryId");

                    b.ToTable("os_static_states", "os_states");
                });

            modelBuilder.Entity("DBG.Infrastructure.Models.Db.OsSystemEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("OsType")
                        .HasColumnType("int");

                    b.Property<int>("Port")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("os_system_entries", "core");
                });

            modelBuilder.Entity("DBG.Infrastructure.Models.Db.SystemEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("DbEntryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("OsEntryId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("DbEntryId");

                    b.HasIndex("OsEntryId");

                    b.ToTable("system_entries", "core");
                });

            modelBuilder.Entity("DBG.Infrastructure.Models.Db.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RefreshToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("RefreshTokenExpiryTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<DateTime?>("UpdatedOn")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("Email")
                        .IsUnique();

                    b.ToTable("users", "core");

                    b.HasData(
                        new
                        {
                            Id = new Guid("05988d8d-4ce8-48b9-b524-713d1c0758c0"),
                            CreatedOn = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "admin@admin.com",
                            Password = "C7AD44CBAD762A5DA0A452F9E854FDC1E0E7A52A38015F23F3EAB1D80B931DD472634DFAC71CD34EBC35D16AB7FB8A90C81F975113D6C7538DC69DD8DE9077EC",
                            Role = 0
                        },
                        new
                        {
                            Id = new Guid("4b65c84c-933a-47e2-87bb-cc85642e3e1e"),
                            CreatedOn = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            Email = "viewer@admin.com",
                            Password = "A8D73E712D9257A75BCE54754E0AD3074894E29FEEEC1709F9E47B761DC38D7AB923A62F1B4883A19569115E8B68850CC86B27FDA81A0DAA5305538E4D910168",
                            Role = 1
                        });
                });

            modelBuilder.Entity("DBG.Infrastructure.Models.Db.SystemEntry", b =>
                {
                    b.HasOne("DBG.Infrastructure.Models.Db.DbSystemEntry", "DbEntry")
                        .WithMany()
                        .HasForeignKey("DbEntryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DBG.Infrastructure.Models.Db.OsSystemEntry", "OsEntry")
                        .WithMany()
                        .HasForeignKey("OsEntryId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DbEntry");

                    b.Navigation("OsEntry");
                });
#pragma warning restore 612, 618
        }
    }
}
