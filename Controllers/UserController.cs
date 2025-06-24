using DotNetApi.Data;
using DotNetApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using DotNetApi.Models;

namespace DotetApi.Controllers;

[ApiController]  // Indicates this class defines a Web API controller
[Route("[controller]")]  // Route will be based on the controller name: /User
public class UserController : ControllerBase
{

  // Instance of the Dapper data context for database operations
  DataContextDapper _dapper;

  // Constructor that takes IConfiguration to initialize Dapper with connection string
  public UserController(IConfiguration config)
  {
    _dapper = new DataContextDapper(config);
  }

  // Endpoint to get a list of all users
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
    // Executes the SQL and returns a list of users
    IEnumerable<User> users = _dapper.LoadData<User>(sql);
    return users;
  }


  // Endpoint to get a single user by userId
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
      WHERE UserId = " + userId.ToString();

    // Executes the SQL and returns a single user
    User user = _dapper.LoadDataSingle<User>(sql);
    return user;
  }


  // PUT endpoint to edit/update an existing user
  [HttpPut("EditUser")]
  public IActionResult EditUser(User user)
  {
    // SQL statement to update user data by UserId
    // WARNING: This uses string concatenation which is vulnerable to SQL Injection attacks
    string sql = @"
    UPDATE TutorialAppSchema.Users
      SET [FirstName] = '" + user.FirstName +
          "', [LastName] = '" + user.LastName +
          "', [Email] = '" + user.Email +
          "', [Gender] = '" + user.Gender +
          "',[Active] = '" + user.Active +
      "' WHERE UserId = " + user.UserId;

    // Execute the update statement
    if (_dapper.ExecuteSql(sql))
    {
      // Return HTTP 200 OK if update succeeded
      return Ok();
    }
    // Throw exception if update failed
    throw new Exception("Failed to Update User");
  }


  // POST endpoint to add a new user
  [HttpPost("AddUser")]
  public IActionResult AddUser(UserToAddDto user)
  {

    // SQL statement to insert a new user record
    // WARNING: This uses string concatenation which is vulnerable to SQL Injection attacks
    string sql = @"INSERT INTO TutorialAppSchema.Users (
            [FirstName],
            [LastName],
            [Email],
            [Gender],
            [Active] 
          ) VALUES ( " +
          "'" + user.FirstName +
          "', '" + user.LastName +
          "', '" + user.Email +
          "', '" + user.Gender +
          "', '" + user.Active +
      "')";

    // Execute the insert statement
    if (_dapper.ExecuteSql(sql))
    {
      // Return HTTP 200 OK if insert succeeded
      return Ok();
    }
    // Throw exception if insert failed
    throw new Exception("Failed to Add User");
  }


  // SQL statement to delete a user by userId
  // WARNING: This uses string concatenation which is vulnerable to SQL Injection attacks
  [HttpDelete("DeleteUser/{userId}")]
  public IActionResult RemoveUser(int userId)
  {
    string sql = @"
      DELETE FROM TutorialAppSchema.Users
      WHERE UserId = " + userId.ToString();

    // Execute the remove statement
    if (_dapper.ExecuteSql(sql))
    {
      // Return HTTP 200 OK if insert succeeded
      return Ok();
    }
    // Throw exception if insert failed
    throw new Exception("Failed to Remove User");
  }
}

