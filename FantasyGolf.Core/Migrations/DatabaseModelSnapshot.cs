using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using FantasyGolf.Core;

namespace FantasyGolf.Core.Migrations
{
    [DbContext(typeof(Database))]
    partial class DatabaseModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("FantasyGolf.Core.Models.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.Property<string>("Link");

                    b.Property<string>("PGAIdNumber");

                    b.HasKey("Id");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("FantasyGolf.Core.Models.Tournament", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("OrderPlayed");

                    b.Property<string>("Url");

                    b.HasKey("Id");

                    b.ToTable("Tournaments");
                });

            modelBuilder.Entity("FantasyGolf.Core.Models.TournamentResult", b =>
                {
                    b.Property<int>("PlayerId");

                    b.Property<int>("Year");

                    b.Property<int>("TournamentId");

                    b.Property<decimal>("Money");

                    b.Property<string>("Position");

                    b.Property<int>("R1");

                    b.Property<int>("R2");

                    b.Property<int>("R3");

                    b.Property<int>("R4");

                    b.Property<int>("Total");

                    b.HasKey("PlayerId", "Year", "TournamentId");

                    b.HasIndex("PlayerId");

                    b.HasIndex("TournamentId");

                    b.ToTable("TournamentResults");
                });

            modelBuilder.Entity("FantasyGolf.Core.Models.TournamentResult", b =>
                {
                    b.HasOne("FantasyGolf.Core.Models.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("FantasyGolf.Core.Models.Tournament", "Tournament")
                        .WithMany()
                        .HasForeignKey("TournamentId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
