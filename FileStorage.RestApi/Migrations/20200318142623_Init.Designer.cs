﻿// <auto-generated />
using FileStorage.RestApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace FileStorage.RestApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20200318142623_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("FileStorage.RestApi.Models.File", b =>
                {
                    b.Property<int>("FileId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("file_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("FilePath")
                        .IsRequired()
                        .HasColumnName("file_path")
                        .HasColumnType("text");

                    b.Property<int>("UserId")
                        .HasColumnName("user_id")
                        .HasColumnType("integer");

                    b.HasKey("FileId")
                        .HasName("pk_files");

                    b.HasIndex("UserId")
                        .HasName("ix_files_user_id");

                    b.ToTable("files");
                });

            modelBuilder.Entity("FileStorage.RestApi.Models.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("user_id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnName("email")
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnName("password")
                        .HasColumnType("text");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasColumnName("user_name")
                        .HasColumnType("text");

                    b.HasKey("UserId")
                        .HasName("pk_users");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasName("ix_users_email");

                    b.HasIndex("UserName")
                        .IsUnique()
                        .HasName("ix_users_user_name");

                    b.ToTable("users");
                });

            modelBuilder.Entity("FileStorage.RestApi.Models.File", b =>
                {
                    b.HasOne("FileStorage.RestApi.Models.User", "User")
                        .WithMany("Files")
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_files_users_user_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
