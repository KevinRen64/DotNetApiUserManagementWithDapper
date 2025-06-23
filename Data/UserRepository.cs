using DotNetApi.Models;

namespace DotNetApi.Data
{
  // This class implements the IUserRepository interface and handles 
  // all database operations using Entity Framework Core
  public class UserRepository : IUserRepository
  {
    private readonly DataContextEF _entityFramework;   // EF Core data context used to interact with the database


    // Constructor sets up the data context with configuration (e.g., connection string)
    public UserRepository(IConfiguration config)
    {
      _entityFramework = new DataContextEF(config);
    }


    // Saves all changes made to the database.
    // Returns true if at least one row was affected.
    public bool SaveChanges()
    {
      return _entityFramework.SaveChanges() > 0;
    }


    // Generic method to add any entity (e.g., User, UserSalary) to the database
    public void AddEntity<T>(T entityToAdd)
    {
      if (entityToAdd != null)
      {
        _entityFramework.Add(entityToAdd);
      }
    }


    // Generic method to remove any entity from the database
    public void RemoveEntity<T>(T entityToRemove)
    {
      if (entityToRemove != null)
      {
        _entityFramework.Remove(entityToRemove);
      }
    }


    // Retrieves a list of all users from the Users table
    public IEnumerable<User> GetUsers()
    {
      IEnumerable<User> users = _entityFramework.Users.ToList<User>();
      return users;
    }


    // Retrieves a single user based on their userId
    public User GetSingleUser(int userId)
    {
      User? user = _entityFramework.Users
          .Where(u => u.UserId == userId)
          .FirstOrDefault<User>();

      if (user != null)
      {
        return user;
      }
      throw new Exception("Failed to Get User");
    }


    // Retrieves a list of all user salaries from the UserSalary table
    public IEnumerable<UserSalary> GetUserSalaries()
    {
      IEnumerable<UserSalary> userSalaries = _entityFramework.UserSalary.ToList<UserSalary>();
      return userSalaries;
    }


    // Retrieves salary data for a single user based on their userId
    public UserSalary GetSingleUserSalary(int userId)
    {
      UserSalary? userSalary = _entityFramework.UserSalary
          .Where(u => u.UserId == userId)
          .FirstOrDefault<UserSalary>();
      if (userSalary != null)
      {
        return userSalary;
      }
      throw new Exception("Failed to Get User");
    }


    // Retrieves a list of all users from the Users table
    public IEnumerable<UserJobInfo> GetUserJobInfos()
    {
      IEnumerable<UserJobInfo> userJobInfos = _entityFramework.UserJobInfo.ToList<UserJobInfo>();
      return userJobInfos;
    }


    // Retrieves salary data for a single user based on their userId
    public UserJobInfo GetSingleUserJobInfo(int userId)
    {
      UserJobInfo? userJobInfo = _entityFramework.UserJobInfo
          .Where(u => u.UserId == userId)
          .FirstOrDefault<UserJobInfo>();
      if (userJobInfo != null)
      {
        return userJobInfo;
      }
      throw new Exception("Failed to Get User");
    }

  }
}