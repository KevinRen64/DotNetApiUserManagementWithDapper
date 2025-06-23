using DotNetApi.Models;
using Microsoft.EntityFrameworkCore;

namespace DotNetApi.Data
{
  
  // This class handles communication with the database using Entity Framework Core
  public class DataContextEF : DbContext
  {
    private readonly IConfiguration _config;  // For accessing appsettings.json (connection strings, etc.)

    // Constructor receives configuration via dependency injection
    public DataContextEF(IConfiguration config)  
    {
      _config = config;
    }

    // These properties represent the database tables
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserSalary> UserSalary { get; set; }
    public virtual DbSet<UserJobInfo> UserJobInfo { get; set; }


    // This method is called by EF Core to configure the database connection
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      // Ensure we only configure if it hasn't been done already
      if (!optionsBuilder.IsConfigured)
      {
        // Set up SQL Server connection with retry logic
        optionsBuilder
            .UseSqlServer(_config.GetConnectionString("DefaultConnection"),
              optionsBuilder => optionsBuilder.EnableRetryOnFailure());
      }
    }


    // This method is used to configure the model (tables, keys, schema, etc.)
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      // Set the default schema for the database
      modelBuilder.HasDefaultSchema("TutorialAppSchema");

      // Map the User class to the "Users" table with "UserId" as the primary key
      modelBuilder.Entity<User>()
          .ToTable("Users", "TutorialAppSchema")
          .HasKey(u => u.UserId);

      modelBuilder.Entity<UserSalary>()
          .HasKey(u => u.UserId);

      modelBuilder.Entity<UserJobInfo>()
          .HasKey(u => u.UserId);
    }
  }
}