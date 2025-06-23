using DotNetApi.Models;

namespace DotNetApi.Data
{
  public interface IUserRepository
  {
    public bool SaveChanges();

    public void AddEntity<T>(T entityToAdd);

    public void RemoveEntity<T>(T entityToRemove);

    public IEnumerable<User> GetUsers();

    public User GetSingleUser(int userId);

    public IEnumerable<UserSalary> GetUserSalaries();

    public UserSalary GetSingleUserSalary(int userId);

    public IEnumerable<UserJobInfo> GetUserJobInfos();

    public UserJobInfo GetSingleUserJobInfo(int userId);
  }
}