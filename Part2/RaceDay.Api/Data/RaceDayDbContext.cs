using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{//start of namespace

    //the DbContext connects the models to the SQL Server database
    public class RaceDayDbContext : DbContext
    {//start of class

        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options)
        {
        }

        //each DbSet is a table
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Enrolment> Enrolments { get; set; }
        public DbSet<Result> Results { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {//start of method
            base.OnModelCreating(modelBuilder);

            //seed the two roles (same as the SQL script)
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Organiser" },
                new Role { RoleId = 2, RoleName = "Participant" }
            );

            //email must be unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("UQ_Users_Email");

            //a participant can only enrol once per category
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.ParticipantId, e.CategoryId })
                .IsUnique()
                .HasDatabaseName("UQ_Enrolments_Participant_Category");

            //one result per enrolment
            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentId)
                .IsUnique()
                .HasDatabaseName("UQ_Results_Enrolment");

            //default values from the SQL script
            modelBuilder.Entity<User>().Property(u => u.CreatedAt).HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Event>().Property(e => e.CreatedAt).HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Enrolment>().Property(e => e.EnrolmentDate).HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Enrolment>().Property(e => e.Status).HasDefaultValue("Pending");
            modelBuilder.Entity<Result>().Property(r => r.RecordedAt).HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Category>().Property(c => c.Price).HasDefaultValue(0m);
            modelBuilder.Entity<Category>().Property(c => c.MaxParticipants).HasDefaultValue(100);

            //stops the "multiple cascade paths" error in SQL Server
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.Participant)
                .WithMany()
                .HasForeignKey(e => e.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany()
                .HasForeignKey(e => e.OrganiserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

        }//end of method

    }//end of class
}//end of namespace