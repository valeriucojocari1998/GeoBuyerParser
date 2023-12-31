﻿// <auto-generated />
using System;
using GeoBuyerParser.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GeoBuyerParser.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "7.0.9");

            modelBuilder.Entity("GeoBuyerParser.Models.ExtendedCategory", b =>
                {
                    b.Property<string>("id")
                        .HasColumnType("TEXT");

                    b.Property<string>("categoryUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("marketId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("marketProvider")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("productCount")
                        .HasColumnType("INTEGER");

                    b.HasKey("id");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("GeoBuyerParser.Models.ExtendedProduct", b =>
                {
                    b.Property<string>("id")
                        .HasColumnType("TEXT");

                    b.Property<string>("brand")
                        .HasColumnType("TEXT");

                    b.Property<string>("categoryId")
                        .HasColumnType("TEXT");

                    b.Property<string>("categoryName")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("currentPrice")
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("dateCreated")
                        .HasColumnType("TEXT");

                    b.Property<string>("imageUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("marketId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("marketProvider")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<decimal?>("oldPrice")
                        .HasColumnType("TEXT");

                    b.Property<string>("priceLabel")
                        .HasColumnType("TEXT");

                    b.Property<string>("saleSpecification")
                        .HasColumnType("TEXT");

                    b.HasKey("id");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("GeoBuyerParser.Models.Spot", b =>
                {
                    b.Property<string>("id")
                        .HasColumnType("TEXT");

                    b.Property<string>("imageUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("latitude")
                        .HasColumnType("TEXT");

                    b.Property<string>("longitude")
                        .HasColumnType("TEXT");

                    b.Property<string>("name")
                        .HasColumnType("TEXT");

                    b.Property<string>("provider")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("url")
                        .HasColumnType("TEXT");

                    b.HasKey("id");

                    b.ToTable("Spots");
                });
#pragma warning restore 612, 618
        }
    }
}
