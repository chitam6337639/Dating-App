﻿// <auto-generated />
using System;
using CSDL.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CSDL.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("CSDL.Models.Account", b =>
                {
                    b.Property<string>("accountId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("accessToken")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("userId")
                        .HasColumnType("int");

                    b.HasKey("accountId");

                    b.HasIndex("userId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("CSDL.Models.History", b =>
                {
                    b.Property<int>("activityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("activityId"), 1L, 1);

                    b.Property<int>("MatchId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("time")
                        .HasColumnType("datetime2");

                    b.Property<string>("type")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("activityId");

                    b.HasIndex("MatchId");

                    b.HasIndex("UserId");

                    b.ToTable("Histories");
                });

            modelBuilder.Entity("CSDL.Models.Image", b =>
                {
                    b.Property<int>("imageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("imageId"), 1L, 1);

                    b.Property<string>("ImageURL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("userId")
                        .HasColumnType("int");

                    b.HasKey("imageId");

                    b.HasIndex("userId");

                    b.ToTable("Images");
                });

            modelBuilder.Entity("CSDL.Models.Match", b =>
                {
                    b.Property<int>("matchId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("matchId"), 1L, 1);

                    b.Property<int>("TargetUserId")
                        .HasColumnType("int");

                    b.Property<int>("UserId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("time")
                        .HasColumnType("datetime2");

                    b.HasKey("matchId");

                    b.HasIndex("TargetUserId");

                    b.HasIndex("UserId");

                    b.ToTable("Matches");
                });

            modelBuilder.Entity("CSDL.Models.Message", b =>
                {
                    b.Property<int>("messageId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("messageId"), 1L, 1);

                    b.Property<int>("UserIdFrom")
                        .HasColumnType("int");

                    b.Property<int>("UserIdTo")
                        .HasColumnType("int");

                    b.Property<string>("content")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("timeSent")
                        .HasColumnType("datetime2");

                    b.HasKey("messageId");

                    b.HasIndex("UserIdFrom");

                    b.HasIndex("UserIdTo");

                    b.ToTable("Messages");
                });

            modelBuilder.Entity("CSDL.Models.User", b =>
                {
                    b.Property<int>("userId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("userId"), 1L, 1);

                    b.Property<string>("ImageURL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("bio")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("birthday")
                        .HasColumnType("datetime2");

                    b.Property<string>("gender")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("userId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("CSDL.Models.Account", b =>
                {
                    b.HasOne("CSDL.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("userId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("CSDL.Models.History", b =>
                {
                    b.HasOne("CSDL.Models.Match", "Match")
                        .WithMany()
                        .HasForeignKey("MatchId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CSDL.Models.User", "User")
                        .WithMany("Histories")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Match");

                    b.Navigation("User");
                });

            modelBuilder.Entity("CSDL.Models.Image", b =>
                {
                    b.HasOne("CSDL.Models.User", "User")
                        .WithMany("Images")
                        .HasForeignKey("userId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("CSDL.Models.Match", b =>
                {
                    b.HasOne("CSDL.Models.User", "TargetUser")
                        .WithMany()
                        .HasForeignKey("TargetUserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CSDL.Models.User", "User")
                        .WithMany("Matches")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("TargetUser");

                    b.Navigation("User");
                });

            modelBuilder.Entity("CSDL.Models.Message", b =>
                {
                    b.HasOne("CSDL.Models.User", "UserFrom")
                        .WithMany()
                        .HasForeignKey("UserIdFrom")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CSDL.Models.User", "UserTo")
                        .WithMany("Messages")
                        .HasForeignKey("UserIdTo")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("UserFrom");

                    b.Navigation("UserTo");
                });

            modelBuilder.Entity("CSDL.Models.User", b =>
                {
                    b.Navigation("Histories");

                    b.Navigation("Images");

                    b.Navigation("Matches");

                    b.Navigation("Messages");
                });
#pragma warning restore 612, 618
        }
    }
}
