using UserManagement.Data;
using UserManagement.Dtos;
using Microsoft.AspNetCore.Mvc;
using UserManagement.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace UserManagement.Controllers;

[ApiController]  // Indicates this class defines a Web API controller
[Route("[controller]")]  // Route will be based on the controller name: /User
public class UserController : ControllerBase
{

  
  private readonly DataContextDapper _dapper;  // Dapper context instance for database operations

  // Constructor that takes IConfiguration to initialize Dapper with connection string
  public UserController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
  }


  // ================================
  // GET: /User/GetUsers
  // Retrieves a list of all users
  // ================================
  [HttpGet("GetUsers")]
  public IEnumerable<User> GetUsers()
  {
    string sql = @"
      SELECT [UserId],
        [FirstName],
        [LastName],
        [Email],
        [Gender],
        [Active] 
      FROM TutorialAppSchema.Users";
    // Executes the SQL and returns a list of user objects
    IEnumerable<User> users = _dapper.LoadData<User>(sql);
    return users;
  }


  // ========================================
  // GET: /User/GetUsers/{userId}
  // Retrieves a single user by their UserId
  // ========================================
  [HttpGet("GetUsers/{userId}")]
  public User GetSingleUser(int userId)
  {
    string sql = @"
      SELECT [UserId],
        [FirstName],
        [LastName],
        [Email],
        [Gender],
        [Active] 
      FROM TutorialAppSchema.Users
      WHERE UserId = @UserId";

    var userIdParam = new { UserId = userId };
    // Use parameterized query to safely retrieve a single user
    return _dapper.LoadDataSingleWithParameters<User>(sql, userIdParam);
  }


  // ===========================================
  // PUT: /User/EditUser
  // Updates an existing user's data by UserId
  // ===========================================
  [HttpPut("EditUser")]
  public IActionResult EditUser(User user)
  {
    // SQL statement to update user data by UserId
    string sql = @"
    UPDATE TutorialAppSchema.Users
      SET [FirstName] = @FirstName,
          [LastName] = @LastName,
          [Email] = @Email,
          [Gender] = @Gender,
          [Active] = @Active
      WHERE UserId = @UserId";

    // Build parameter list using safe SqlParameter objects
    var putParameter = new List<SqlParameter>
      {
        new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = user.FirstName},
        new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = user.LastName},
        new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email },
        new SqlParameter("@Gender", SqlDbType.NVarChar) { Value = user.Gender },
        new SqlParameter("@Active", SqlDbType.NVarChar) { Value = user.Active },
        new SqlParameter("@UserId", SqlDbType.Int) { Value = user.UserId }
      };    

    // Execute the update statement and return result
      if (_dapper.ExecuteSqlWithParameters(sql, putParameter))
      {
        return Ok();
      }
      throw new Exception("Failed to update Post");
  }


  // ============================================
  // POST: /User/AddUser
  // Adds a new user using a DTO (UserToAddDto)
  // ============================================
  [HttpPost("AddUser")]
  public IActionResult AddUser(UserToAddDto user)
  {

    string sql = @"INSERT INTO TutorialAppSchema.Users (
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active] 
          ) VALUES ( 
            @FirstName,
            @LastName,
            @Email,
            @Gender,
            @Active
          )";

    // Create SQL parameters from the DTO
    var postParameter = new List<SqlParameter>
      {
        new SqlParameter("@FirstName", SqlDbType.NVarChar) { Value = user.FirstName},
        new SqlParameter("@LastName", SqlDbType.NVarChar) { Value = user.LastName},
        new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email },
        new SqlParameter("@Gender", SqlDbType.NVarChar) { Value = user.Gender },
        new SqlParameter("@Active", SqlDbType.NVarChar) { Value = user.Active },
      };   

    // Execute the insert statement and return result
      if (_dapper.ExecuteSqlWithParameters(sql, postParameter))
      {
        return Ok();
      }
      throw new Exception("Failed to add Post");
  }


  // ========================================
  // DELETE: /User/DeleteUser/{userId}
  // Deletes a user by their UserId
  // ========================================
  [HttpDelete("DeleteUser/{userId}")]
  public IActionResult RemoveUser(int userId)
  {
    string sql = @"
      DELETE FROM TutorialAppSchema.Users
      WHERE UserId = @UserId";

    var userIdParam = new { UserId = userId };

    // Execute the remove statement
    if (_dapper.ExecuteSqlWithSingleParameter(sql, userIdParam) > 0)
    {
      return Ok();  // 200 OK if successful
    }
    throw new Exception("Failed to Remove User");
  }
}

